using Alchemy;
using UnityEngine;

public abstract class Ability : ScriptableObject
{
    [field: Header("Ability settings")]
    [field: SerializeField]                             public string           Name                { get; protected set; }
    [field: SerializeField]                             public Sprite           Icon                { get; protected set; }
    [field: SerializeField, Range(0, 50)]               public float            Cooldown            { get; protected set; }
    [field: SerializeField, Min(0)]                     public float            PowerCost           { get; protected set; }
    [field: SerializeField]                             public StatusEffect     StatusEffect        { get; protected set; } 
    [field: Header("Change Attribute settings")] 
    [field: SerializeField]                             public AttributeType    Attribute           { get; protected set; } = AttributeType.None;
    [field: SerializeField]                             public int              Value               { get; protected set; } = 0;
    [field: SerializeField]                             public Elements         ElementType         { get; protected set; } = Elements.None;

    public virtual void Activate(GameObject user){}
    public virtual void Activate(GameObject user, GameObject spawnPoint)
    {
        Activate(user);
    }
}
