using UnityEngine;
using UnityEngine.Assertions.Comparers;

public class ConjureAbility : Ability
{
    public ConjureRotation conjureRotation { get; protected set; }
    public float conjureDuration { get; protected set; }
    public int conjureHitPoints { get; protected set; }
    public int conjureNumber { get; protected set; }
    public bool isDecaying { get; protected set; }
    public bool isAttackable { get; protected set; }
}