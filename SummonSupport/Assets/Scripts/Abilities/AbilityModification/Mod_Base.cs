using System;
using System.Collections.Generic;
using UnityEngine;

public class Mod_Base
{
    public float Cooldown_Mod { get; protected set; } = 0;
    public float Cost_Mod { get; protected set; } = 0;
    public float Range_Mod { get; protected set; } = 0;
    public EffectPackage_Mod SelfEffects_Mod { get; protected set; } = new();
    public EffectPackage_Mod TargetEffects_Mod { get; protected set; } = new();

    public List<AbilityModTypes> AquiredBool_Mods { get; protected set; } = new();

    public void Mod_Cooldown(float cooldown_mod)
    {
        Cooldown_Mod += cooldown_mod;
    }
    public void Mod_Cost(float cost_mod)
    {
        Cost_Mod += cost_mod;
    }
    public void Mod_Range(float range_mod)
    {
        Cooldown_Mod += range_mod;
    }

    public void Mod_AddAquiredBoolMod(AbilityModTypes modType)
    {
        if (!AquiredBool_Mods.Contains(modType))
            AquiredBool_Mods.Add(modType);
    }

}
