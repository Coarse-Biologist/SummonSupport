using UnityEngine;

public static class NetworkAbilityMods
{
    public static void ModAttribute(Ability ability, AbilityModTypes modType, float changeValue)
    {
        if (!NetworkSession.IsOnline)
        {
            AbilityModHandler.Instance.ModAttributeByType(ability, modType, changeValue);
            return;
        }

        int abilityIndex = AbilityRegistry.IndexOf(ability);
        NetworkPlayer requester = PlayerStats.Instance.GetComponent<NetworkPlayer>();
        if (abilityIndex < 0 || requester == null)
        {
            Debug.LogWarning($"Mod for {ability.name} could not be shared; it is not in SetupManager.AllAbilities.");
            return;
        }
        requester.SubmitAttributeModServerRpc(abilityIndex, (int)modType, changeValue);
    }

    public static void AddStatusEffect(Ability ability, StatusEffects effect)
    {
        if (!NetworkSession.IsOnline)
        {
            AbilityModHandler.Instance.AddStatusEffectToAbility(ability, effect);
            return;
        }

        int abilityIndex = AbilityRegistry.IndexOf(ability);
        int effectIndex = AbilityRegistry.IndexOf(effect);
        NetworkPlayer requester = PlayerStats.Instance.GetComponent<NetworkPlayer>();
        if (abilityIndex < 0 || effectIndex < 0 || requester == null)
        {
            Debug.LogWarning($"Status effect mod for {ability.name} could not be shared; the ability or effect is not in the shared lists.");
            return;
        }
        requester.SubmitStatusModServerRpc(abilityIndex, effectIndex);
    }
}
