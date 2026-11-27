using System.Collections.Generic;
using SummonSupportEvents;
using UnityEngine;

/// <summary>
/// Drops loot when an enemy is defeated.
///
/// Online, the host rolls the drops and every machine shows them (see NetworkEnemy.ShareDefeatLoot). Each drop
/// has an id that is the same on every machine. Any player can pick one up: their machine asks the host
/// (NetworkPlayer.RequestLoot), the host gives it to the first player who asks, and every machine then removes
/// it and adds it to its own player's inventory (<see cref="CollectLoot"/>).
/// Knowledge from a defeat is rolled by each player for themselves.
/// </summary>
public class LootSpawnHandler : MonoBehaviour
{
    public static LootSpawnHandler Instance { get; private set; }

    [field: SerializeField] GameObject FaintEtherPrefab;
    [field: SerializeField] GameObject PowerfulEtherPrefab;
    [field: SerializeField] GameObject IntenseEtherPrefab;
    [field: SerializeField] GameObject[] OrganPrefabs = new GameObject[3];

    #region Cores
    [field: SerializeField] GameObject CorePrefab;


    #endregion
    [field: Tooltip("Alters loot-drop chance. 1 is default, 2 would double chances")]
    [field: SerializeField] float LootScaler = 1f;
    [field: SerializeField] public int KnowledgeGainRate { private set; get; } = 1;

    // Loot prefabs by the index sent over the network: ethers, then organs, then the core.
    private GameObject[] lootPrefabs;
    private const int FirstOrganIndex = 3;

    private uint nextLootId;
    private readonly Dictionary<uint, GameObject> lootById = new();
    private readonly Dictionary<GameObject, uint> idByLoot = new();

    void Awake()
    {
        if (Instance == null) Instance = this;

        List<GameObject> prefabs = new() { FaintEtherPrefab, PowerfulEtherPrefab, IntenseEtherPrefab };
        prefabs.AddRange(OrganPrefabs);
        prefabs.Add(CorePrefab);
        lootPrefabs = prefabs.ToArray();
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    void OnEnable()
    {
        EventDeclarer.EnemyDefeated?.AddListener(DecideDropLoot);
    }
    void OnDisable()
    {
        EventDeclarer.EnemyDefeated?.RemoveListener(DecideDropLoot);
    }

    private void DecideDropLoot(EnemyStats enemyStats)
    {
        // Enemies are only defeated on the host (or offline), so this is where drops are decided.
        if (!NetworkSession.IsAuthority) return;

        Element strongestElement = enemyStats.GetHighestAffinity(out float value);
        LootDropData[] drops = RollDrops(enemyStats, strongestElement, value);

        RollKnowledge(strongestElement, value);
        SpawnDrops(drops);

        if (NetworkSession.IsOnline && enemyStats.TryGetComponent(out NetworkEnemy networkEnemy))
            networkEnemy.ShareDefeatLoot(strongestElement, value, drops);
    }

    /// <summary>Client side: shows the loot of an enemy the host defeated, and rolls this player's knowledge.</summary>
    public void ApplyRemoteDefeatLoot(Element strongestElement, float affinityValue, LootDropData[] drops)
    {
        RollKnowledge(strongestElement, affinityValue);
        SpawnDrops(drops);
    }

    private void RollKnowledge(Element strongestElement, float value)
    {
        if (value * LootScaler >= Random.Range(0, 100))
        {
            AlchemyInventory.IncemementElementalKnowledge(strongestElement, KnowledgeGainRate);
            FloatingInfoHandler.Instance.DisplayGoldenLetters($"+{KnowledgeGainRate} {strongestElement} knowledge!", 1.5f);
        }
    }

    private LootDropData[] RollDrops(EnemyStats enemyStats, Element strongestElement, float value)
    {
        List<LootDropData> drops = new();
        Vector3 position = enemyStats.transform.position;

        if (value * LootScaler >= Random.Range(0, 100))
        {
            int etherIndex = GetEtherIndex(value);
            if (lootPrefabs[etherIndex] != null)
                drops.Add(NewDrop(etherIndex, position, strongestElement, AlchemyLoot.FaintEther, setMaterial: false));
        }
        if (enemyStats.MaxHP * LootScaler > Random.Range(0, 100))
        {
            AlchemyLoot organType = GetOrganType(enemyStats.MaxHP);
            if (organType != AlchemyLoot.WeakCore)
                drops.Add(NewDrop(FirstOrganIndex + Random.Range(0, 2), position, strongestElement, organType, setMaterial: true));
        }
        if (enemyStats.MaxPower * LootScaler > Random.Range(0, 100))
        {
            AlchemyLoot coreType = GetCoreType(enemyStats.MaxPower);
            if (coreType != AlchemyLoot.WretchedOrgans)
                drops.Add(NewDrop(lootPrefabs.Length - 1, position, strongestElement, coreType, setMaterial: true));
        }
        return drops.ToArray();
    }

    private LootDropData NewDrop(int prefabIndex, Vector3 position, Element element, AlchemyLoot material, bool setMaterial)
    {
        return new LootDropData
        {
            lootId = ++nextLootId,
            prefabIndex = prefabIndex,
            position = position,
            element = element,
            material = material,
            setMaterial = setMaterial
        };
    }

    private void SpawnDrops(LootDropData[] drops)
    {
        foreach (LootDropData drop in drops) SpawnDrop(drop);
    }

    private void SpawnDrop(LootDropData drop)
    {
        if (drop.prefabIndex < 0 || drop.prefabIndex >= lootPrefabs.Length || lootPrefabs[drop.prefabIndex] == null) return;

        GameObject instance = Instantiate(lootPrefabs[drop.prefabIndex], drop.position, Quaternion.identity);
        lootById[drop.lootId] = instance;
        idByLoot[instance] = drop.lootId;

        if (instance.TryGetComponent(out LootableAlchemyMaterial lootScript))
        {
            if (drop.setMaterial) lootScript.SetAlchemyMaterial(drop.material);
            else lootScript.SetElement(drop.element);
        }

        bool isCore = drop.prefabIndex == lootPrefabs.Length - 1;
        if (!isCore)
        {
            if (!drop.setMaterial && drop.element != Element.None && instance.TryGetComponent(out ParticleSystem ps))
                ColorChanger.ChangeParticleSystemColor(drop.element, ps);
            return;
        }

        Renderer renderer = instance.GetComponentInChildren<Renderer>();
        if (renderer != null && drop.element != Element.None)
        {
            Material glowMaterial = ColorChanger.GetGlowStrengthByElement(drop.element);
            ColorChanger.ChangeMatByAffinity(renderer, glowMaterial);
        }
    }

    /// <summary>The network id of a dropped loot object, if it was dropped by an enemy.</summary>
    public bool TryGetLootId(GameObject loot, out uint lootId)
    {
        return idByLoot.TryGetValue(loot, out lootId);
    }

    /// <summary>Host side: whether the loot is still on the ground, so the first player to ask gets it.</summary>
    public bool IsLootAvailable(uint lootId)
    {
        return lootById.ContainsKey(lootId);
    }

    /// <summary>
    /// Removes a dropped loot object and adds it to this machine's player's inventory.
    /// Online, every machine calls this when any player picks the loot up.
    /// </summary>
    public void CollectLoot(uint lootId)
    {
        if (!lootById.TryGetValue(lootId, out GameObject loot)) return;
        lootById.Remove(lootId);
        if (loot == null) return;
        idByLoot.Remove(loot);

        if (loot.TryGetComponent(out I_LootInterface lootable)) lootable.Loot();
        else Destroy(loot);
    }

    private int GetEtherIndex(float etherValue)
    {
        if (etherValue > 150) return 2; // IntenseEtherPrefab
        else if (etherValue > 100) return 1; // PowerfulEtherPrefab
        else return 0; // FaintEtherPrefab
    }

    private AlchemyLoot GetOrganType(float HP)
    {

        if (HP > 150) return AlchemyLoot.HulkingOrgans;
        else if (HP > 100) return AlchemyLoot.FunctionalOrgans;
        else if (HP > 50) return AlchemyLoot.WretchedOrgans;
        else return AlchemyLoot.WeakCore;
    }

    private AlchemyLoot GetCoreType(float power)
    {
        float lootRoll = Random.Range(0, 120) + (power * LootScaler);
        if (lootRoll < 50) return AlchemyLoot.WretchedOrgans;
        if (lootRoll > 40 && lootRoll < 80) return AlchemyLoot.WeakCore;
        if (lootRoll > 80 && lootRoll < 120) return AlchemyLoot.SolidCore;
        if (lootRoll > 120 && lootRoll < 160) return AlchemyLoot.PowerfulCore;
        if (lootRoll > 160) return AlchemyLoot.HulkingCore;

        else return AlchemyLoot.WretchedOrgans;
    }
}
