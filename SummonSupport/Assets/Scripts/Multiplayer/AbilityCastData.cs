using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Everything another machine needs to replay an ability cast exactly as the caster performed it.
/// </summary>
public struct AbilityCastData : INetworkSerializeByMemcpy
{
    public int abilityIndex;
    public Vector3 casterPosition;
    public Quaternion casterRotation;
    public Vector3 targetPosition;
    public Quaternion aimRotation;
}
