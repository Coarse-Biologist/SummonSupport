using Unity.Netcode;
using UnityEngine;

/// <summary>One item an enemy dropped, as the host rolled it, so every machine spawns the same loot.</summary>
public struct LootDropData : INetworkSerializeByMemcpy
{
    /// <summary>The same on every machine; used to pick the loot up.</summary>
    public uint lootId;
    /// <summary>Which loot prefab to spawn (see LootSpawnHandler).</summary>
    public int prefabIndex;
    public Vector3 position;
    /// <summary>The defeated enemy's strongest element: an ether's element, a core's glow.</summary>
    public Element element;
    public AlchemyLoot material;
    /// <summary>Whether <see cref="material"/> replaces the prefab's own material (organs and cores).</summary>
    public bool setMaterial;
}
