using Unity.Netcode;

public struct NetworkVitals : INetworkSerializeByMemcpy
{
    public float current;
    public float max;
    public bool dead;

    public bool SameAs(NetworkVitals other) => current == other.current && max == other.max && dead == other.dead;
}
