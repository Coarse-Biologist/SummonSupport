#region Imports
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using System;
using SummonSupportEvents;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using Unity.Entities.UniversalDelegates;
using SS_Structs;


#endregion
public class AlchemyHandler : MonoBehaviour
{
    #region Class Variables
    public string minionPrefabAddress { private set; get; } = "Assets/Prefabs/AIPrefab/MinionPrefab2.prefab";
    private GameObject craftedMinion;
    public GameObject minionPrefab;
    //[field: Tooltip("The amount of minion HP per new extra ability they can use.")]
    [field: SerializeField] public int ManaToAbilityRatio { get; private set; } = 50;
    [field: SerializeField] public float RecycleExchangeRate { get; private set; } = .05f;
    [field: SerializeField] public float KnowledgeGainRate { get; private set; } = 1f;
    [field: SerializeField] public float HealthScalar { get; private set; } = 2f;
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
        Debug.Log($"active minions at awake: {activeMinions.Count}");
    }


    #region Crafting Minion

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

                craftingResults += UpgradeMinion(craftedMinion.GetComponent<LivingBeing>(), combinedPotential, elementList);
                AddActiveMinion(craftedMinion);


                int knowledgeGain = AlchemyInventory.GainKnowledge(elementList, combinedPotential);

                EventDeclarer.RepeatableQuestCompleted?.Invoke(Quest.RepeatableAccomplishments.KnowledgeGained, knowledgeGain);

                craftingResults += $"You have gained {knowledgeGain} alchemic knowledge.\n";
                //Logging.Info($"You have just gained a total of {knowledgeGain} knowledge from alchemic work.");
            }
            else Logging.Error("Crafted Minion is null, was he loaded promtly or correctly?");
        }
        return craftingResults;
    }

    public static void HandleMinionRecycling(LivingBeing minion)
    {
        if (minion == null) throw new SystemException("Minionstats is null when trying to recycle a minion");
        EventDeclarer.minionRecycled?.Invoke(minion.gameObject);

        //#TODO examine closely and maybe make this all better

        float minionPower = minion.MaxHP - 100f;
        float minionHP = minion.MaxHP - 100f;
        float minionAffinity = GetCombinedElementValues(minion);

        AlchemyInventory.AlterCraftingPotential(CraftingPotential.CorePower, (int)(minionPower * Instance.RecycleExchangeRate)); //This should all scale in a more satosfying way
        AlchemyInventory.AlterCraftingPotential(CraftingPotential.EtherDensity, (int)(minionAffinity * Instance.RecycleExchangeRate));
        AlchemyInventory.AlterCraftingPotential(CraftingPotential.OrganMass, (int)(minionHP * Instance.RecycleExchangeRate));

        EventDeclarer.minionDied?.Invoke(minion.gameObject);
        minion.Die();
    }
    private static int GetCombinedElementValues(LivingBeing stats)
    {
        int combinedAffinity = 0;
        foreach (Element element in Enum.GetValues(typeof(Element)))
        {
            combinedAffinity += stats.GetAffinity(element);
        }
        return combinedAffinity;
    }
    private int HandleOrganUse(LivingBeing stats, int value)
    {
        int healthUpgrade = 0;

        EventDeclarer.RepeatableQuestCompleted?.Invoke(Quest.RepeatableAccomplishments.OrganMassUsed, value);
        stats.ChangeAttribute(AttributeType.MaxHitpoints, value);
        healthUpgrade += (int)(value * HealthScalar);
        AlterSizeByOrganNum(stats, value);

        return healthUpgrade;
    }
    private void AlterSizeByOrganNum(LivingBeing stats, int organValue)
    {
        float sizeChangeScalar = organValue * SizeScalar;
        if (sizeChangeScalar > 1)
        {
            Debug.Log($"changing sie of {stats.Name} by {sizeChangeScalar}");
            stats.gameObject.transform.localScale *= sizeChangeScalar;
        }
    }
    private int HandleCoreUse(LivingBeing stats, int value)
    {

        int powerUpgrade = 0;

        EventDeclarer.RepeatableQuestCompleted?.Invoke(Quest.RepeatableAccomplishments.CorePowerUsed, value); //#TODO this should be elswehere

        stats.ChangeAttribute(AttributeType.MaxPower, value);
        powerUpgrade += value;

        return powerUpgrade;
    }
    private int HandleEtherUse(LivingBeing stats, int value, List<Element> elementList)
    {
        if (elementList.Count() == 0) return 0;

        int elementUpgrade = 0;

        EventDeclarer.RepeatableQuestCompleted?.Invoke(Quest.RepeatableAccomplishments.EtherDensityUsed, value); // #TODO this ought be elswhere

        foreach (Element element in elementList)
        {
            stats.ChangeAffinity(element, value / elementList.Count);
            elementUpgrade += value;
        }

        return elementUpgrade;
    }

    public string UpgradeMinion(LivingBeing minion, Dictionary<CraftingPotential, int> potentialUsed, List<Element> elementList)
    {
        string upgradeResults = "";
        MinionStats stats = minion.GetComponent<MinionStats>();
        int healthUpgrade = 0;
        int powerUpgrade = 0;
        int elementUpgrade = 0;

        healthUpgrade += HandleOrganUse(minion, potentialUsed[CraftingPotential.OrganMass]);
        powerUpgrade += HandleCoreUse(stats, potentialUsed[CraftingPotential.CorePower]);

        elementUpgrade += HandleEtherUse(stats, potentialUsed[CraftingPotential.EtherDensity], elementList);

        upgradeResults += $"Health upgraded by {healthUpgrade} \n";
        upgradeResults += $"Power upgraded by {powerUpgrade} \n";
        upgradeResults += $"Elemental affinity upgraded by {elementUpgrade} \n";
        stats.RestoreResources();
        AlterMinionByElement(minion);
        AddAbilitiesByElement(minion, (int)stats.GetAttribute(AttributeType.MaxPower));
        AddMeleeAbilityByElement(stats);
        return upgradeResults;
    }

    private void AddAbilitiesByElement(LivingBeing livingBeing, int minionPower)
    {

        int abilitySlotsToAdd = (int)minionPower / 100;
        Debug.Log($"ability slots to add: {abilitySlotsToAdd}. power used: {minionPower}");

        CreatureAbilityHandler abilityHandler = livingBeing.gameObject.GetComponent<CreatureAbilityHandler>();

        for (int i = abilitySlotsToAdd; i > 0; i--)
        {
            abilityHandler.AddAbilitySlot();
        }
        int abilitiesAdded = 0;
        if (abilityHandler == null) return;
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
                    abilityHandler.LearnAbility(ability);
                    abilityHandler.SlotAbility(ability, abilitiesAdded);
                    abilitiesAdded++;
                }
            }
        }
        abilityHandler.SetAbilityLists();
    }

    public void AddMeleeAbilityByElement(LivingBeing livingBeing)
    {
        CreatureAbilityHandler abilityHandler = livingBeing.GetComponent<CreatureAbilityHandler>();
        if (abilityHandler != null)
        {

            Element strongestElement = livingBeing.GetHighestAffinity(out float value);
            //Debug.Log($"Strongest Element : {strongestElement}, Value : {value}");
            Ability meleeAbility = AbilityLibrary.abilityLibrary.defaultAttack;

            if (strongestElement != Element.None)
            {
                meleeAbility = AbilityLibrary.GetElementalMeleeAbility(strongestElement, value);
                //Debug.Log($"Setting melee ability to {meleeAbility.Name}");
            }
            abilityHandler.LearnAbility(meleeAbility);

            abilityHandler.SetAbilityLists();
        }

    }

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
            Debug.Log($"Indded adding Active minion: {livingBeing.name}");

            activeMinions.Add(livingBeing);
            EventDeclarer.newMinionAdded?.Invoke(livingBeing);
        }
    }
    public void RemoveActiveMinion(LivingBeing livingBeing)
    {
        activeMinions.Remove(livingBeing);

    }

    #endregion




    #region Load Prefab
    public void LoadMinionPrefab(string address)
    {
        Addressables.LoadAssetAsync<GameObject>(address).Completed += handle =>
            {
                if (handle.Status == AsyncOperationStatus.Succeeded)
                {
                    craftedMinion = handle.Result;
                    minionPrefab = handle.Result;

                    //Debug.Log($"Loaded: {address}");
                }
                //else Debug.Log($"address {address} failed to Load");
            };
    }

    #endregion
}
