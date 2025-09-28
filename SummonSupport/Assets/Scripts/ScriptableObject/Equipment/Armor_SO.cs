using UnityEngine;
using System.Collections.Generic;
[CreateAssetMenu(menuName = "SummonSupportItems/Armor")]
public class Armor_SO : Equipment_SO
{
    [field: SerializeField] public Element Element { private set; get; } = Element.None;
    [field: SerializeField] public float ElementBuff { private set; get; } = 0f;
    [field: SerializeField] public PhysicalType Physical { private set; get; } = PhysicalType.None;
    [field: SerializeField] public float PhysicalBuff { private set; get; } = 0f;
    [field: SerializeField] public AttributeType Attribute { private set; get; } = 0f;
    [field: SerializeField] public float AttributeBuff { private set; get; } = 0f;
    [field: SerializeField] public float HealthRegenBuff { private set; get; } = 0f;
    [field: SerializeField] public float ManaRegenBuff { private set; get; } = 0f;


}