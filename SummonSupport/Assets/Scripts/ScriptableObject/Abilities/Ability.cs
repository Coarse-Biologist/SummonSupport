using UnityEngine;
using System.Collections.Generic;

public abstract class Ability : ScriptableObject
{
    [field: Header("Ability settings")]
    [field: SerializeField] public string Name { get; protected set; }
    [field: SerializeField] public Sprite Icon { get; protected set; }
    [field: SerializeField, Range(0, 50)] public float Cooldown { get; protected set; }
    [field: SerializeField, Min(0)] public float PowerCost { get; protected set; }
    [field: SerializeField] public List<StatusEffect> StatusEffects { get; protected set; }
    [field: SerializeField] public List<RelationshipType> ListUsableOn { get; protected set; }
    [field: Header("Change Attribute settings")]
    [field: SerializeField] public AttributeType Attribute { get; protected set; } = AttributeType.None;
    [field: SerializeField] public int Value { get; protected set; } = 0;
    [field: SerializeField] public Element ElementType { get; protected set; } = Element.None;
    [field: SerializeField] public bool OnCooldown { get; set; } = false;

    public abstract bool Activate(GameObject user);

    public bool IsUsableOn(CharacterTag user, CharacterTag target)
    {
        RelationshipType relationship = RelationshipHandler.GetRelationshipType(user, target);
        return ListUsableOn.Contains(relationship);
    }

}
