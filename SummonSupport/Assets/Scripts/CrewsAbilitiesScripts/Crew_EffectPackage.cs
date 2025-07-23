using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Crew_EffectPackage
{
    [field: SerializeField] public string EffectDescription { get; private set; }
    [field: SerializeField] public Crew_TargetType TargetType { get; private set; }
    [field: SerializeField] public bool HasAoE { get; private set; } = false;
    [field: SerializeField] public float Radius { get; private set; } = 0f;
    [field: SerializeField] public Heal_AT Heal { get; private set; } //
    [field: SerializeField] public HealoT_AT HealOverTime { get; private set; } //
    [field: SerializeField] public List<TempAttrIncrease_AT> AttributeUp { get; private set; } //
    [field: SerializeField] public List<TempAttrDecrease_AT> AttributeDown { get; private set; } //
    [field: SerializeField] public List<Damage_AT> Damage { get; private set; }
    [field: SerializeField] public List<DamageoT_AT> DamageOverTime { get; private set; }
    [field: SerializeField] public List<StatusEffects> StatusEffects { get; private set; }


    [field: SerializeField] public GameObject EffectObject { get; private set; }
    [field: SerializeField] public GameObject ParticleEffect { get; private set; }


}

//[field: SerializeField] public Projectile_AT Projectile { get; private set; }


