#region Imports
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Events;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using System;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using SummonSupportEvents;
using UnityEditor.EditorTools;

#endregion
public class AlchemyHandler : MonoBehaviour
{
    #region Class Variables
    public string minionPrefabAddress { private set; get; } = "Assets/Prefabs/AIPrefab/MinionPrefab2.prefab";
    private GameObject craftedMinion;
    public GameObject minionPrefab;
    public UnityEvent<GameObject> requestInstantiation = new UnityEvent<GameObject>();
    [field: Tooltip("The amount of minion HP per new extra ability they can use.")]
    [field: SerializeField] public int HPToAbilityRatio { get; private set; } = 50;
    [SerializeField] public List<GameObject> activeMinions = new List<GameObject>();
    public UnityEvent<LivingBeing> newMinionAdded;
    public static AlchemyHandler Instance { get; private set; }

    public static Dictionary<AlchemyLoot, int> AlchemyLootValueDict = new();

    #endregion

    public void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        InitiateAlchemyLootDict();
    }
    private void InitiateAlchemyLootDict()
    {
        AlchemyLootValueDict.Add(AlchemyLoot.WretchedOrgans, 5);
        AlchemyLootValueDict.Add(AlchemyLoot.FunctionalOrgans, 10);
        AlchemyLootValueDict.Add(AlchemyLoot.HulkingOrgans, 20);
        AlchemyLootValueDict.Add(AlchemyLoot.WeakCores, 5);
        AlchemyLootValueDict.Add(AlchemyLoot.WorkingCore, 10);
        AlchemyLootValueDict.Add(AlchemyLoot.PowerfulCore, 20);
        AlchemyLootValueDict.Add(AlchemyLoot.HulkingCore, 30);
        AlchemyLootValueDict.Add(AlchemyLoot.FaintEther, 10);
        AlchemyLootValueDict.Add(AlchemyLoot.PureEther, 30);
        AlchemyLootValueDict.Add(AlchemyLoot.IntenseEther, 50);
    }

    #region Crafting Minion

    public void HandleCraftingResults(Dictionary<AlchemyLoot, int> combinedIngredients, List<Element> elementList)
    {

        if (combinedIngredients.Keys.Count > 0)
        {
            if (minionPrefab != null)
            {
                craftedMinion = Instantiate(minionPrefab, transform.position, Quaternion.identity);

                UpgradeMinion(craftedMinion, combinedIngredients, elementList);
                AddActiveMinion(craftedMinion);
                Debug.Log("Add ACtive minion");
                int knowledgeGain = GainKnowledge(elementList, combinedIngredients);

                //Logging.Info($"You have just gained a total of {knowledgeGain} knowledge from alchemic work.");
            }
            else Logging.Error("Crafted Minion is null, was he loaded promtly or correctly?");
        }
    }
    private int HandleOrganUse(LivingBeing stats, KeyValuePair<AlchemyLoot, int> organKvp)
    {
        EventDeclarer.RepeatableQuestCompleted?.Invoke(Quest.RepeatableAccomplishments.UseOrgans, 1);
        int healthUpgrade = 0;
        if (AlchemyLootValueDict.TryGetValue(organKvp.Key, out int num))
        {
            stats.ChangeAttribute(AttributeType.MaxHitpoints, num * organKvp.Value);
            healthUpgrade += num * organKvp.Value;
        }
        return healthUpgrade;
    }
    private int HandleCoreUse(LivingBeing stats, KeyValuePair<AlchemyLoot, int> coreKvp)
    {
        EventDeclarer.RepeatableQuestCompleted?.Invoke(Quest.RepeatableAccomplishments.UseCores, 1);

        int powerUpgrade = 0;

        if (AlchemyLootValueDict.TryGetValue(coreKvp.Key, out int num))
        {
            stats.ChangeAttribute(AttributeType.MaxPower, num * coreKvp.Value);
            powerUpgrade += num * coreKvp.Value;
        }
        else Debug.Log($"Core = {coreKvp.Key}");
        return powerUpgrade;
    }
    private int HandleEtherUse(LivingBeing stats, KeyValuePair<AlchemyLoot, int> etherKvp, List<Element> elementList)
    {
        EventDeclarer.RepeatableQuestCompleted?.Invoke(Quest.RepeatableAccomplishments.UseEther, 1);
        Debug.Log($"ether type : {etherKvp.Key}. Amount : {etherKvp.Value}");
        int elementUpgrade = 0;
        if (AlchemyLootValueDict.TryGetValue(etherKvp.Key, out int num))
        {
            foreach (Element element in elementList)
            {
                stats.ChangeAffinity(element, num * etherKvp.Value / elementList.Count);
                elementUpgrade += num * etherKvp.Value;
            }
        }


        return elementUpgrade;
    }

    public void UpgradeMinion(GameObject minion, Dictionary<AlchemyLoot, int> ingredients, List<Element> elementList)
    {
        MinionStats stats = minion.GetComponent<MinionStats>();

        foreach (KeyValuePair<AlchemyLoot, int> kvp in ingredients)
        {
            if (kvp.Key.ToString().Contains("Organs")) HandleOrganUse(stats, kvp);
            if (kvp.Key.ToString().Contains("Cores")) HandleCoreUse(stats, kvp);
            if (kvp.Key.ToString().Contains("Ether")) HandleEtherUse(stats, kvp, elementList);
        }
        stats.RestoreResources();
        AlterMinionByElement(minion);
        AddAbilitiesByElement(stats);
    }

    private void AddAbilitiesByElement(LivingBeing livingBeing)
    {
        CreatureAbilityHandler abilityHandler = livingBeing.gameObject.GetComponent<CreatureAbilityHandler>();
        if (abilityHandler == null) return;
        Element strongestElement = livingBeing.GetHighestAffinity();
        if (strongestElement != Element.None)
        {
            List<Ability> abilities = AbilityLibrary.GetRandomAbilities(strongestElement, (int)(livingBeing.GetAttribute(AttributeType.MaxHitpoints) / HPToAbilityRatio));
            Debug.Log(abilities);

            if (abilities != null)
            {
                foreach (Ability ability in abilities)
                {
                    abilityHandler.LearnAbility(ability);
                }

            }
            abilityHandler.SetAbilityLists();
        }
    }

    private void AlterMinionByElement(GameObject minion)
    {
        LivingBeing stats = minion.GetComponent<LivingBeing>();
        CreatureSpriteController spriteControl = minion.GetComponentInChildren<CreatureSpriteController>();
        Element strongestElement = stats.GetHighestAffinity();
        string nameModifier = "";

        if (strongestElement != Element.None && stats.Affinities[strongestElement].Get() > 50)
        {
            spriteControl.AlterColorByAffinity(strongestElement);
            nameModifier = strongestElement.ToString();
            stats.SetName(nameModifier + "Elemental");
        }
        else stats.SetName("Flesh Atronach");
    }


    #endregion

    #region set Class Variable functions
    private void AddActiveMinion(GameObject minion)
    {
        LivingBeing livingBeing = minion.GetComponent<LivingBeing>();
        if (!activeMinions.Contains(minion))
        {
            activeMinions.Add(minion);
            //CommandMinion.SetSelectedMinion(minion); //Good idea?...
            Debug.Log("add active minion func. alchemy handler");
            newMinionAdded?.Invoke(livingBeing);
        }
        CommandMinion.SetActiveMinions(activeMinions);
    }
    #endregion


    private int GainKnowledge(List<Element> elementsList, Dictionary<AlchemyLoot, int> combinedIngredients)
    {
        int total = 0;
        foreach (KeyValuePair<AlchemyLoot, int> kvp in combinedIngredients)
        {
            int strengthAndAmount = AlchemyInventory.ingredientValues[kvp.Key] * kvp.Value; //multiply ingredient value by num used
            strengthAndAmount += AlchemyInventory.KnownTools.Count;
            total += strengthAndAmount;
            foreach (Element element in elementsList)
            {
                AlchemyInventory.IncemementElementalKnowledge(element, strengthAndAmount);
            }
        }
        EventDeclarer.RepeatableQuestCompleted?.Invoke(Quest.RepeatableAccomplishments.GainKnowledge, total);

        return total;
    }

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
