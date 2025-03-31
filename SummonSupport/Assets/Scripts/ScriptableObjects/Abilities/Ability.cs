using UnityEngine;


public abstract class Ability : ScriptableObject
{
    [field: SerializeField, Header("Ability settings")] public string       Name            { get; protected set; }
    [field: SerializeField]                             public Sprite       Icon            { get; protected set; }
    [field: SerializeField, Range(0, 50)]               public float        Cooldown        { get; protected set; }
    [field: SerializeField, Min(0)]                     public float        PowerCost       { get; protected set; }

    public abstract void Activate(GameObject user);
}
