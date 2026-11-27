using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Network base for every LivingBeing that can cast abilities (players, enemies, minions).
///
/// How a cast travels:
///  1. The cast authority (the owner: a player's own client, or the host for creatures) runs the cast locally
///     right away, so it feels instant, then calls <see cref="ShareCast"/>.
///  2. A client sends the cast to the host. The host checks it and replays it. The host's copy is the only one
///     that applies gameplay effects (damage, heals, status effects; see CombatStatHandler).
///  3. The host relays the cast to every other client, which replays it so everyone sees the same projectiles,
///     auras, beams, conjured objects and so on.
///
/// Ability objects stay plain local GameObjects on every machine, so no ability prefab needs a NetworkObject
/// and clients never have to spawn network objects.
/// </summary>
public abstract class NetworkCombatant : NetworkBehaviour
{
    private AbilityHandler abilityHandler;

    private AbilityHandler Handler
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

    public void ShareCast(Ability ability, Vector3 targetPosition, Quaternion aimRotation, AbilityCastKind kind = AbilityCastKind.Cast, bool toggledOn = false)
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
            toggledOn = toggledOn
        };

        if (IsServer) BroadcastCastClientRpc(cast);
        else RequestCastServerRpc(cast);
    }

    /// <summary>Ends this being's toggled abilities (beams) on every machine.</summary>
    public void ShareToggledAbilitiesCleared()
    {
        if (!IsSpawned || !IsOwner) return;

        if (IsServer) BroadcastToggledAbilitiesClearedClientRpc();
        else RequestToggledAbilitiesClearedServerRpc();
    }

    /// <summary>Host-side check before a client's cast is accepted.</summary>
    protected virtual bool CanCastOnServer(Ability ability)
    {
        LivingBeing caster = GetComponent<LivingBeing>();
        return caster != null && !caster.Dead;
    }

    [ServerRpc]
    private void RequestCastServerRpc(AbilityCastData cast)
    {
        Ability ability = AbilityRegistry.AbilityAt(cast.abilityIndex);
        if (ability == null || !CanCastOnServer(ability)) return;

        Replay(cast, ability);
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

    private void Replay(AbilityCastData cast, Ability ability)
    {
        if (Handler == null) return;

        // This copy of the caster is interpolated and lags behind. Line it up with where the caster really was,
        // so the cast starts there and aims the same way. NetworkTransform takes over again on its next update.
        transform.SetPositionAndRotation(cast.casterPosition, cast.casterRotation);
        Handler.ReplayCast(ability, cast);
    }
}
