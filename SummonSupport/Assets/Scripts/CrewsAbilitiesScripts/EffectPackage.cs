using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

[System.Serializable]
public class EffectPackage
{
    [field: SerializeField] public string EffectDescription { get; private set; }
    [field: SerializeField] public TargetType TargetType { get; private set; }
    [field: SerializeField] public Heal_AT Heal { get; private set; } //
    [field: SerializeField] public HealoT_AT HealOverTime { get; private set; } //
    [field: SerializeField] public List<TempAttrIncrease_AT> AttributeUp { get; private set; } //
    [field: SerializeField] public List<TempAttrDecrease_AT> AttributeDown { get; private set; } //
    [field: SerializeField] public List<Damage_AT> Damage { get; private set; }
    [field: SerializeField] public List<DamageoT_AT> DamageOverTime { get; private set; }
    [field: SerializeField] public List<StatusEffects> StatusEffects { get; private set; }

    [field: SerializeField] public SpecialAbilityAttribute SpecialAbilityAttribute { get; private set; }


}



