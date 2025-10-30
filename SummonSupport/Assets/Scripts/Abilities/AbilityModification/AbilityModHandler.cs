using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Entities.UniversalDelegates;
using Unity.VisualScripting;
using UnityEngine;

public class AbilityModHandler : MonoBehaviour
{
    public Dictionary<Ability, Mod_Base> ModdedAbilities { private set; get; } = new();
    public static readonly List<AbilityModTypes> BoolMods = new();

    public static readonly Dictionary<AbilityModTypes, int> ModIncrements = new()

    {
        { AbilityModTypes.Cost, -1 },
        { AbilityModTypes.Cooldown, -1 },
        { AbilityModTypes.Damage, 10 },
        { AbilityModTypes.DamageOverTime, 1 },
        { AbilityModTypes.Heal, 5 },
        { AbilityModTypes.HealOverTime, 1 },
        { AbilityModTypes.Duration, 1 },
        { AbilityModTypes.Range, 1 },
        { AbilityModTypes.Radius, 1 },

        { AbilityModTypes.MaxPierce, 1 },
        { AbilityModTypes.MaxSplit, 1 },
        { AbilityModTypes.MaxRicochet, 1 },
        { AbilityModTypes.ProjectileNumber, 1 } };





    public static readonly Dictionary<AbilityModTypes, int> ModCosts = new()
    {
        { AbilityModTypes.Cost, 10 },
        { AbilityModTypes.Cooldown, 100 },
        { AbilityModTypes.Damage, 50 },
        { AbilityModTypes.DamageOverTime, 40 },
        { AbilityModTypes.Heal, 60 },
        { AbilityModTypes.HealOverTime, 50 },
        { AbilityModTypes.Duration, 80 },
        { AbilityModTypes.Range, 30 },
        { AbilityModTypes.Radius, 100 },
// projectile perks
        { AbilityModTypes.MaxPierce, 150 },
        { AbilityModTypes.MaxSplit, 150 },
        { AbilityModTypes.MaxRicochet, 150 },
        { AbilityModTypes.ProjectileNumber, 400 } };

    public static readonly Dictionary<Type, List<AbilityModTypes>> ModOptions = new()
        {
            {typeof(ProjectileAbility), new List<AbilityModTypes>() { AbilityModTypes.ProjectileNumber, AbilityModTypes.MaxPierce, AbilityModTypes.MaxSplit, AbilityModTypes.MaxRicochet, AbilityModTypes.Cost, AbilityModTypes.Cooldown, AbilityModTypes.Damage, AbilityModTypes.DamageOverTime, AbilityModTypes.Heal, AbilityModTypes.HealOverTime } },
            {typeof(TargetMouseAbility),new List<AbilityModTypes>() { AbilityModTypes.Cost, AbilityModTypes.Cooldown, AbilityModTypes.Damage, AbilityModTypes.DamageOverTime, AbilityModTypes.Heal, AbilityModTypes.HealOverTime } },
            {typeof(ConjureAbility), new List<AbilityModTypes>() { AbilityModTypes.Cost, AbilityModTypes.Cooldown, AbilityModTypes.Damage, AbilityModTypes.DamageOverTime, AbilityModTypes.Heal, AbilityModTypes.HealOverTime } },
            {typeof(AuraAbility), new List<AbilityModTypes>() { AbilityModTypes.Cost, AbilityModTypes.Cooldown, AbilityModTypes.Damage, AbilityModTypes.DamageOverTime, AbilityModTypes.Heal, AbilityModTypes.HealOverTime } },
            {typeof(TeleportAbility), new List<AbilityModTypes>() { AbilityModTypes.Cost, AbilityModTypes.Cooldown, AbilityModTypes.Damage, AbilityModTypes.DamageOverTime, AbilityModTypes.Heal, AbilityModTypes.HealOverTime } },
            {typeof(MeleeAbility), new List<AbilityModTypes>() { AbilityModTypes.Cost, AbilityModTypes.Cooldown, AbilityModTypes.Damage, AbilityModTypes.DamageOverTime, AbilityModTypes.Heal, AbilityModTypes.HealOverTime } },
            {typeof(BeamAbility), new List<AbilityModTypes>() { AbilityModTypes.Cost, AbilityModTypes.Cooldown, AbilityModTypes.Damage, AbilityModTypes.DamageOverTime, AbilityModTypes.Heal, AbilityModTypes.HealOverTime } },
            {typeof(ChargeAbility), new List<AbilityModTypes>() { AbilityModTypes.Range, AbilityModTypes.Cost, AbilityModTypes.Cooldown, AbilityModTypes.Damage, AbilityModTypes.DamageOverTime, AbilityModTypes.Heal, AbilityModTypes.HealOverTime }},
        };

    public static int GetModCost(AbilityModTypes modType)
    {
        if (ModCosts.TryGetValue(modType, out int cost))
        {
            return cost;
        }
        else throw new System.Exception($"The mod whose price you are searching {modType} is not in the Ability Mod cost dictionary!");
    }
    public Mod_Base GetAbilityMod(Ability ability)
    {
        if (ModdedAbilities.TryGetValue(ability, out Mod_Base mod))
            return mod;
        else return null;
    }


    public Mod_Base TryAddNewAbilityMod(Ability ability)
    {
        Mod_Base mod;
        if (!ModdedAbilities.TryGetValue(ability, out Mod_Base existingMod))
        {
            mod = new Mod_Base();
            ModdedAbilities.Add(ability, mod);
            return mod;
        }
        else return existingMod;
    }



    public void ModAttributeByType(Ability ability, AbilityModTypes modType, float changeValue)
    {
        Mod_Base mod = TryAddNewAbilityMod(ability);
        mod.Mod_Attribute(modType, changeValue);
    }



    public int GetModAttributeByType(Ability ability, AbilityModTypes modType)
    {
        if (!ModdedAbilities.TryGetValue(ability, out Mod_Base existingMod)) return 0;
        else return existingMod.GetModdedAttribute(modType);
    }


    public static int GetModIncrementValue(AbilityModTypes modType)
    {
        if (ModIncrements.TryGetValue(modType, out int num))
        {
            return num;
        }
        else return 0;
    }


    public List<AbilityModTypes> GetModableAttributes(Ability ability)
    {
        Mod_Base mod = TryAddNewAbilityMod(ability);

        if (ModOptions.TryGetValue(ability.GetType(), out List<AbilityModTypes> modOptions))
        {
            foreach (AbilityModTypes modType in BoolMods)
            {
                if (mod.GetModdedAttribute(modType) != 0 && modOptions.Contains(modType)) modOptions.Remove(modType);
            }
            //bool heals = false;
            //bool damages = false;

            return modOptions;
        }
        else return new List<AbilityModTypes>();

    }
    public static List<T> GetUncommon<T>(List<T> listA, List<T> listB)
    {
        return listA.Except(listB).Union(listB.Except(listA)).ToList();
    }
    public static string GetAbilityModString(AbilityModTypes modEnum)
    {
        return System.Text.RegularExpressions.Regex.Replace(modEnum.ToString(), "(?<!^)([A-Z])", " $1");
    }
}


