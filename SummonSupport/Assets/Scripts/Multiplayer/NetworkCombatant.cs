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

    public void ShareCast(Ability ability, Vector3 targetPosition, Quaternion aimRotation, AbilityCastKind kind = AbilityCastKind.Cast, bool toggledOn = false)
    {
        if (!IsSpawned || !IsOwner) return;

        int abilityIndex = AbilityRegistry.IndexOf(ability);
        if (abilityIndex < 0)
        {
            Debug.LogWarning($"{name} cast {ability.name}, but it is not in SetupManager.AllAbilities, so other players will not see it.");
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

    private void Replay(AbilityCastData cast, Ability ability)
    {
        if (Handler == null) return;

        // This copy of the caster is interpolated and lags behind. Line it up with where the caster really was,
        // so the cast starts there and aims the same way. NetworkTransform takes over again on its next update.
        transform.SetPositionAndRotation(cast.casterPosition, cast.casterRotation);
        Handler.ReplayCast(ability, cast);
    }
}
