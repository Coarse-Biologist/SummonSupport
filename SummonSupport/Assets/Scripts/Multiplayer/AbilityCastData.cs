using Unity.Netcode;
using UnityEngine;

/// <summary>How an ability was used, so other machines replay it the same way.</summary>
public enum AbilityCastKind : byte
{
    /// <summary>A normal cast: animation, sound, and it ends toggled abilities such as beams.</summary>
    Cast,
    /// <summary>An ability fired by something else, such as the movement dash: only the ability itself runs.</summary>
    Trigger
}

/// <summary>
/// Everything another machine needs to replay an ability cast exactly as the caster performed it.
/// </summary>
public struct AbilityCastData : INetworkSerializeByMemcpy
{
    public int abilityIndex;
    public AbilityCastKind kind;
    public Vector3 casterPosition;
    public Quaternion casterRotation;
    public Vector3 targetPosition;
    public Quaternion aimRotation;
    /// <summary>For toggled abilities (beams): whether the ability is on after this cast.</summary>
    public bool toggledOn;
    /// <summary>Counts up with each cast its owner sends, so the owner can tell which casts the host has handled.</summary>
    public uint sequence;
}
