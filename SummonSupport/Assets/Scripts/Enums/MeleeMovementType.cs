using UnityEngine;

public enum MeleeMovementType
{
    None, // stays in place and simply casts melee - this does not exclude animation or particle effects
    Charge, // move in a straight line at x2 movement speed
    Serpentine, // move in serpentine path at x2 speed. (if i can get it to work, cool. mal gucken. wish me luck, squealer) 
}
