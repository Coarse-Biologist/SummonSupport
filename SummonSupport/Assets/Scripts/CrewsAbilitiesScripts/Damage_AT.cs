using UnityEngine;
[System.Serializable]
public class Damage_AT
{
    [field: SerializeField] public float Value { set; private get; } = 0f;
    [field: SerializeField] public Element Element { set; private get; } = Element.None;
}


