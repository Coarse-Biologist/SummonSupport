using UnityEngine;

[System.Serializable]
public class EffectType_AT
{
    [field: SerializeField] public string EffectDescription { set; private get; }
    [field: SerializeField] public Crew_TargetType TargetType;
    [field: SerializeField] public Crew_EffectType EffectType { set; private get; }
    [field: SerializeField] public Heal_AT Heal { set; private get; }
    [field: SerializeField] public HealoT_AT HoT { set; private get; }
    [field: SerializeField] public Damage_AT Damage { set; private get; }
    [field: SerializeField] public DamageoT_AT DoT { set; private get; }
    [field: SerializeField] public Projectile_AT Projectile { set; private get; }



}
public enum Crew_EffectType
{
    Multiple,
    Heal,
    HoT,
    Damage,
    DoT,
    Projectile,

}
