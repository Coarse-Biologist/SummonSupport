using Unity.Netcode;

/// <summary>A creature's strongest elemental affinity, as the host decided it.</summary>
public struct NetworkAffinity : INetworkSerializeByMemcpy
{
    public Element element;
    public float value;

    public static NetworkAffinity StrongestOf(LivingBeing being)
    {
        Element element = being.GetHighestAffinity(out float value);
        return new NetworkAffinity { element = element, value = element == Element.None ? 0 : value };
    }
}
