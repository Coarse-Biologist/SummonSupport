using System;
using System.Collections.Generic;
using SummonSupportEvents;
using Unity.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class EnemySpawnHandler : MonoBehaviour
{
    [field: SerializeField] public GameObject enemyPrefab { get; private set; } = null;
    [field: SerializeField] public GameObject SpawnCenter { get; private set; } = null;
    [field: SerializeField] public List<GameObject> AllCreatures { get; private set; } = new();
    [field: SerializeField] public List<CreatureDict> ElementCreatureDict { get; private set; } = new();

    #region Creature Scaling numbers
    [field: SerializeField] public float Hp_Scalar { get; private set; } = 1.5f;
    [field: SerializeField] public float Power_Scalar { get; private set; } = 1.5f;
    [field: SerializeField] public float Regen_Scalar { get; private set; } = 1.5f;
    [field: SerializeField] public float Damage_Scalar { get; private set; } = 1.5f;
    [field: SerializeField] public float Affinity_Scalar { get; private set; } = 25; // amount of affinity (strongest affinity) gained per difficulty increase

    [field: SerializeField] public float Difficulty_Scalar { get; private set; } = .25f; //Likelihood of each successive creature difficulty spawn is multiplied by this
    [field: SerializeField] public float Balance_Scalar { get; private set; } = .5f; //starting Likelihood of easy creatures to spawn
    [field: SerializeField] public Difficulty Difficulty { get; private set; } = Difficulty.Novice;

    private int lvl2_creatureProbability = 90;
    private int lvl1_creatureProbability = 94;
    private int lvl3_creatureProbability = 97;
    private int lvl4_creatureProbability = 100;






    #endregion

    void Start()
    {
        SetSpawnProbabilities(Difficulty.Novice);
    }


    void OnEnable()
    {
        EventDeclarer.SpawnEnemies.AddListener(SpawnEnemies);
    }
    void OnDisable()
    {
        EventDeclarer.SpawnEnemies.RemoveListener(SpawnEnemies);
    }

    private void SpawnEnemies(SpawnLocationInfo spawnInfo = null)
    {
        List<GameObject> spawnedCreatures = new();
        if (spawnInfo != null & spawnInfo.Creatures.Count > 0)
        {
            List<Vector2> spawnLocs = GetSpawnLocations(SpawnCenter.transform.position, spawnInfo.Radius, spawnInfo.DesiredSpawns);
            foreach (Vector2 loc in spawnLocs)
            {
                GameObject SpawnedCreature = Instantiate(spawnInfo.Creatures[UnityEngine.Random.Range(0, spawnInfo.Creatures.Count)], loc, Quaternion.identity);
                int level = GetCreatureLevel();
                ModifyCreatureStats(level, SpawnedCreature.GetComponent<LivingBeing>());
                spawnedCreatures.Add(SpawnedCreature);
            }
            if (spawnInfo.MoveTowardLocation && spawnInfo.TargetLocation != null)
            {
                foreach (GameObject creature in spawnedCreatures)
                {
                    MoveTowardTarget(creature, spawnInfo.TargetLocation.position);
                }
            }
        }
        else Debug.Log($"The invoked spawn center {spawnInfo} contains no creatures.");
    }
    private void MoveTowardTarget(GameObject creature, Vector2 loc)
    {
        if (creature.TryGetComponent<AIStateHandler>(out AIStateHandler stateHandler))
        {
            stateHandler.lastSeenLoc = PlayerStats.Instance.transform.position;
            stateHandler.SetTarget(PlayerStats.Instance);
            stateHandler.SetCurrentState(creature.GetComponent<AIChaseState>());
        }
        if (creature.TryGetComponent<Collider2D>(out Collider2D collider))
        {
            collider.isTrigger = true; // turned to false in the one way barrier script
        }
    }

    private List<Vector2> GetSpawnLocations(Vector2 origin, float radius, int desiredSpawns)
    {
        if (desiredSpawns == 0)
        {
            desiredSpawns += (int)Difficulty + UnityEngine.Random.Range(0, 3);
        }
        List<Vector2> spawnLocs = new();
        for (int i = 0; i < desiredSpawns; i++)
        {
            spawnLocs.Add(new Vector2(origin.x + GetRandomOffset(radius), origin.y + GetRandomOffset(radius)));
        }
        return spawnLocs;
    }

    private float GetRandomOffset(float radius)
    {
        return UnityEngine.Random.Range(-radius, radius);
    }

    public void ModifyCreatureStats(int level, LivingBeing originalCreatureStats) // should pass the stats of a creature which has already been instantiated
    {
        if (level > 0)
        {
            //Debug.Log($"modifying stats to be {level}");
            originalCreatureStats.SetAttribute(AttributeType.MaxHitpoints, originalCreatureStats.GetAttribute(AttributeType.MaxHitpoints) * Hp_Scalar * level);
            originalCreatureStats.SetAttribute(AttributeType.MaxPower, originalCreatureStats.GetAttribute(AttributeType.MaxPower) * Power_Scalar * level);
            originalCreatureStats.RestoreResources();
            originalCreatureStats.ChangeRegeneration(AttributeType.CurrentHitpoints, originalCreatureStats.HealthRegeneration * Regen_Scalar * level);
            originalCreatureStats.ChangeRegeneration(AttributeType.CurrentPower, originalCreatureStats.PowerRegeneration * Regen_Scalar * level);

            Element strongestElement = originalCreatureStats.GetHighestAffinity();

            if (originalCreatureStats.Affinities[strongestElement].Get() > 0) // if the strongest affinity is non-zero
            {
                originalCreatureStats.ChangeAffinity(strongestElement, level * Affinity_Scalar);
            }

        }
    }

    private void SetSpawnProbabilities(Difficulty gameDifficulty)
    {
        switch (gameDifficulty)
        {
            case Difficulty.Novice:
                lvl1_creatureProbability = 90;
                lvl2_creatureProbability = 94;
                lvl3_creatureProbability = 97;
                lvl4_creatureProbability = 100;
                break;
            case Difficulty.Apprentice:
                lvl1_creatureProbability = 80;
                lvl2_creatureProbability = 85;
                lvl3_creatureProbability = 95;
                lvl4_creatureProbability = 100;
                break;

            case Difficulty.Journeyman:
                lvl1_creatureProbability = 70;
                lvl2_creatureProbability = 80;
                lvl3_creatureProbability = 90;
                lvl4_creatureProbability = 100;
                break;
            case Difficulty.Master:
                lvl1_creatureProbability = 60;
                lvl2_creatureProbability = 70;
                lvl3_creatureProbability = 80;
                lvl4_creatureProbability = 100;
                break;
            case Difficulty.Legend:
                lvl1_creatureProbability = 50;
                lvl2_creatureProbability = 65;
                lvl3_creatureProbability = 75;
                lvl4_creatureProbability = 100;
                break;
            default:
                lvl1_creatureProbability = 90;
                lvl2_creatureProbability = 94;
                lvl3_creatureProbability = 97;
                lvl4_creatureProbability = 100;
                break;
        }
    }

    private int GetCreatureLevel()
    {
        float roll = UnityEngine.Random.Range(0, 100);
        //Debug.Log($"roll = {roll}");
        if (roll < lvl1_creatureProbability) return 0;
        if (roll < lvl2_creatureProbability) return 1;
        if (roll < lvl3_creatureProbability) return 2;
        if (roll < lvl4_creatureProbability) return 3;
        else return 0;
    }
}

