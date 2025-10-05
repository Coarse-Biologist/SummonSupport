using SummonSupportEvents;
using UnityEngine;

public class LootSpawnHandler : MonoBehaviour
{
    [field: SerializeField] GameObject FaintEtherPrefab;
    [field: SerializeField] GameObject PowerfulEtherPrefab;
    [field: SerializeField] GameObject IntenseEtherPrefab;
    [field: SerializeField] GameObject OrganPrefab;
    [field: SerializeField] GameObject CorePrefab;
    [field: SerializeField] float LootScaler = .3f;



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
        float etherValue = enemyStats.GetAffinity(enemyStats.GetHighestAffinity());
        Debug.Log("Maybe going to drop some loot, eh?");
        if (etherValue > 50)
        {

            // do dice roll for core / ether / organ drop and scales by their Power / Affinity and HP levels.
            // if the resulting value is high enough, spawn tge thing :)
            SpawnEther(enemyStats, GetEtherType(etherValue));
        }
        if (enemyStats.MaxHP > 150)
        {
            SpawnOrgans(enemyStats.transform, GetOrganType(enemyStats.MaxHP));
        }
        if (enemyStats.MaxPower > 150)
        {
            SpawnCores(enemyStats.transform, GetCoreType(enemyStats.MaxPower));
        }

    }
    private void SpawnEther(EnemyStats enemyStats, GameObject etherPrefab)
    {
        if (etherPrefab == null) return;
        GameObject instance = Instantiate(etherPrefab, enemyStats.transform.position, Quaternion.identity);
        if (instance.TryGetComponent<ParticleSystem>(out ParticleSystem ps))
        {
            EffectColorChanger.ChangeParticleSystemColor(enemyStats, ps);
        }
    }

    private GameObject GetEtherType(float etherValue)
    {
        float lootRoll = Random.Range(0, 150) + (etherValue * LootScaler);
        Debug.Log($"power = {etherValue}. lootRoll = {lootRoll}");

        if (lootRoll < 50) return null;
        if (lootRoll > 50 && lootRoll < 100) return FaintEtherPrefab;
        if (lootRoll > 100 && lootRoll < 150) return PowerfulEtherPrefab;
        if (lootRoll > 150) return IntenseEtherPrefab;
        else return null;
    }

    private void SpawnOrgans(Transform enemy, AlchemyLoot organType)
    {

        if (organType == AlchemyLoot.WeakCores) return;
        Instantiate(OrganPrefab, enemy.transform.position, Quaternion.identity);
        if (OrganPrefab.TryGetComponent<LootableAlchemyMaterial>(out LootableAlchemyMaterial lootScript))
        {
            lootScript.SetAlchemyMaterial(organType);
        }
    }

    private AlchemyLoot GetOrganType(float HP)
    {
        float lootRoll = Random.Range(0, 150) + (HP * LootScaler);
        Debug.Log($"power = {HP}. lootRoll = {lootRoll}");

        if (lootRoll < 50) return AlchemyLoot.WeakCores;
        if (lootRoll > 50 && lootRoll < 100) return AlchemyLoot.WretchedOrgans;
        if (lootRoll > 100 && lootRoll < 150) return AlchemyLoot.FunctionalOrgans;
        if (lootRoll > 150) return AlchemyLoot.HulkingOrgans;
        else return AlchemyLoot.WeakCores;
    }

    private void SpawnCores(Transform enemy, AlchemyLoot coreType)
    {
        Debug.Log($"core type = {coreType}");
        if (coreType == AlchemyLoot.WretchedOrgans) return;
        Instantiate(CorePrefab, enemy.transform.position, Quaternion.identity);
        if (CorePrefab.TryGetComponent<LootableAlchemyMaterial>(out LootableAlchemyMaterial lootScript))
        {
            lootScript.SetAlchemyMaterial(coreType);
        }
    }

    private AlchemyLoot GetCoreType(float power)
    {
        float lootRoll = Random.Range(0, 150) + (power * LootScaler);
        Debug.Log($"power = {power}. lootRoll = {lootRoll}");
        if (lootRoll < 50) return AlchemyLoot.WretchedOrgans;
        if (lootRoll > 40 && lootRoll < 80) return AlchemyLoot.WeakCores;
        if (lootRoll > 80 && lootRoll < 120) return AlchemyLoot.WorkingCore;
        if (lootRoll > 120 && lootRoll < 160) return AlchemyLoot.PowerfulCore;
        if (lootRoll > 160) return AlchemyLoot.HulkingCore;

        else return AlchemyLoot.WretchedOrgans;
    }
}
