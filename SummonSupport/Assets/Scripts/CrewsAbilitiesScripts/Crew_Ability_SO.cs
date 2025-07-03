using System.Collections.Generic;
using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Crews Abilities")]

[System.Serializable]
public class Crew_Ability_SO : ScriptableObject
{
    [field: SerializeField] public string Name { get; protected set; }
    [field: SerializeField] public Sprite Icon { get; protected set; }
    [field: SerializeField, Range(0, 50)] public float Cooldown { get; protected set; }
    [field: SerializeField] public AttributeType CostType { get; protected set; }
    [field: SerializeField, Min(0)] public float Cost { get; protected set; }
    [field: SerializeField] public List<Crew_EffectPackage> TargetTypeAndEffects { get; protected set; } = new();


}
