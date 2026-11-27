using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

/// <summary>One modded attribute of one ability, as sent to a client that joins.</summary>
public struct AbilityAttributeModData : INetworkSerializeByMemcpy
{
    public int abilityIndex;
    public AbilityModTypes modType;
    public float value;
}

/// <summary>One status effect added to one ability by a mod, as sent to a client that joins.</summary>
public struct AbilityStatusModData : INetworkSerializeByMemcpy
{
    public int abilityIndex;
    public int effectIndex;
}

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

    /// <summary>Host side: every mod made so far, for a client that joins.</summary>
    public static void CollectAllMods(out AbilityAttributeModData[] attributeMods, out AbilityStatusModData[] statusMods)
    {
        List<AbilityAttributeModData> attributes = new();
        List<AbilityStatusModData> statuses = new();

        foreach (KeyValuePair<Ability, Mod_Base> modded in AbilityModHandler.Instance.ModdedAbilities)
        {
            int abilityIndex = AbilityRegistry.IndexOf(modded.Key);
            if (abilityIndex < 0) continue;

            foreach (var attribute in modded.Value.Base_Mods)
            {
                float value = attribute.Value.Get();
                if (value != 0) attributes.Add(new AbilityAttributeModData { abilityIndex = abilityIndex, modType = attribute.Key, value = value });
            }
            foreach (StatusEffects effect in modded.Value.GetStatusEffects())
            {
                int effectIndex = AbilityRegistry.IndexOf(effect);
                if (effectIndex >= 0) statuses.Add(new AbilityStatusModData { abilityIndex = abilityIndex, effectIndex = effectIndex });
            }
        }

        attributeMods = attributes.ToArray();
        statusMods = statuses.ToArray();
    }

    /// <summary>Client side: replaces this machine's mods with the host's, which belong to everyone in the game.</summary>
    public static void ApplyAllMods(AbilityAttributeModData[] attributeMods, AbilityStatusModData[] statusMods)
    {
        AbilityModHandler modHandler = AbilityModHandler.Instance;
        modHandler.ClearMods();

        foreach (AbilityAttributeModData mod in attributeMods)
        {
            Ability ability = AbilityRegistry.AbilityAt(mod.abilityIndex);
            if (ability != null) modHandler.ModAttributeByType(ability, mod.modType, mod.value);
        }
        foreach (AbilityStatusModData mod in statusMods)
        {
            Ability ability = AbilityRegistry.AbilityAt(mod.abilityIndex);
            StatusEffects effect = AbilityRegistry.StatusEffectAt(mod.effectIndex);
            if (ability != null && effect != null && !modHandler.GetModStatusEffects(ability).Contains(effect))
                modHandler.AddStatusEffectToAbility(ability, effect);
        }
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
