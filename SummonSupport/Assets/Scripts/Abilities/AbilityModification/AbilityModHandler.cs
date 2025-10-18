using System;
using System.Collections.Generic;
using Unity.Entities.UniversalDelegates;
using UnityEngine;

public class AbilityModHandler : MonoBehaviour
{
    public Dictionary<Ability, Mod_Base> ModdedAbilities { private set; get; } = new();
    public static readonly Dictionary<AbilityModTypes, int> ModIncrements = new()
    {
        { AbilityModTypes.Cost, -1 },
        { AbilityModTypes.Cooldown, -1 },
        { AbilityModTypes.Damage, 10 },
        { AbilityModTypes.DamageOverTime, 1 },
        { AbilityModTypes.Heal, 5 },
        { AbilityModTypes.HealOverTime, 1 },
        { AbilityModTypes.MaxPierce, 1 },
        { AbilityModTypes.MaxSplit, 1 },
        { AbilityModTypes.Duration, 1 }
    };


    public Mod_Base GetAbilityMod(Ability ability)
    {
        if (ModdedAbilities.TryGetValue(ability, out Mod_Base mod))
            return mod;
        else return null;
    }


    public Mod_Base TryAddNewAbilityMod(Ability ability)
    {
        if (!ModdedAbilities.TryGetValue(ability, out Mod_Base existingMod))
        {
            Mod_Base mod = new();
            ModdedAbilities.Add(ability, mod);
            return mod;
        }
        else return existingMod;
    }
    #region Specific mods
    public void Mod_InstantDamage(Ability ability, float modValue)
    {
        Mod_Base mod = TryAddNewAbilityMod(ability);
        mod.TargetEffects_Mod.Damage.Mod_DamageValue(modValue);
    }
    public void Mod_DotDamage(Ability ability, float modValue)
    {
        Mod_Base mod = TryAddNewAbilityMod(ability);
        mod.TargetEffects_Mod.DamageOverTime.Mod_DamageValue(modValue);
    }
    public void Mod_HealOverTime(Ability ability, float modValue)
    {
        Mod_Base mod = TryAddNewAbilityMod(ability);
        mod.TargetEffects_Mod.HealOverTime.Mod_HealValue(modValue);
    }
    public void Mod_Heal(Ability ability, float modValue)
    {
        Mod_Base mod = TryAddNewAbilityMod(ability);
        mod.TargetEffects_Mod.Heal.Mod_HealValue(modValue);
    }

    public void Mod_Cost(Ability ability, float modValue)
    {
        Mod_Base mod = TryAddNewAbilityMod(ability);
        mod.Mod_Cost(modValue);
    }
    public void Mod_Cooldown(Ability ability, float modValue)
    {
        Mod_Base mod = TryAddNewAbilityMod(ability);
        mod.Mod_Cooldown(modValue);
    }
    #endregion
    public void ModAttributeByType(Ability ability, AbilityModTypes modType, float changeValue)
    {
        if (modType == AbilityModTypes.Damage)
        {
            Mod_InstantDamage(ability, changeValue);
        }
        else if (modType == AbilityModTypes.DamageOverTime)
        {
            Mod_DotDamage(ability, changeValue);
        }
        else if (modType == AbilityModTypes.Heal)
        {
            Mod_Heal(ability, changeValue);
        }
        else if (modType == AbilityModTypes.HealOverTime)
        {
            Mod_HealOverTime(ability, changeValue);
        }
        else if (modType == AbilityModTypes.Cost)
        {
            Mod_Cost(ability, changeValue);
        }
        else if (modType == AbilityModTypes.Cooldown)
        {
            Mod_Cooldown(ability, changeValue);
        }
        else if (modType == AbilityModTypes.Cooldown)
        {
            Mod_Heal(ability, changeValue);
        }
        Debug.Log($"attribute type {modType} will be changed by {changeValue}");

    }

    public int GetModAttributeByType(Ability ability, AbilityModTypes modType)
    {
        if (!ModdedAbilities.TryGetValue(ability, out Mod_Base mod)) return 0;
        if (modType == AbilityModTypes.Damage)
        {
            return (int)mod.TargetEffects_Mod.Damage.Value;
        }
        if (modType == AbilityModTypes.Cost)
        {
            return (int)mod.Cost_Mod;
        }
        if (modType == AbilityModTypes.Cooldown)
        {
            return (int)mod.Cooldown_Mod;
        }
        else return 0;
    }

    public static int GetModCost(Ability selectedAbility, AbilityModTypes modType)
    {
        Debug.Log($"A genius yet elegantly simple calculation has produced the ability upgrade value 7");
        return 7;
    }
    public static int GetModIncrementValue(AbilityModTypes modType)
    {
        if (ModIncrements.TryGetValue(modType, out int num))
        {
            return num;
        }
        else return 0;
    }


    public static List<AbilityModTypes> GetModableAttributes(Ability ability)
    {
        switch (ability)
        {
            case ProjectileAbility projectile:
                return new List<AbilityModTypes>() { AbilityModTypes.MakePierce, AbilityModTypes.MaxPierce, AbilityModTypes.MakeSplit, AbilityModTypes.MaxSplit, AbilityModTypes.Cost, AbilityModTypes.Cooldown, AbilityModTypes.Damage, AbilityModTypes.DamageOverTime, AbilityModTypes.Heal, AbilityModTypes.HealOverTime };
            case TargetMouseAbility pointAndClickAbility:
                return new List<AbilityModTypes>() { AbilityModTypes.Cost, AbilityModTypes.Cooldown, AbilityModTypes.Damage, AbilityModTypes.DamageOverTime, AbilityModTypes.Heal, AbilityModTypes.HealOverTime };
            case ConjureAbility conjureAbility:
                return new List<AbilityModTypes>() { AbilityModTypes.Cost, AbilityModTypes.Cooldown, AbilityModTypes.Damage, AbilityModTypes.DamageOverTime, AbilityModTypes.Heal, AbilityModTypes.HealOverTime };
            case AuraAbility auraAbility:
                return new List<AbilityModTypes>() { AbilityModTypes.Cost, AbilityModTypes.Cooldown, AbilityModTypes.Damage, AbilityModTypes.DamageOverTime, AbilityModTypes.Heal, AbilityModTypes.HealOverTime };
            case TeleportAbility teleportAbility:
                return new List<AbilityModTypes>() { AbilityModTypes.Cost, AbilityModTypes.Cooldown, AbilityModTypes.Damage, AbilityModTypes.DamageOverTime, AbilityModTypes.Heal, AbilityModTypes.HealOverTime };
            case MeleeAbility meleeAbility:
                return new List<AbilityModTypes>() { AbilityModTypes.Cost, AbilityModTypes.Cooldown, AbilityModTypes.Damage, AbilityModTypes.DamageOverTime, AbilityModTypes.Heal, AbilityModTypes.HealOverTime };
            case BeamAbility beamAbility:
                return new List<AbilityModTypes>() { AbilityModTypes.Cost, AbilityModTypes.Cooldown, AbilityModTypes.Damage, AbilityModTypes.DamageOverTime, AbilityModTypes.Heal, AbilityModTypes.HealOverTime };
            case ChargeAbility chargeAbility:
                return new List<AbilityModTypes>() { AbilityModTypes.Range, AbilityModTypes.Cost, AbilityModTypes.Cooldown, AbilityModTypes.Damage, AbilityModTypes.DamageOverTime, AbilityModTypes.Heal, AbilityModTypes.HealOverTime };
            default:
                return new List<AbilityModTypes>() { AbilityModTypes.Cost, AbilityModTypes.Cooldown, AbilityModTypes.Damage, AbilityModTypes.DamageOverTime, AbilityModTypes.Heal, AbilityModTypes.HealOverTime };
        }
    }

    public static string GetAbilityModString(AbilityModTypes modEnum)
    {
        return System.Text.RegularExpressions.Regex.Replace(modEnum.ToString(), "(?<!^)([A-Z])", " $1");
    }
}
