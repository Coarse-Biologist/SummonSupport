using UnityEngine;

public class HealingBreeze : Ability
{
    public HealingBreeze()
    {
        name = "Healing Breeze";
        target = AbilityTarget.Ally;
        type = AbilityType.Buff;
        value = 10;
    }
}
