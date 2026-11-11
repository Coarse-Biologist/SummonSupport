using SummonSupportEvents;
using UnityEngine;

public class LootSpawnHandler : MonoBehaviour
{
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



    void OnEnable()
    {
        EventDeclarer.EnemyDefeated?.AddListener(DecideDropLoot);
    }
    void OnDisable()
    {
        EventDeclarer.EnemyDefeated?.AddListener(DecideDropLoot);
    }

    private void DecideDropLoot(EnemyStats enemyStats)
    {
        Element strongestElement = enemyStats.GetHighestAffinity(out float value);
        if (value * LootScaler >= Random.Range(0, 100))
        {
            AlchemyInventory.IncemementElementalKnowledge(strongestElement, KnowledgeGainRate);
            FloatingInfoHandler.Instance.DisplayGoldenLetters($"+{KnowledgeGainRate} {strongestElement} knowledge!", 1.5f);
        }
        if (value * LootScaler >= Random.Range(0, 100))
        {
            SpawnEther(enemyStats, GetEtherType(value), strongestElement);
        }
        if (enemyStats.MaxHP * LootScaler > Random.Range(0, 100))
        {
            SpawnOrgans(enemyStats, GetOrganType(enemyStats.MaxHP));
        }
        if (enemyStats.MaxPower * LootScaler > Random.Range(0, 100))
        {
            SpawnCores(enemyStats, GetCoreType(enemyStats.MaxPower));
        }
    }
    private void SpawnEther(EnemyStats enemyStats, GameObject etherPrefab, Element element)
    {

        if (etherPrefab == null) return;

        GameObject instance = Instantiate(etherPrefab, enemyStats.transform.position, Quaternion.identity);

        if (instance.TryGetComponent(out LootableAlchemyMaterial lootScript))
        {
            lootScript.SetElement(element);
        }
        if (instance.TryGetComponent(out ParticleSystem ps))
        {
            ColorChanger.ChangeParticleSystemColor(enemyStats, ps);
        }
    }

    private GameObject GetEtherType(float etherValue)
    {

        if (etherValue > 150) return IntenseEtherPrefab;
        else if (etherValue > 100) return PowerfulEtherPrefab;
        else return FaintEtherPrefab;
    }

    private void SpawnOrgans(EnemyStats enemy, AlchemyLoot organType)
    {
        //Debug.Log("Spawning organ!");
        if (organType == AlchemyLoot.WeakCore) return;
        GameObject randomOrgan = Instantiate(OrganPrefabs[Random.Range(0, 2)], enemy.transform.position, Quaternion.identity);
        //Debug.Log("Indeed, Spawning organ");

        if (randomOrgan.TryGetComponent(out LootableAlchemyMaterial lootScript))
        {
            lootScript.SetAlchemyMaterial(organType);
        }
    }

    private AlchemyLoot GetOrganType(float HP)
    {

        if (HP > 150) return AlchemyLoot.HulkingOrgans;
        else if (HP > 100) return AlchemyLoot.FunctionalOrgans;
        else if (HP > 50) return AlchemyLoot.WretchedOrgans;
        else return AlchemyLoot.WeakCore;
    }

    private void SpawnCores(EnemyStats enemy, AlchemyLoot coreType)
    {

        if (coreType == AlchemyLoot.WretchedOrgans) return;
        GameObject instance = Instantiate(CorePrefab, enemy.transform.position, Quaternion.identity);
        //Debug.Log("Indeed, Spawning Core");

        if (instance.TryGetComponent(out LootableAlchemyMaterial lootScript))
        {
            lootScript.SetAlchemyMaterial(coreType);
        }
        Renderer renderer = instance.GetComponentInChildren<Renderer>();
        Element strongestElement = enemy.GetHighestAffinity(out float value);

        if (renderer != null && strongestElement != Element.None)
        {
            //Debug.Log("trying to change core color");
            Material glowMaterial = ColorChanger.GetGlowStrengthByElement(strongestElement);
            ColorChanger.ChangeMatByAffinity(renderer, glowMaterial);
        }
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
