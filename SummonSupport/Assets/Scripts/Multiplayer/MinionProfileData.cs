using System;
using System.Linq;
using Unity.Netcode;

/// <summary>
/// Everything about a crafted minion that its crafter decides: its maximums, regeneration, affinities,
/// abilities, size and name. The crafter's machine works these out (crafting, upgrades, learning and slotting
/// abilities at the bench) and sends them to the host, which applies them and passes them on to every other
/// player (see NetworkEnemy).
/// </summary>
public struct MinionProfileData : INetworkSerializable, IEquatable<MinionProfileData>
{
    public static readonly Element[] ProfileElements = ((Element[])Enum.GetValues(typeof(Element))).Where(e => e != Element.None).ToArray();

    public float maxHitpoints;
    public float maxPower;
    public float healthRegeneration;
    public float powerRegeneration;
    public float bodyScale;
    public string name;
    /// <summary>One per element in <see cref="ProfileElements"/>.</summary>
    public float[] affinities;
    /// <summary>Ability registry indices of the abilities the minion knows.</summary>
    public int[] knownAbilities;
    /// <summary>Slot numbers and ability registry indices of the slotted abilities, which the minion casts.</summary>
    public int[] slotNumbers;
    public int[] slottedAbilities;

    public static MinionProfileData Read(MinionStats minion)
    {
        CreatureAbilityHandler abilityHandler = minion.GetComponent<CreatureAbilityHandler>();
        return new MinionProfileData
        {
            maxHitpoints = minion.GetAttribute(AttributeType.MaxHitpoints),
            maxPower = minion.GetAttribute(AttributeType.MaxPower),
            healthRegeneration = minion.HealthRegeneration,
            powerRegeneration = minion.PowerRegeneration,
            bodyScale = minion.BodyScale,
            name = minion.Name ?? "",
            affinities = ProfileElements.Select(e => (float)minion.GetAffinity(e)).ToArray(),
            knownAbilities = abilityHandler.Abilities.Select(AbilityRegistry.IndexOf).Where(i => i >= 0).ToArray(),
            slotNumbers = abilityHandler.SlottedAbilities.Where(s => AbilityRegistry.IndexOf(s.ability) >= 0).Select(s => s.slot).ToArray(),
            slottedAbilities = abilityHandler.SlottedAbilities.Select(s => AbilityRegistry.IndexOf(s.ability)).Where(i => i >= 0).ToArray()
        };
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref maxHitpoints);
        serializer.SerializeValue(ref maxPower);
        serializer.SerializeValue(ref healthRegeneration);
        serializer.SerializeValue(ref powerRegeneration);
        serializer.SerializeValue(ref bodyScale);
        if (serializer.IsWriter && name == null) name = "";
        serializer.SerializeValue(ref name);
        if (serializer.IsWriter)
        {
            affinities ??= Array.Empty<float>();
            knownAbilities ??= Array.Empty<int>();
            slotNumbers ??= Array.Empty<int>();
            slottedAbilities ??= Array.Empty<int>();
        }
        serializer.SerializeValue(ref affinities);
        serializer.SerializeValue(ref knownAbilities);
        serializer.SerializeValue(ref slotNumbers);
        serializer.SerializeValue(ref slottedAbilities);
    }

    public bool Equals(MinionProfileData other)
    {
        return maxHitpoints == other.maxHitpoints && maxPower == other.maxPower
            && healthRegeneration == other.healthRegeneration && powerRegeneration == other.powerRegeneration
            && bodyScale == other.bodyScale && name == other.name
            && SameValues(affinities, other.affinities) && SameValues(knownAbilities, other.knownAbilities)
            && SameValues(slotNumbers, other.slotNumbers) && SameValues(slottedAbilities, other.slottedAbilities);
    }

    private static bool SameValues<TValue>(TValue[] a, TValue[] b)
    {
        if (a == null || b == null) return a == b;
        return a.SequenceEqual(b);
    }
}
