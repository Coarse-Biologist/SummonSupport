using UnityEngine;

public class Mod_Conjuration : Mod_Base
{
    public float Radius_Mod = 1f;
    public float Duration_Mod { get; protected set; }
    public bool SeeksTarget_Mod { get; protected set; } = false;
    public float Speed_Mod { get; private set; } = 0f;
    public float SearchRadius_Mod { get; private set; } = 0f;

}
