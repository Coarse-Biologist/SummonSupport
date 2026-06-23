#region Imports
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;
using SummonSupportEvents;



#endregion
public class AlchemyHandler : MonoBehaviour
{
    #region Class Variables
    public string minionPrefabAddress { private set; get; } = "Assets/Prefabs/AIPrefab/MinionPrefab2.prefab";
    private GameObject craftedMinion;
    public GameObject minionPrefab;
    //[field: Tooltip("The amount of minion HP per new extra ability they can use.")]
    [field: SerializeField] public static int ManaToAbilityRatio { get; private set; } = 50;
    [field: SerializeField] public static float RecycleExchangeRate { get; private set; } = .2f;
    [field: SerializeField] public static float KnowledgeGainRate { get; private set; } = .03f;
    [field: SerializeField] public static float HealthScalar { get; private set; } = 2f;
    [field: SerializeField] public static int ElementThreshhold { get; private set; } = 50;
    [field: SerializeField] public static float AffinityToColorScalar { private set; get; } = .01f; // likelihood per affinity that a portion of the elemental will show affinity in their material.



    [field: SerializeField] public float SizeScalar { get; private set; } = .1f;
    [SerializeField] public List<LivingBeing> activeMinions = new List<LivingBeing>();
    public static AlchemyHandler Instance { get; private set; }
    public static Dictionary<AlchemyLoot, int> AlchemyLootValueDict = new()
    {
        { AlchemyLoot.WretchedOrgans, 5 },
        { AlchemyLoot.FunctionalOrgans, 10 },
        { AlchemyLoot.HulkingOrgans, 20 },
        { AlchemyLoot.WeakCore, 5 },
        { AlchemyLoot.SolidCore, 10 },
        { AlchemyLoot.PowerfulCore, 20 },
        { AlchemyLoot.HulkingCore, 30 },
        { AlchemyLoot.FaintEther, 10 },
        { AlchemyLoot.PureEther, 30 },
        { AlchemyLoot.IntenseEther, 50 }
    };


    [field: SerializeField] public Transform SpawnLocation { get; private set; }


    #endregion

    public void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    #region Crafting Minion

    #region HandleCraftingResults
    /// <summary>
    /// Crafts a minion and spawns it by the alchemy bench.
    /// </summary>
    /// <param name="combinedPotential">The enemy to damage.</param>
    /// <param name="elementList">The enemy to damage.</param>
    /// <returns> A description string of the resuls of crafting.</returns>
    public string HandleCraftingResults(Dictionary<CraftingPotential, int> combinedPotential, List<Element> elementList)
    {
        string craftingResults = "";

        AlchemyInventory.ExpendCraftingPotential(combinedPotential);

        if (combinedPotential.Keys.Count > 0)
        {
            if (minionPrefab != null)
            {
                Vector3 spawnPos = PlayerStats.Instance.transform.position;
                if (SpawnLocation != null) spawnPos = SpawnLocation.position;
                craftedMinion = Instantiate(minionPrefab, spawnPos, Quaternion.identity);
                MinionStats minionStats = craftedMinion.GetComponent<MinionStats>();
                craftingResults += UpgradeMinion(minionStats, combinedPotential, elementList);
                AddAbilitiesToCraftedMinion(minionStats);

                AddActiveMinion(craftedMinion);

                int knowledgeGain = AlchemyInventory.GainKnowledge(elementList, combinedPotential);

                EventDeclarer.RepeatableQuestCompleted?.Invoke(Quest.RepeatableAccomplishments.KnowledgeGained, knowledgeGain);

                craftingResults += $"You have gained {knowledgeGain} alchemic knowledge.\n";
                //Logging.Info($"You have just gained a total of {knowledgeGain} knowledge from alchemic work.");
            }
            else throw new Exception($"Minion prefab was null when trying to craft minion");
        }
        return craftingResults;
    }
    #endregion

    public static void HandleMinionRecycling(MinionStats minionStats)
    {
        if (minionStats == null) throw new SystemException("Minionstats is null when trying to recycle a minion");
        EventDeclarer.minionRecycled?.Invoke(minionStats.gameObject);

        //#TODO examine closely and maybe make this all better

        float minionPower = minionStats.MaxHP - 100f;
        float minionHP = minionStats.MaxHP - 100f;
        float minionAffinity = GetCombinedElementValues(minionStats);

        AlchemyInventory.AlterCraftingPotential(CraftingPotential.CorePower, (int)(minionPower * RecycleExchangeRate)); //This should all scale in a more satosfying way
        AlchemyInventory.AlterCraftingPotential(CraftingPotential.EtherDensity, (int)(minionAffinity * RecycleExchangeRate));
        AlchemyInventory.AlterCraftingPotential(CraftingPotential.OrganMass, (int)(minionHP * RecycleExchangeRate));

        minionStats.Die();
    }
    private static int GetCombinedElementValues(MinionStats stats)
    {
        int combinedAffinity = 0;
        foreach (Element element in Enum.GetValues(typeof(Element)))
        {
            combinedAffinity += stats.GetAffinity(element);
        }
        return combinedAffinity;
    }
    private int HandleOrganUse(MinionStats stats, int value)
    {
        int healthUpgrade = 0;

        EventDeclarer.RepeatableQuestCompleted?.Invoke(Quest.RepeatableAccomplishments.OrganMassUsed, value);
        stats.ChangeAttribute(AttributeType.MaxHitpoints, value);
        healthUpgrade += (int)(value * HealthScalar);
        AlterSizeByOrganNum(stats, value);

        return healthUpgrade;
    }
    private void AlterSizeByOrganNum(MinionStats stats, int organValue)
    {
        float sizeChangeScalar = organValue * SizeScalar;
        if (sizeChangeScalar > 1)
        {
            Debug.Log($"changing sie of {stats.Name} by {sizeChangeScalar}");
            stats.gameObject.transform.localScale *= sizeChangeScalar;
        }
    }
    private int HandleCoreUse(MinionStats stats, int value)
    {
        int powerUpgrade = 0;

        EventDeclarer.RepeatableQuestCompleted?.Invoke(Quest.RepeatableAccomplishments.CorePowerUsed, value); //#TODO this should be elswehere

        stats.ChangeAttribute(AttributeType.MaxPower, value);
        powerUpgrade += value;

        return powerUpgrade;
    }
    private int HandleEtherUse(MinionStats stats, int value, List<Element> elementList)
    {
        if (elementList.Count() == 0) return 0;

        int elementUpgrade = 0;

        EventDeclarer.RepeatableQuestCompleted?.Invoke(Quest.RepeatableAccomplishments.EtherDensityUsed, value); // #TODO this ought be elswhere
        foreach (Element element in elementList)
        {
            int moddedValue = value * (AlchemyInventory.GetElementalKnowledge(element) / 100);
            stats.ChangeAffinity(element, moddedValue / elementList.Count);
            elementUpgrade += value;
        }

        return elementUpgrade;
    }

    public string UpgradeMinion(MinionStats minionStats, Dictionary<CraftingPotential, int> potentialUsed, List<Element> elementList)
    {
        string upgradeResults = "";
        int healthUpgrade = 0;
        int powerUpgrade = 0;
        int elementUpgrade = 0;

        healthUpgrade += HandleOrganUse(minionStats, potentialUsed[CraftingPotential.OrganMass]);
        powerUpgrade += HandleCoreUse(minionStats, potentialUsed[CraftingPotential.CorePower]);

        elementUpgrade += HandleEtherUse(minionStats, potentialUsed[CraftingPotential.EtherDensity], elementList);

        upgradeResults += $"Health upgraded by {healthUpgrade} \n";
        upgradeResults += $"Power upgraded by {powerUpgrade} \n";
        upgradeResults += $"Elemental affinity upgraded by {elementUpgrade} \n";
        minionStats.RestoreResources();
        AlterMinionByElement(minionStats);

        return upgradeResults;
    }
    #region Give Minions Abilities
    private void AddAbilitiesToCraftedMinion(MinionStats minionStats)
    {
        CreatureAbilityHandler abilityHandler = minionStats.GetComponent<CreatureAbilityHandler>();
        Ability meleeAbility = GetMeleeAbilityByElement(minionStats);
        abilityHandler.LearnAbility(meleeAbility);
        abilityHandler.AddAbilitySlot(0, meleeAbility);
        for (int i = (int)minionStats.GetAttribute(AttributeType.MaxPower) / ManaToAbilityRatio; i > 0; i--)
        {
            abilityHandler.AddAbilitySlot(i, null);
        }
        //List<Ability> abilities = new() { GetMeleeAbilityByElement(minionStats) };
        //foreach (Ability ability in GetAbilitiesByElement(minionStats, (int)minionStats.GetAttribute(AttributeType.MaxPower)))
        //{
        //    abilities.Add(ability);
        //}
        //foreach (Ability ability in abilities)
        //{
        //    abilityHandler.LearnAbility(ability);
        //}
    }

    private List<Ability> GetAbilitiesByElement(LivingBeing livingBeing, int minionPower) //broken
    {
        List<Ability> abilitiesToLearn = new();
        int abilitySlotsToAdd = (int)minionPower / ManaToAbilityRatio;
        Debug.Log($"ability slots to add: {abilitySlotsToAdd}. power used: {minionPower}");

        CreatureAbilityHandler abilityHandler = livingBeing.gameObject.GetComponent<CreatureAbilityHandler>();

        for (int i = abilitySlotsToAdd; i > 0; i--)
        {
            //abilityHandler.AddAbilitySlot();
        }
        int abilitiesAdded = 0;
        if (abilityHandler == null) throw new Exception($"Ability handler is null for {livingBeing.Name}");
        Element strongestElement = livingBeing.GetHighestAffinity(out float value);
        if (strongestElement != Element.None)
        {
            List<Ability> abilities = AbilityLibrary.abilityLibrary.GetElementalAbilitiesBelowLevel(minionPower, new() { strongestElement });

            if (abilities != null)
            {
                foreach (Ability ability in abilities)
                {
                    if (abilityHandler.SlottedAbilities.Count <= abilitiesAdded)
                    {
                        Debug.Log($"Breaking because the number of ability slots is {abilityHandler.SlottedAbilities.Count} and the numbe ralready added was {abilitiesAdded}");
                        break;
                    }
                    abilitiesToLearn.Add(ability);
                    abilitiesAdded++;
                }
            }
        }
        return abilitiesToLearn;
    }

    public Ability GetMeleeAbilityByElement(LivingBeing livingBeing)
    {

        Element strongestElement = livingBeing.GetHighestAffinity(out float value);
        //Debug.Log($"Strongest Element : {strongestElement}, Value : {value}");
        Ability meleeAbility = AbilityLibrary.abilityLibrary.defaultAttack;

        if (strongestElement != Element.None)
        {
            meleeAbility = AbilityLibrary.GetElementalMeleeAbility(strongestElement, value);
            //Debug.Log($"Setting melee ability to {meleeAbility.Name}");
        }
        return meleeAbility;

    }

    #endregion
    private void AlterMinionByElement(LivingBeing stats)
    {
        Element strongestElement = stats.GetHighestAffinity(out float value);
        string nameModifier;
        Debug.Log($"Strongest Element : {strongestElement}, Value : {value}");
        if (strongestElement != Element.None && stats.GetAffinity(strongestElement) > ElementThreshhold)
        {
            Debug.Log($"changing color by {strongestElement} affinity for {stats.Name}");
            ColorChanger.ChangeAllMatsByAffinity(stats);
            nameModifier = strongestElement.ToString();
            stats.SetName(nameModifier + " Elemental");
        }
        else stats.SetName("Flesh Atronach");
    }


    #endregion

    #region set Class Variable functions
    /// <summary>
    /// Called by an Alchemy Inventory when a tool is gained.
    /// Modifies crafting scalars based on the number of tools the player has.
    /// </summary>
    public static void ModifyToolBasedScaling(int numOfToolsKnown)
    {
        ManaToAbilityRatio -= numOfToolsKnown;
        RecycleExchangeRate += (float)(numOfToolsKnown * .04);
        KnowledgeGainRate += (float)(numOfToolsKnown * .01);
        HealthScalar += (float)(numOfToolsKnown * .01);
    }
    public GameObject SpawnMinion(Vector3 location, Quaternion rotation)
    {
        GameObject minion = Instantiate(minionPrefab, location, rotation);

        return minion;
    }
    public void AddActiveMinion(GameObject minion)
    {
        LivingBeing livingBeing = minion.GetComponent<LivingBeing>();
        Debug.Log($"attempting to add Active minion: {livingBeing.name}");

        if (!activeMinions.Contains(livingBeing))
        {
            Debug.Log($"Indeed adding Active minion: {livingBeing.name}");

            activeMinions.Add(livingBeing);
            EventDeclarer.newMinionAdded?.Invoke(livingBeing);
        }
    }
    public void RemoveActiveMinion(LivingBeing livingBeing)
    {
        activeMinions.Remove(livingBeing);

    }

    #endregion




}
