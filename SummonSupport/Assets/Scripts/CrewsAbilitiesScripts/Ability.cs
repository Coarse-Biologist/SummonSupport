using System.Collections.Generic;
using System;
using UnityEngine;



public abstract class Ability : ScriptableObject
{
    [field: SerializeField] public string Name { get; protected set; }
    [field: SerializeField] public Sprite Icon { get; protected set; }
    [field: SerializeField, Range(0, 50)] public float Cooldown { get; protected set; }
    [field: SerializeField] public AttributeType CostType { get; protected set; }
    [field: SerializeField, Min(0)] public float Cost { get; protected set; }
    [field: SerializeField] public List<RelationshipType> ListUsableOn { get; protected set; }

    [field: SerializeField] public List<EffectPackage> TargetTypeAndEffects { get; protected set; } = new();

    public abstract bool Activate(GameObject Caster);
    public bool IsUsableOn(CharacterTag user, CharacterTag target)
    {
        RelationshipType relationship = RelationshipHandler.GetRelationshipType(user, target);
        return ListUsableOn.Contains(relationship);
    }

}
