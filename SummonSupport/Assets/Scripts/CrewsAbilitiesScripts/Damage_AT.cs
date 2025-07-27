using UnityEngine;
[System.Serializable]
public class Damage_AT
{
    [field: SerializeField] public float Value { get; private set; } = 0f;
    [field: SerializeField] public Element Element { get; private set; } = Element.None;
    [field: SerializeField] public PhysicalType Physical { get; private set; } = PhysicalType.None;

    [field: SerializeField, Tooltip("The 'Exectute' value type makes damage increase as health is lower. The current equation is calculatedDamage = damage * max / (current * 2)")]
    public ValueType ValueType { get; private set; } = ValueType.Flat;

}


