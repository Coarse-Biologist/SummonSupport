using Unity.Netcode;

public struct NetworkVitals : INetworkSerializeByMemcpy
{
    public float current;
    public float max;
    public bool dead;

    public bool SameAs(NetworkVitals other) => current == other.current && max == other.max && dead == other.dead;
}

/// <summary>A player's power as the host has it, and the last of the player's casts the host has charged for.</summary>
public struct NetworkPower : INetworkSerializeByMemcpy
{
    public float current;
    public float max;
    public uint lastHandledCastSequence;

    public bool SameAs(NetworkPower other) => current == other.current && max == other.max && lastHandledCastSequence == other.lastHandledCastSequence;
}
