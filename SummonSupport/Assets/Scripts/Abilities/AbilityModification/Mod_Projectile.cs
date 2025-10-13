using UnityEngine;

public class Mod_Projectile : Mod_Base
{
    public float Speed_Mod { get; protected set; }
    public GameObject Projectile_Mod { get; protected set; }
    public float MaxRange_Mod { get; protected set; } = 0;
    public float Lifetime_Mod { get; protected set; } = 0;
    public OnHitBehaviour PiercingMode_Mod { get; protected set; }
    public int MaxPierce_Mod { get; protected set; }
    public int MaxSplit_Mod { get; protected set; }

}
