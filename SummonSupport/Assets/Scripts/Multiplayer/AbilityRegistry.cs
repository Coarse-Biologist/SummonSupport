using System;
using System.Collections.Generic;

/// <summary>
/// Maps abilities and status effects to indices that are identical on every machine, so they can be sent over the network.
/// Indices come from the lists configured on SetupManager, which every build shares.
/// </summary>
public static class AbilityRegistry
{
    public static int IndexOf(Ability ability)
    {
        List<Ability> abilities = SetupManager.Instance != null ? SetupManager.Instance.AllAbilities : null;
        return ability == null || abilities == null ? -1 : abilities.IndexOf(ability);
    }

    public static Ability AbilityAt(int index)
    {
        List<Ability> abilities = SetupManager.Instance != null ? SetupManager.Instance.AllAbilities : null;
        return abilities != null && index >= 0 && index < abilities.Count ? abilities[index] : null;
    }

    public static int IndexOf(StatusEffects effect)
    {
        if (effect == null || SetupManager.Instance == null) return -1;
        return Array.FindIndex(SetupManager.Instance.GetStatusEffectLibrary().entries, entry => entry.Effect == effect);
    }

    public static StatusEffects StatusEffectAt(int index)
    {
        if (SetupManager.Instance == null) return null;
        var entries = SetupManager.Instance.GetStatusEffectLibrary().entries;
        return index >= 0 && index < entries.Length ? entries[index].Effect : null;
    }
}
