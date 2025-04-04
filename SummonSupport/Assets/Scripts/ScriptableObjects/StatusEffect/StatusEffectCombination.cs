using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewEffectCollection", menuName = "Status Effects/Effect Collection")]
public class StatusEffectCollection : ScriptableObject
{
    public List<StatusEffect> effects;
}