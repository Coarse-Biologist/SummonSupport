using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Network base for every LivingBeing that can cast abilities (players, enemies, minions).
///
/// How a cast travels:
///  1. The cast authority (the owner: a player's own client, or the host for creatures) runs the cast locally
///     right away, so it feels instant, then calls <see cref="ShareCast"/>.
///  2. A client sends the cast to the host. The host checks it (alive, off cooldown, enough power), replays it,
///     and charges its power. The host's copy is the only one that applies gameplay effects (damage, heals,
///     status effects; see CombatStatHandler), and the host's power value is the real one.
///  3. The host relays the cast to every other client, which replays it so everyone sees the same projectiles,
///     auras, beams, conjured objects and so on.
///
/// The owner pays a cast's power straight away too, so its power bar responds instantly. Every cast it sends
/// carries a sequence number; the host reports the last one it handled (see NetworkPlayer), and the owner takes
/// the power of casts the host hasn't handled yet off the host's value (<see cref="PendingCastCost"/>).
///
/// Ability objects stay plain local GameObjects on every machine, so no ability prefab needs a NetworkObject
/// and clients never have to spawn network objects.
/// </summary>
public abstract class NetworkCombatant : NetworkBehaviour
{
    [Tooltip("How early, in seconds, the host still accepts a client's cast before the ability's cooldown is over. Covers network jitter.")]
    [SerializeField] private float cooldownTolerance = 0.25f;
    [Tooltip("How much power a client may be short of an ability's cost on the host. Covers rounding and regeneration timing.")]
    [SerializeField] private float powerTolerance = 1f;

    private AbilityHandler abilityHandler;

    // Owner side: casts sent to the host that it hasn't handled yet, with the power they cost.
    private uint nextCastSequence;
    private readonly List<(uint sequence, float cost)> pendingCasts = new();

    // Host side: when each ability (by registry index) comes off cooldown for this being's client casts.
    private readonly Dictionary<int, float> serverCooldownEnds = new();

    /// <summary>Host side: the sequence number of the last cast this being's owner sent that the host has handled.</summary>
    public uint LastHandledCastSequence { get; private set; }

    protected AbilityHandler Handler
    {
        get
        {
            if (abilityHandler == null) abilityHandler = GetComponent<AbilityHandler>();
            return abilityHandler;
        }
    }

    /// <summary>True on the one machine that decides when this being casts.</summary>
    public bool HasCastAuthority => !IsSpawned || IsOwner;

    public override void OnNetworkSpawn()
    {
        // A client that joins while this being has a beam on never saw it start, so it asks the host.
        if (!IsServer && !IsOwner) RequestActiveBeamsServerRpc();
    }

    /// <param name="cost">The power the owner paid for this cast, so it can be predicted until the host confirms it.</param>
    public void ShareCast(Ability ability, Vector3 targetPosition, Quaternion aimRotation, AbilityCastKind kind = AbilityCastKind.Cast, bool toggledOn = false, float cost = 0f)
    {
        if (!IsSpawned || !IsOwner) return;

        int abilityIndex = AbilityRegistry.IndexOf(ability);
        if (abilityIndex < 0)
        {
            Debug.LogWarning($"{name} cast {ability.name}, but it is not in SetupManager.AllAbilities, so other players will not see it. Save the scene or run SummonSupport > Refresh Ability Registry to add it.");
            return;
        }

        AbilityCastData cast = new AbilityCastData
        {
            abilityIndex = abilityIndex,
            kind = kind,
            casterPosition = transform.position,
            casterRotation = transform.rotation,
            targetPosition = targetPosition,
            aimRotation = aimRotation,
            toggledOn = toggledOn,
            sequence = ++nextCastSequence
        };

        if (IsServer)
        {
            BroadcastCastClientRpc(cast);
            return;
        }

        if (cost > 0f) pendingCasts.Add((cast.sequence, cost));
        RequestCastServerRpc(cast);
    }

    /// <summary>
    /// Owner side: forgets casts up to <paramref name="handledSequence"/> and returns the power of the casts the
    /// host hasn't handled yet, which the host's power value doesn't include.
    /// </summary>
    public float PendingCastCost(uint handledSequence)
    {
        pendingCasts.RemoveAll(entry => entry.sequence <= handledSequence);
        float total = 0f;
        foreach (var entry in pendingCasts) total += entry.cost;
        return total;
    }

    /// <summary>Ends this being's toggled abilities (beams) on every machine.</summary>
    public void ShareToggledAbilitiesCleared()
    {
        if (!IsSpawned || !IsOwner) return;

        if (IsServer) BroadcastToggledAbilitiesClearedClientRpc();
        else RequestToggledAbilitiesClearedServerRpc();
    }

    /// <summary>Host-side check before a client's cast is accepted.</summary>
    protected virtual bool CanCastOnServer(Ability ability, AbilityCastData cast, out string reason)
    {
        reason = null;
        LivingBeing caster = GetComponent<LivingBeing>();
        if (caster == null || caster.Dead)
        {
            reason = "the caster is dead";
            return false;
        }

        // Triggered abilities (the movement dash) are free and have their own timing,
        // and turning a beam off must always go through so no copy is left with it on.
        if (cast.kind == AbilityCastKind.Trigger) return true;
        if (ability is BeamAbility beam && (!cast.toggledOn || (Handler != null && Handler.IsToggledOn(beam)))) return true;

        if (serverCooldownEnds.TryGetValue(cast.abilityIndex, out float readyAt) && Time.time < readyAt - cooldownTolerance)
        {
            reason = $"it is on cooldown for another {readyAt - Time.time:0.00}s";
            return false;
        }

        // Same rule the owner uses before casting (PlayerAbilityHandler.CheckAbilityUsePossible).
        float power = caster.GetAttribute(AttributeType.CurrentPower);
        if (power + powerTolerance < ability.Cost)
        {
            reason = $"it costs {ability.Cost} power and the caster has {power}";
            return false;
        }

        return true;
    }

    [ServerRpc]
    private void RequestCastServerRpc(AbilityCastData cast)
    {
        if (cast.sequence > LastHandledCastSequence) LastHandledCastSequence = cast.sequence;

        Ability ability = AbilityRegistry.AbilityAt(cast.abilityIndex);
        if (ability == null) return;

        if (!CanCastOnServer(ability, cast, out string reason))
        {
            // The owner already showed this cast; it gets its power back when it sees this cast handled.
            Debug.LogWarning($"Rejected {name}'s cast of {ability.name}: {reason}.");
            return;
        }

        bool usedAbility = Replay(cast, ability);
        if (usedAbility && cast.kind == AbilityCastKind.Cast && Handler != null)
        {
            Handler.PayReplayedCastCost(ability);
            serverCooldownEnds[cast.abilityIndex] = Time.time + Handler.GetCooldownDuration(ability);
        }

        BroadcastCastClientRpc(cast);
    }

    [ClientRpc]
    private void BroadcastCastClientRpc(AbilityCastData cast)
    {
        // The host already ran this cast, and the owner ran it when it was cast.
        if (IsServer || IsOwner) return;

        Ability ability = AbilityRegistry.AbilityAt(cast.abilityIndex);
        if (ability != null) Replay(cast, ability);
    }

    [ServerRpc]
    private void RequestToggledAbilitiesClearedServerRpc()
    {
        if (Handler != null) Handler.StopAllToggledAbilities();
        BroadcastToggledAbilitiesClearedClientRpc();
    }

    [ClientRpc]
    private void BroadcastToggledAbilitiesClearedClientRpc()
    {
        if (IsServer || IsOwner) return;
        if (Handler != null) Handler.StopAllToggledAbilities();
    }

    /// <summary>Host side: tells every client that a status effect started or ended on this being.</summary>
    public void ShareStatusEffect(StatusEffects effect, bool add)
    {
        if (!IsSpawned || !IsServer) return;

        int effectIndex = AbilityRegistry.IndexOf(effect);
        if (effectIndex < 0)
        {
            Debug.LogWarning($"{effect.name} on {name} is not in the status effect library, so clients will not see it.");
            return;
        }
        StatusEffectChangedClientRpc(effectIndex, add);
    }

    [ClientRpc]
    private void StatusEffectChangedClientRpc(int effectIndex, bool add)
    {
        if (IsServer) return;

        StatusEffects effect = AbilityRegistry.StatusEffectAt(effectIndex);
        if (effect != null && TryGetComponent(out StatusEffectHandler statusEffectHandler))
            statusEffectHandler.ApplyReplicatedStatusEffect(effect, add);
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestActiveBeamsServerRpc(ServerRpcParams rpcParams = default)
    {
        if (Handler == null) return;

        List<int> beamIndices = new List<int>();
        foreach (BeamAbility beam in Handler.ActiveBeams)
        {
            int index = AbilityRegistry.IndexOf(beam);
            if (index >= 0) beamIndices.Add(index);
        }
        if (beamIndices.Count == 0) return;

        ClientRpcParams toRequester = new ClientRpcParams
        {
            Send = new ClientRpcSendParams { TargetClientIds = new[] { rpcParams.Receive.SenderClientId } }
        };
        ActiveBeamsClientRpc(beamIndices.ToArray(), toRequester);
    }

    [ClientRpc]
    private void ActiveBeamsClientRpc(int[] beamIndices, ClientRpcParams rpcParams = default)
    {
        if (IsServer || IsOwner || Handler == null) return;

        foreach (int index in beamIndices)
        {
            if (AbilityRegistry.AbilityAt(index) is not BeamAbility beam) continue;

            // Beams follow the caster's ability spawn point, so the caster's current pose is all they need.
            AbilityCastData cast = new AbilityCastData
            {
                abilityIndex = index,
                kind = AbilityCastKind.Cast,
                casterPosition = transform.position,
                casterRotation = transform.rotation,
                targetPosition = transform.position,
                aimRotation = transform.rotation,
                toggledOn = true
            };
            Handler.ReplayCast(beam, cast);
        }
    }

    private bool Replay(AbilityCastData cast, Ability ability)
    {
        if (Handler == null) return false;

        // This copy of the caster is interpolated and lags behind. Line it up with where the caster really was,
        // so the cast starts there and aims the same way. NetworkTransform takes over again on its next update.
        transform.SetPositionAndRotation(cast.casterPosition, cast.casterRotation);
        return Handler.ReplayCast(ability, cast);
    }
}
