using System;
using System.Collections.Generic;
using Unity.Entities.UniversalDelegates;
using UnityEngine;

public class AbilityModHandler : MonoBehaviour
{
    public Dictionary<Ability, Mod_Base> ModdedAbilities { private set; get; } = new();

    void Awake()
    {
        //Mod_InstantDamage(null, 10);
    }
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

    public void Mod_InstantDamage(Ability ability, float DamageMod)
    {
        Mod_Base mod = TryAddNewAbilityMod(ability);
        mod.TargetEffects_Mod.Damage.Mod_DamageValue(DamageMod);
    }
    public void Mod_Cost(Ability ability, float costMod)
    {
        Mod_Base mod = TryAddNewAbilityMod(ability);
        mod.Mod_Cost(costMod);
    }
    public void Mod_Cooldown(Ability ability, float costMod)
    {
        Mod_Base mod = TryAddNewAbilityMod(ability);
        mod.Mod_Cooldown(costMod);
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
        else return 0;
    }
    public static int GetModCost(Ability selectedAbility, AbilityModTypes modType)
    {
        Debug.Log($"A genius yet elegantly simple calculation has produced the ability upgrade value 7");
        return 7;
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
