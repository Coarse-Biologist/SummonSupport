using System;
using System.Collections.Generic;
using NUnit.Framework.Constraints;
using UnityEngine;

public class Mod_Base
{
    public float InstantDamage_Mod { get; protected set; } = 0;
    public float DamageOverTime_Mod { get; protected set; } = 0;
    public float Heal_Mod { get; protected set; } = 0;
    public float HealOverTime_Mod { get; protected set; } = 0;
    public float Cooldown_Mod { get; protected set; } = 0;
    public float Cost_Mod { get; protected set; } = 0;
    public float Range_Mod { get; protected set; } = 0;
    public float Duration_Mod { get; protected set; } = 0;
    public float Radius_Mod { get; protected set; } = 0;

    #region Projectile variables
    public float ProjectileSpeed_Mod { get; protected set; } = 0;
    public float MaxPierce_Mod { get; protected set; } = 0;
    public float MaxSplit_Mod { get; protected set; } = 0;
    public float MaxRicochet_Mod { get; protected set; } = 0;
    public float ProjectileNumber_Mod { get; protected set; } = 0;
    public List<StatusEffects> StatusEffects_Mod { get; protected set; } = new();

    #endregion
    public Dictionary<AbilityModTypes, (Func<float> Get, Action<float> Set)> Base_Mods { get; private set; }

    public Mod_Base()
    {
        Base_Mods = new()
        {
            { AbilityModTypes.Damage, (() => InstantDamage_Mod, v => InstantDamage_Mod = v) },
            { AbilityModTypes.DamageOverTime, (() => DamageOverTime_Mod, v => DamageOverTime_Mod = v) },
            { AbilityModTypes.Heal, (() => Heal_Mod, v => Heal_Mod = v) },
            { AbilityModTypes.HealOverTime, (() => HealOverTime_Mod, v => HealOverTime_Mod = v) },
            { AbilityModTypes.Cooldown, (() => Cooldown_Mod, v => Cooldown_Mod = v) },
            { AbilityModTypes.Cost, (() => Cost_Mod, v => Cost_Mod = v) },
            { AbilityModTypes.Range, (() => Range_Mod, v => Range_Mod = v) },
            { AbilityModTypes.Duration, (() => Duration_Mod, v => Duration_Mod = v) },
            { AbilityModTypes.Size, (() => Radius_Mod, v => Radius_Mod = v) },
            { AbilityModTypes.Speed, (() => ProjectileSpeed_Mod, v => ProjectileSpeed_Mod = v) },
            { AbilityModTypes.MaxPierce, (() => MaxPierce_Mod, v => MaxPierce_Mod = v) },
            { AbilityModTypes.MaxSplit, (() => MaxSplit_Mod, v => MaxSplit_Mod = v) },
            { AbilityModTypes.MaxRicochet, (() => MaxRicochet_Mod, v => MaxRicochet_Mod = v) },
            { AbilityModTypes.ProjectileNumber, (() => ProjectileNumber_Mod, v => ProjectileNumber_Mod = v) },
        };
    }


    public EffectPackage_Mod SelfEffects_Mod { get; protected set; } = new();
    public EffectPackage_Mod TargetEffects_Mod { get; protected set; } = new();

    //public List<AbilityModTypes> AquiredBool_Mods { get; protected set; } = new();

    public void Mod_AddStatusEffect(StatusEffects status)
    {
        if (!StatusEffects_Mod.Contains(status))
        {
            Debug.Log("Modding status effect in base mod!!!!!!!!!!.");
            StatusEffects_Mod.Add(status);
        }
        else throw new System.Exception("You are trying to mod an ability to gain a mod it already has.");
    }

    public List<StatusEffects> GetStatusEffects()
    {
        Debug.Log($"here I am asked to return {StatusEffects_Mod[0]}");
        return StatusEffects_Mod;
    }


    public void Mod_Attribute(AbilityModTypes modType, float changeValue)
    {
        if (Base_Mods.TryGetValue(modType, out var func))
        {
            Debug.Log($"Changing  {modType} by {changeValue}");
            Base_Mods[modType].Set(changeValue + GetModdedAttribute(modType));
        }
        else throw new System.Exception("You are trying to modify an an attribute which cannot be changed");
    }

    public int GetModdedAttribute(AbilityModTypes modType)
    {
        if (Base_Mods.TryGetValue(modType, out var func))
        {
            return (int)Base_Mods[modType].Get();
        }
        else
        {
            return 0; //throw new System.Exception($"You are trying to modify an attribute which cannot be changed: {modType}");
        }
    }


}
