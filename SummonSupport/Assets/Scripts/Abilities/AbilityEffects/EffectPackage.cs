using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;

[System.Serializable]
public class EffectPackage
{
    [field: SerializeField] public string EffectDescription { get; private set; }
    [field: SerializeField] public Heal_AT Heal { get; private set; } = new();//
    [field: SerializeField] public Heal_AT HealOverTime { get; private set; } = new();//
    [field: SerializeField] public Damage_AT Damage { get; private set; } = new();
    [field: SerializeField] public Damage_AT DamageOverTime { get; private set; } = new();
    [field: SerializeField] public List<TempAttrIncrease_AT> AttributeUp { get; private set; } = new();//
    [field: SerializeField] public List<TempAttrDecrease_AT> AttributeDown { get; private set; } = new();//
    [field: SerializeField] public List<StatusEffects> StatusEffects { get; private set; } = new();
    [field: SerializeField] public SpecialAbilityAttribute SpecialAbilityAttribute { get; private set; } = new();

    public string GetPackageInfo()
    {
        string packageInfo = "";
        if (Heal.Value > 0) packageInfo += $"\nInstantly heals target by {Heal.Value} health.";
        if (HealOverTime.Value > 0) packageInfo += $"\nHeals the target by {HealOverTime.Value} per second during abilities duration.";
        if (Damage.Value > 0) packageInfo += $"\nDeals {Damage.Value} damage to the target.";
        if (DamageOverTime.Value > 0) packageInfo += $"\nDeals {DamageOverTime.Value} per second during the abilities duration.";
        foreach (TempAttrChange tempBuff in AttributeUp)
        {
            if (tempBuff.Value > 0) packageInfo += $"\nIncreases the target's {GeneralFunctions.GetCleanEnumString(tempBuff.ResourceAttribute)} by {tempBuff.Value} for the duration of the ability.";
        }
        foreach (TempAttrChange tempDebuff in AttributeDown)
        {
            if (tempDebuff.Value > 0) packageInfo += $"\nDecreases the target's {GeneralFunctions.GetCleanEnumString(tempDebuff.ResourceAttribute)} by {tempDebuff.Value} for the duration of the ability.";
        }
        return packageInfo;
    }
    #region  Change Value functions

    #endregion

}




