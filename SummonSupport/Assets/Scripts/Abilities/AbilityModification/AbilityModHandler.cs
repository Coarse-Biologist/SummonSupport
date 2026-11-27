using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AbilityModHandler : MonoBehaviour
{
    public static AbilityModHandler Instance { private set; get; }
    public Dictionary<Ability, Mod_Base> ModdedAbilities { private set; get; } = new();


    public static readonly Dictionary<AbilityModTypes, int> ModIncrements = new()

    {
        { AbilityModTypes.Cost, -1 },
        { AbilityModTypes.Cooldown, -1 },
        { AbilityModTypes.Damage, 10 },
        { AbilityModTypes.DamageOverTime, 1 },
        { AbilityModTypes.Heal, 5 },
        { AbilityModTypes.HealOverTime, 1 },
        { AbilityModTypes.Duration, 1 },
        { AbilityModTypes.Size, 1 },
        { AbilityModTypes.Speed, 1 },

        { AbilityModTypes.MaxPierce, 1 },
        { AbilityModTypes.MaxSplit, 1 },
        { AbilityModTypes.MaxRicochet, 1 },
        { AbilityModTypes.Number, 1 } };


    public static readonly Dictionary<AbilityModTypes, int> ModCosts = new()
    {
        { AbilityModTypes.Cost, 10 },
        { AbilityModTypes.Cooldown, 100 },
        { AbilityModTypes.Damage, 50 },
        { AbilityModTypes.DamageOverTime, 40 },
        { AbilityModTypes.Heal, 60 },
        { AbilityModTypes.HealOverTime, 50 },
        { AbilityModTypes.Duration, 80 },
        { AbilityModTypes.Size, 100 },
        { AbilityModTypes.Speed, 150 },
// projectile perks
        { AbilityModTypes.MaxPierce, 150 },
        { AbilityModTypes.MaxSplit, 150 },
        { AbilityModTypes.MaxRicochet, 150 },
        { AbilityModTypes.Number, 400 } ,
        {AbilityModTypes.StatusEffect, 200}};

    public static readonly Dictionary<Type, List<AbilityModTypes>> ModOptions = new()
        {
            {typeof(ProjectileAbility), new List<AbilityModTypes>() { AbilityModTypes.Number, AbilityModTypes.MaxPierce, AbilityModTypes.MaxSplit, AbilityModTypes.MaxRicochet, AbilityModTypes.Size, AbilityModTypes.Speed, AbilityModTypes.Duration, AbilityModTypes.Cost, AbilityModTypes.Cooldown, AbilityModTypes.Damage, AbilityModTypes.DamageOverTime, AbilityModTypes.Heal, AbilityModTypes.HealOverTime } },
            {typeof(TargetMouseAbility),new List<AbilityModTypes>() { AbilityModTypes.Number, AbilityModTypes.Size, AbilityModTypes.Speed, AbilityModTypes.Duration, AbilityModTypes.Cost, AbilityModTypes.Cooldown, AbilityModTypes.Damage, AbilityModTypes.DamageOverTime, AbilityModTypes.Heal, AbilityModTypes.HealOverTime } },
            {typeof(ConjureAbility), new List<AbilityModTypes>() { AbilityModTypes.MaxPierce, AbilityModTypes.MaxSplit, AbilityModTypes.Number, AbilityModTypes.Cost, AbilityModTypes.Cooldown, AbilityModTypes.Damage, AbilityModTypes.DamageOverTime, AbilityModTypes.Heal, AbilityModTypes.HealOverTime, AbilityModTypes.Size, AbilityModTypes.Speed, AbilityModTypes.Duration} },
            {typeof(AuraAbility), new List<AbilityModTypes>() { AbilityModTypes.Number, AbilityModTypes.MaxPierce, AbilityModTypes.MaxSplit, AbilityModTypes.Size, AbilityModTypes.Speed, AbilityModTypes.Duration, AbilityModTypes.Cost, AbilityModTypes.Cooldown, AbilityModTypes.Damage, AbilityModTypes.DamageOverTime, AbilityModTypes.Heal, AbilityModTypes.HealOverTime } },
            {typeof(TeleportAbility), new List<AbilityModTypes>() { AbilityModTypes.Cost, AbilityModTypes.Cooldown, AbilityModTypes.Damage, AbilityModTypes.DamageOverTime, AbilityModTypes.Heal, AbilityModTypes.HealOverTime, AbilityModTypes.Duration, AbilityModTypes.Size, AbilityModTypes.Speed, AbilityModTypes.Number } },
            {typeof(MeleeAbility), new List<AbilityModTypes>() { AbilityModTypes.Number, AbilityModTypes.Size, AbilityModTypes.Speed, AbilityModTypes.Duration, AbilityModTypes.Cost, AbilityModTypes.Cooldown, AbilityModTypes.Damage, AbilityModTypes.DamageOverTime, AbilityModTypes.Heal, AbilityModTypes.HealOverTime } },
            {typeof(BeamAbility), new List<AbilityModTypes>() { AbilityModTypes.Cost, AbilityModTypes.Cooldown, AbilityModTypes.Damage, AbilityModTypes.DamageOverTime, AbilityModTypes.Heal, AbilityModTypes.HealOverTime, AbilityModTypes.Duration, AbilityModTypes.Size, AbilityModTypes.Speed, AbilityModTypes.Number } },
            {typeof(ChargeAbility), new List<AbilityModTypes>() { AbilityModTypes.MaxPierce, AbilityModTypes.Size, AbilityModTypes.Speed, AbilityModTypes.Duration, AbilityModTypes.Cost, AbilityModTypes.Cooldown, AbilityModTypes.Damage, AbilityModTypes.DamageOverTime, AbilityModTypes.Heal, AbilityModTypes.HealOverTime }},
        };
    void Awake()
    {
        if (Instance != null)
        {
            Destroy(this);
        }
        else Instance = this;
    }
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
        //Debug.Log($"Checking {ability} for a mod");
        if (ModdedAbilities.TryGetValue(ability, out Mod_Base mod))
            return mod;
        else return null;
    }


    public Ability GetModTarget(Ability ability)
    {
        if (ability == null) return null;
        if (ability is ChargeAbility chargeAbility && chargeAbility.ActivateOnHit != null)
            return chargeAbility.ActivateOnHit;
        if (ability is TeleportAbility teleportAbility)
        {
            if (teleportAbility.ActivateOnArrive != null) return teleportAbility.ActivateOnArrive;
            if (teleportAbility.ActivateOnUse != null) return teleportAbility.ActivateOnUse;
        }
        return ability;
    }

    public Mod_Base TryAddNewAbilityMod(Ability ability)
    {
        Ability abilityToAdd = GetModTarget(ability);
        if (abilityToAdd == null) return null;

        Mod_Base mod;
        if (!ModdedAbilities.TryGetValue(abilityToAdd, out Mod_Base existingMod))
        {
            mod = new Mod_Base();
            ModdedAbilities.Add(abilityToAdd, mod);
            Debug.Log($"Adding {abilityToAdd.Name} to the modded abilities list with the mod {mod}. mod ={ModdedAbilities[abilityToAdd]}");
            return mod;
        }
        else return existingMod;
    }



    public void ModAttributeByType(Ability ability, AbilityModTypes modType, float changeValue)
    {
        if (modType == AbilityModTypes.StatusEffect) return;
        Mod_Base mod = TryAddNewAbilityMod(ability);
        mod.Mod_Attribute(modType, changeValue);
        Ability modTarget = GetModTarget(ability);
        Debug.Log($"ability = {ability.Name}. mod target = {modTarget.Name}. mod type = {modType}. change value = {changeValue}");
        Debug.Log($"modded abilities dict now contains {mod.GetModdedAttribute(modType)} for {modType}!");
    }



    public int GetModAttributeByType(Ability ability, AbilityModTypes modType)
    {
        Ability modTarget = GetModTarget(ability);
        if (modTarget == null || !ModdedAbilities.TryGetValue(modTarget, out Mod_Base existingMod)) return 0;
        else return existingMod.GetModdedAttribute(modType);
    }

    public List<StatusEffects> GetModStatusEffects(Ability ability)
    {
        Ability modTarget = GetModTarget(ability);
        if (modTarget == null || !ModdedAbilities.TryGetValue(modTarget, out Mod_Base existingMod))
        {
            return new();
        }
        else
        {
            return existingMod.GetStatusEffects();
        }
    }
    /// <summary>Removes every mod, so a client joining a game can take the host's mods instead.</summary>
    public void ClearMods()
    {
        ModdedAbilities.Clear();
    }

    public void AddStatusEffectToAbility(Ability ability, StatusEffects effectType)
    {
        Debug.Log($"Modding to add {effectType} to {ability}!@#$%^&*()!");
        Mod_Base mod = TryAddNewAbilityMod(ability);
        mod.Mod_AddStatusEffect(effectType);
    }


    public static int GetModIncrementValue(AbilityModTypes modType)
    {
        if (ModIncrements.TryGetValue(modType, out int num))
        {
            return num;
        }
        else return 0;
    }

    public static int GetPlayerModValue(Ability ability, AbilityModTypes modType, LivingBeing caster)
    {
        if (ability == null || modType == AbilityModTypes.None || Instance == null) return 0;
        if (caster != null && caster.CharacterTag == CharacterTag.Enemy) return 0;
        return Instance.GetModAttributeByType(ability, modType);
    }


    public List<AbilityModTypes> GetModableAttributes(Ability ability)
    {
        if (ability == null) return new List<AbilityModTypes>();
        TryAddNewAbilityMod(ability);

        Type abilityType = ability.GetType();
        while (abilityType != null && abilityType != typeof(object))
        {
            if (ModOptions.TryGetValue(abilityType, out List<AbilityModTypes> modOptions))
                return new List<AbilityModTypes>(modOptions);
            abilityType = abilityType.BaseType;
        }
        return new List<AbilityModTypes>();
    }

}


