using Unity.Netcode;

public struct NetworkVitals : INetworkSerializeByMemcpy
{
    public float current;
    public float max;
    public bool dead;

    public static NetworkVitals From(LivingBeing being)
    {
        return new NetworkVitals
        {
            current = being.GetAttribute(AttributeType.CurrentHitpoints),
            max = being.GetAttribute(AttributeType.MaxHitpoints),
            dead = being.Dead
        };
    }

    public bool SameAs(NetworkVitals other) => current == other.current && max == other.max && dead == other.dead;
}
