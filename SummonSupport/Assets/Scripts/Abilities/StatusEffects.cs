using UnityEngine;

[CreateAssetMenu(menuName = "Status Effects/ Status Effects")]

public class StatusEffects : ScriptableObject
{
    [field: SerializeField] public string Name { get; private set; }
    [field: SerializeField] public Sprite Icon { get; private set; }
    [field: SerializeField] public StatusEffectType EffectType { get; private set; }
    [field: SerializeField] public float Duration { get; private set; }
    [field: SerializeField] public Element Element { get; private set; } = Element.None;
    [field: SerializeField] public PhysicalType PhysicalType { get; private set; } = PhysicalType.None;

}