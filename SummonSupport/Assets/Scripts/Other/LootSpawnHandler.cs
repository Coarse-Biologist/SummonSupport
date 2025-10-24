using SummonSupportEvents;
using UnityEngine;

public class LootSpawnHandler : MonoBehaviour
{
    [field: SerializeField] GameObject FaintEtherPrefab;
    [field: SerializeField] GameObject PowerfulEtherPrefab;
    [field: SerializeField] GameObject IntenseEtherPrefab;
    [field: SerializeField] GameObject OrganPrefab;

    #region Cores
    [field: SerializeField] GameObject CorePrefab;
    [field: SerializeField] public Sprite WeakCoreSprite { get; private set; }
    [field: SerializeField] public Sprite WorkingCoreSprite { get; private set; }

    [field: SerializeField] public Sprite PowerfulCoreSprite { get; private set; }

    [field: SerializeField] public Sprite HulkingCoreSprite { get; private set; }


    #endregion
    [field: SerializeField] float LootScaler = 1f;



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
        if (etherValue >= 50)
        {
            SpawnEther(enemyStats, GetEtherType(etherValue));
        }
        if (enemyStats.MaxHP > 150)
        {
            SpawnOrgans(enemyStats, GetOrganType(enemyStats.MaxHP));
        }
        if (enemyStats.MaxPower > 150)
        {
            SpawnCores(enemyStats, GetCoreType(enemyStats.MaxPower));
        }
    }
    private void SpawnEther(EnemyStats enemyStats, GameObject etherPrefab)
    {

        if (etherPrefab == null) return;

        GameObject instance = Instantiate(etherPrefab, enemyStats.transform.position, Quaternion.identity);
        if (instance.TryGetComponent(out LootableAlchemyMaterial lootScript))
        {
            lootScript.SetElement(enemyStats.GetHighestAffinity());
        }
        if (instance.TryGetComponent(out ParticleSystem ps))
        {
            EffectColorChanger.ChangeParticleSystemColor(enemyStats, ps);
        }
    }

    private GameObject GetEtherType(float etherValue)
    {
        float lootRoll = Random.Range(0, 150) + (etherValue * LootScaler);

        if (lootRoll < 50) return null;
        if (lootRoll > 50 && lootRoll < 100) return FaintEtherPrefab;
        if (lootRoll > 100 && lootRoll < 150) return PowerfulEtherPrefab;
        if (lootRoll > 150) return IntenseEtherPrefab;
        else return null;
    }

    private void SpawnOrgans(EnemyStats enemy, AlchemyLoot organType)
    {

        if (organType == AlchemyLoot.WeakCore) return;
        Instantiate(OrganPrefab, enemy.transform.position, Quaternion.identity);
        if (OrganPrefab.TryGetComponent(out LootableAlchemyMaterial lootScript))
        {
            lootScript.SetAlchemyMaterial(organType);
        }
    }

    private AlchemyLoot GetOrganType(float HP)
    {
        float lootRoll = Random.Range(0, 150) + (HP * LootScaler);

        if (lootRoll < 50) return AlchemyLoot.WeakCore;
        if (lootRoll > 50 && lootRoll < 100) return AlchemyLoot.WretchedOrgans;
        if (lootRoll > 100 && lootRoll < 150) return AlchemyLoot.FunctionalOrgans;
        if (lootRoll > 150) return AlchemyLoot.HulkingOrgans;
        else return AlchemyLoot.WeakCore;
    }

    private void SpawnCores(EnemyStats enemy, AlchemyLoot coreType)
    {
        if (coreType == AlchemyLoot.WretchedOrgans) return;
        GameObject instance = Instantiate(CorePrefab, enemy.transform.position, Quaternion.identity);
        if (instance.TryGetComponent<LootableAlchemyMaterial>(out LootableAlchemyMaterial lootScript))
        {
            lootScript.SetAlchemyMaterial(coreType);
        }
        if (instance.TryGetComponent<SpriteRenderer>(out SpriteRenderer sr))
        {
            switch (coreType)
            {
                case AlchemyLoot.WeakCore:
                    sr.sprite = WeakCoreSprite;
                    break;
                case AlchemyLoot.SolidCore:
                    sr.sprite = WorkingCoreSprite;
                    break;
                case AlchemyLoot.PowerfulCore:
                    sr.sprite = PowerfulCoreSprite;
                    break;
                case AlchemyLoot.HulkingCore:
                    sr.sprite = HulkingCoreSprite;
                    break;
            }
            Element strongestElement = enemy.GetHighestAffinity();
            EffectColorChanger.SetColor(sr, EffectColorChanger.GetColorFromElement(strongestElement));
            Material glowMaterial = EffectColorChanger.GetGlowByElement(strongestElement);
            sr.material = glowMaterial;
        }
    }

    private AlchemyLoot GetCoreType(float power)
    {
        float lootRoll = Random.Range(0, 150) + (power * LootScaler);
        if (lootRoll < 50) return AlchemyLoot.WretchedOrgans;
        if (lootRoll > 40 && lootRoll < 80) return AlchemyLoot.WeakCore;
        if (lootRoll > 80 && lootRoll < 120) return AlchemyLoot.SolidCore;
        if (lootRoll > 120 && lootRoll < 160) return AlchemyLoot.PowerfulCore;
        if (lootRoll > 160) return AlchemyLoot.HulkingCore;

        else return AlchemyLoot.WretchedOrgans;
    }
}
