using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SummonSupportEvents;
using Unity.Collections;
using Unity.VisualScripting;
using UnityEngine;
using static AbilityLibrary_SO;

public class EnemySpawnHandler : MonoBehaviour
{
    [field: SerializeField] public bool Active { get; private set; } = false;

    [field: SerializeField] public GameObject[] enemyPrefab { get; private set; } = new GameObject[3];
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

    #region Setup
    void Start()
    {
        SetSpawnProbabilities(Difficulty.Novice);
        if (Active) InvokeRepeating("SpawnEnemies", 0f, 10f);
    }


    void OnEnable()
    {
        EventDeclarer.SpawnEnemies.AddListener(SpawnEnemies);
    }
    void OnDisable()
    {
        EventDeclarer.SpawnEnemies.RemoveListener(SpawnEnemies);
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
    #endregion

    #region Spawn logic
    public void SpawnEnemies(SpawnLocationInfo spawnInfo = null)
    {
        StartCoroutine(SpawnWavesOfEnemies(spawnInfo));
    }
    private IEnumerator SpawnWavesOfEnemies(SpawnLocationInfo spawnInfo = null)
    {
        if (spawnInfo == null) spawnInfo = SpawnCenter.GetComponentInChildren<SpawnLocationInfo>();
        if (spawnInfo != null & spawnInfo.Creatures.Length > 0)
        {
            for (int i = spawnInfo.Waves; i > 0; i--)
            {
                Debug.Log($"Spawning wave {i}");
                List<GameObject> spawnedCreatures = new();

                List<Vector2> spawnLocs = GetSpawnLocations(SpawnCenter.transform.position, spawnInfo.Radius, spawnInfo.CreaturesPerWave);
                foreach (Vector2 loc in spawnLocs)
                {
                    GameObject SpawnedCreature = Instantiate(spawnInfo.Creatures[UnityEngine.Random.Range(0, spawnInfo.Creatures.Length)], loc, Quaternion.identity);
                    int level = GetCreatureLevel();
                    ModifyCreatureStats(level, SpawnedCreature.GetComponent<LivingBeing>(), spawnInfo);
                    spawnedCreatures.Add(SpawnedCreature);
                }
                if (spawnInfo.MoveTowardLocation && spawnInfo.TargetLocation != null)
                {
                    foreach (GameObject creature in spawnedCreatures)
                    {
                        MoveTowardTarget(creature, spawnInfo.TargetLocation.position);
                    }
                }
                yield return new WaitForSeconds(spawnInfo.SecondsPerWave);
            }
        }
        else Debug.Log($"The invoked spawn center {spawnInfo} contains no creatures.");
    }
    private void SpawnEnemies()
    {
        SpawnLocationInfo spawnInfo = SpawnCenter.GetComponentInChildren<SpawnLocationInfo>();
        if (spawnInfo != null & spawnInfo.Creatures.Length > 0)
        {
            List<GameObject> spawnedCreatures = new();

            List<Vector2> spawnLocs = GetSpawnLocations(SpawnCenter.transform.position, spawnInfo.Radius, spawnInfo.CreaturesPerWave);
            foreach (Vector2 loc in spawnLocs)
            {
                GameObject SpawnedCreature = Instantiate(spawnInfo.Creatures[UnityEngine.Random.Range(0, spawnInfo.Creatures.Length)], loc, Quaternion.identity);
                int level = GetCreatureLevel();
                ModifyCreatureStats(level, SpawnedCreature.GetComponent<LivingBeing>(), spawnInfo);
                spawnedCreatures.Add(SpawnedCreature);
                Debug.Log($"{SpawnedCreature} lives and breathes");
            }
            if (spawnInfo.MoveTowardLocation && spawnInfo.TargetLocation != null)
            {
                foreach (GameObject creature in spawnedCreatures)
                {
                    MoveTowardTarget(creature, spawnInfo.TargetLocation.position);
                }
            }
        }
    }
    private void MoveTowardTarget(GameObject creature, Vector2 loc)
    {
        if (creature.TryGetComponent(out AIStateHandler stateHandler))
        {
            stateHandler.lastSeenLoc = loc;
            stateHandler.SetTarget(PlayerStats.Instance);
            stateHandler.SetCurrentState(creature.GetComponent<AIChaseState>());
        }
        //if (creature.TryGetComponent(out Collider collider))
        //{
        //    collider.isTrigger = true; // turned to false in the one way barrier script
        //}
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

    #endregion

    #region Creature Modification and Specialization
    public void ModifyCreatureStats(int level, LivingBeing originalCreatureStats, SpawnLocationInfo spawnInfo = null) // should pass the stats of a creature which has already been instantiated
    {
        Element element = SelectCreatureElement(spawnInfo);
        PhysicalType physical = SelectCreaturePhysical(spawnInfo);
        if (level > 0)
        {
            //Debug.Log($"modifying stats to be {level}");
            originalCreatureStats.SetAttribute(AttributeType.MaxHitpoints, originalCreatureStats.GetAttribute(AttributeType.MaxHitpoints) * Hp_Scalar * level);
            originalCreatureStats.SetAttribute(AttributeType.MaxPower, originalCreatureStats.GetAttribute(AttributeType.MaxPower) * Power_Scalar * level);
            originalCreatureStats.RestoreResources();
            originalCreatureStats.ChangeRegeneration(AttributeType.CurrentHitpoints, originalCreatureStats.HealthRegeneration * Regen_Scalar * level);
            originalCreatureStats.ChangeRegeneration(AttributeType.CurrentPower, originalCreatureStats.PowerRegeneration * Regen_Scalar * level);
            originalCreatureStats.ChangePhysicalResistance(physical, 2 * level * Affinity_Scalar);
            originalCreatureStats.ChangeAffinity(element, 2 * level * Affinity_Scalar);
        }
        ModifyCreatureAbilties(level, originalCreatureStats, element, physical);


    }
    private Element SelectCreatureElement(SpawnLocationInfo spawnInfo = null)
    {
        if (spawnInfo == null || spawnInfo.PreferedElement == Element.None)
            return Element.None;
        else return spawnInfo.PreferedElement;
    }
    private PhysicalType SelectCreaturePhysical(SpawnLocationInfo spawnInfo = null)
    {
        if (spawnInfo == null || spawnInfo.PreferedPhysical == PhysicalType.None)
            return PhysicalType.None;
        else return spawnInfo.PreferedPhysical;
    }
    public void ModifyCreatureAbilties(int level, LivingBeing livingBeing, Element element, PhysicalType physical)
    {
        Debug.Log("Spawning creature of level " + level);
        if (level < 2) AlchemyHandler.Instance.AddMeleeAbilityByElement(livingBeing);
        else if (element != Element.None) AddElementalAbilities(level, livingBeing, element);
        if (physical != PhysicalType.None) AddPhysicalAbilities(level, livingBeing, physical);
        else livingBeing.abilityHandler.LearnAbility(AbilityLibrary.abilityLibrary.defaultAttack);
    }
    private void AddElementalAbilities(int level, LivingBeing livingBeing, Element element)
    {
        CreatureAbilityHandler abilityHandler = livingBeing.GetComponent<CreatureAbilityHandler>();
        if (UnityEngine.Random.value > .5)
        {
            //Ability elementalAbility = SetupManager.Instance.ElementToAbilityLibrary_SO.GetAbilityOfElementType(element);
            foreach (Ability ability in AbilityLibrary.GetRandomAbilities(element, level + 1))
            {
                abilityHandler.LearnAbility(ability);
            }
        }
        else abilityHandler.LearnAbility(AbilityLibrary.GetElementalMeleeAbility(element, 50));
    }
    private void AddPhysicalAbilities(int level, LivingBeing livingBeing, PhysicalType physical)
    {
        CreatureAbilityHandler abilityHandler = livingBeing.GetComponent<CreatureAbilityHandler>();
        Debug.Log($"learning a new ability for {livingBeing.Name}");
        foreach (Ability ability in AbilityLibrary.GetRandomAbilities(physical, level + 1))
            livingBeing.GetComponent<CreatureAbilityHandler>().LearnAbility(ability);
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

    #endregion
}

