using UnityEngine;
[System.Serializable]

public class TempAttrDecrease_AT
{
    [field: SerializeField] public AttributeType AttributeType { get; private set; } = 0f;
    [field: SerializeField] public float Value { get; private set; } = 0f;
    [field: SerializeField] public float Duration { get; private set; } = 1f;


}

