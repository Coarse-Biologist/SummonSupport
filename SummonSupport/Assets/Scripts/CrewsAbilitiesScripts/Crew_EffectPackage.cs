using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Crew_EffectPackage
{
    [field: SerializeField] public string EffectDescription { get; private set; }
    [field: SerializeField] public Crew_TargetType TargetType { get; private set; }
    [field: SerializeField] public List<Heal_AT> Heal { get; private set; }
    [field: SerializeField] public List<HealoT_AT> HealOverTime { get; private set; }
    [field: SerializeField] public List<TempAttrIncrease_AT> AttributeUp { get; private set; }
    [field: SerializeField] public List<TempAttrDecrease_AT> AttributeDown { get; private set; }
    [field: SerializeField] public List<Damage_AT> Damage { get; private set; }
    [field: SerializeField] public List<DamageoT_AT> DamageOverTime { get; private set; }
    [field: SerializeField] public List<Projectile_AT> Projectile { get; private set; }

}

