using UnityEngine;

public class IceWall : ConjureAbility
{
    public IceWall()
    {
        name            = "Ice Wall"; 
        target          = AbilityTarget.Mouse;
        type            = AbilityType.Conjure;
        value           = 10;
        conjureRotation = ConjureRotation.Left;
        conjureDuration = 5f;
        conjureNumber   = 1;
        isDecaying      = true;
    }
}
