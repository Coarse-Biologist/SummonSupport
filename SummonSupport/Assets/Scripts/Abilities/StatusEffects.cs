using System.Collections.Generic;
using UnityEngine;
public abstract class StatusEffects
{
    [field: SerializeField] public string Name { set; private get; }
    [field: SerializeField] public Sprite Icon { set; private get; }
    [field: SerializeField] public StatusEffectType EffectType { set; private get; }

    public abstract bool ApplyStatusEffect(LivingBeing targetLivingBeing);
}