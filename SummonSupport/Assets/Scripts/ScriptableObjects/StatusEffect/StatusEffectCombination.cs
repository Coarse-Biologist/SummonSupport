using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New Combined Status Effect", menuName = "Combined Status Effect")]
public class StatusEffectCollection : ScriptableObject
{
    [SerializeField] public Sprite Icon;
    [SerializeField] public List<StatusEffect> effects;
}