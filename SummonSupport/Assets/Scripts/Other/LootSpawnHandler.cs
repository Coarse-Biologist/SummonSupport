using SummonSupportEvents;
using UnityEngine;

public class LootSpawnHandler : MonoBehaviour
{
    [field: SerializeField] GameObject FaintEtherPrefab;
    [field: SerializeField] GameObject PowerfulEtherPrefab;
    [field: SerializeField] GameObject IntenseEtherPrefab;
    [field: SerializeField] GameObject OrganPrefab;
    [field: SerializeField] GameObject CorePrefab;


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
        Debug.Log("Maybe going to drop some loot, eh?");
        if (enemyStats.GetAffinity(enemyStats.GetHighestAffinity()) > 50)
        {
            // do dice roll for core / ether / organ drop and scales by their Power / Affinity and HP levels.
            // if the resulting value is high enough, spawn tge thing :)
            GameObject instance = Instantiate(FaintEtherPrefab, enemyStats.transform.position, Quaternion.identity);
            if (instance.TryGetComponent<ParticleSystem>(out ParticleSystem ps))
            {
                Debug.Log("particle System found. color being changed i hope");
                EffectColorChanger.ChangeParticleSystemColor(enemyStats, ps);
            }
        }
    }
}
