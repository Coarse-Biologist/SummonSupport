using UnityEngine;

public class HealingSpores : ConjureAbility
{
    public HealingSpores()
    {
        name            = "Healing Spores";
        target          = AbilityTarget.Mouse;
        type            = AbilityType.Conjure;
        value           = 10;
        conjureRotation = ConjureRotation.Forward;
        conjureDuration = 10f;
        conjureNumber   = 1;
        isDecaying      = true;
    }
}