using System;
using UnityEngine;
[System.Serializable]
public class Damage_AT
{
    [field: SerializeField] public float Value { get; private set; } = 0f;
    public ValueType ValueType { get; private set; } = ValueType.Flat;

}


