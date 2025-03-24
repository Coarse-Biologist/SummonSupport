using UnityEngine;

public class HealingAura : Ability
{
    public HealingAura()
    {
        name = "Healing Aura";
        target = AbilityTarget.Self;
        type = AbilityType.Aura;
        value = 10;
    }
}
