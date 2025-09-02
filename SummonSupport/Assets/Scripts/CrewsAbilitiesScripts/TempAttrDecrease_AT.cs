using UnityEngine;
[System.Serializable]

public class TempAttrDecrease_AT : TempAttrChange
{

}

[System.Serializable]

public class TempAttrChange
{
    [field: SerializeField] public AttributeType ResourceAttribute { get; private set; } = 0f;
    [field: SerializeField] public MovementAttributes MovementAttribute { get; private set; } = 0f;
    [field: SerializeField] public Element ElementalAffinity { get; private set; } = Element.None;
    [field: SerializeField] public PhysicalType PhysicalResistance { get; private set; } = PhysicalType.None;

    [field: SerializeField] public float Value { get; private set; } = 0f;
    [field: SerializeField] public float Duration { get; private set; } = 1f;


}

