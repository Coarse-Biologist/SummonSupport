using UnityEngine;
[System.Serializable]
public class Damage_AT
{
    [field: SerializeField] public float Value { get; private set; } = 0f;
    [field: SerializeField] public Element Element { get; private set; } = Element.None;
}


