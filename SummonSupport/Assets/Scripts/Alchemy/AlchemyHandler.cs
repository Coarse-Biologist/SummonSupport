#region Imports
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Events;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using System;
//using Unity.VisualScripting.Antlr3.Runtime.Misc;
using SummonSupportEvents;


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
    [field: SerializeField] public static float recycleExchangeRate { get; private set; } = .05f;
    [field: SerializeField] public static float knowledgeGainRate { get; private set; } = 1f;


    [SerializeField] public List<GameObject> activeMinions = new List<GameObject>();
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
        if (AlchemyLootValueDict.TryGetValue(AlchemyLoot.WretchedOrgans, out int num)) return;
        AlchemyLootValueDict.Add(AlchemyLoot.WretchedOrgans, 5);
        AlchemyLootValueDict.Add(AlchemyLoot.FunctionalOrgans, 10);
        AlchemyLootValueDict.Add(AlchemyLoot.HulkingOrgans, 20);
        AlchemyLootValueDict.Add(AlchemyLoot.WeakCore, 5);
        AlchemyLootValueDict.Add(AlchemyLoot.SolidCore, 10);
        AlchemyLootValueDict.Add(AlchemyLoot.PowerfulCore, 20);
        AlchemyLootValueDict.Add(AlchemyLoot.HulkingCore, 30);
        AlchemyLootValueDict.Add(AlchemyLoot.FaintEther, 10);
        AlchemyLootValueDict.Add(AlchemyLoot.PureEther, 30);
        AlchemyLootValueDict.Add(AlchemyLoot.IntenseEther, 50);
    }

    #region Crafting Minion

    public void HandleCraftingResults(Dictionary<AlchemyLoot, int> combinedIngredients, List<Element> elementList)
    {
        if (elementList.Count() == 0)
        {
            combinedIngredients = combinedIngredients
            .Where(g => !g.ToString()
            .Contains("Ether"))
            .ToDictionary(g => g.Key, g => g.Value);
        }

        AlchemyInventory.ExpendIngredients(combinedIngredients);

        if (combinedIngredients.Keys.Count > 0)
        {
            if (minionPrefab != null)
            {
                Vector3 playerPos = PlayerStats.Instance.transform.position;
                craftedMinion = Instantiate(minionPrefab, new Vector3(playerPos.x + 10, playerPos.y, playerPos.z + 10), Quaternion.identity);

                UpgradeMinion(craftedMinion, combinedIngredients, elementList);
                AddActiveMinion(craftedMinion);
                //Debug.Log("Add ACtive minion");
                int knowledgeGain = GainKnowledge(elementList, combinedIngredients);

                //Logging.Info($"You have just gained a total of {knowledgeGain} knowledge from alchemic work.");
            }
            else Logging.Error("Crafted Minion is null, was he loaded promtly or correctly?");
        }
    }

    public static void HandleMinionRecycling(GameObject minion)
    {
        EventDeclarer.minionRecycled?.Invoke(minion);

        if (minion.TryGetComponent<MinionStats>(out MinionStats stats))
        {
            float minionPower = stats.MaxHP - 100f;
            float minionHP = stats.MaxHP - 100f;
            float minionAffinity = GetCombinedElementValues(stats);

            AlchemyInventory.AlterIngredientNum(AlchemyLoot.WeakCore, (int)(minionPower * recycleExchangeRate));
            AlchemyInventory.AlterIngredientNum(AlchemyLoot.FaintEther, (int)(minionAffinity * recycleExchangeRate));
            AlchemyInventory.AlterIngredientNum(AlchemyLoot.WretchedOrgans, (int)(minionHP * recycleExchangeRate));
        }
        EventDeclarer.minionDied?.Invoke(minion);
        CommandMinion.RemoveActiveMinions(minion);


        Destroy(minion);
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
    private int HandleOrganUse(LivingBeing stats, KeyValuePair<AlchemyLoot, int> organKvp)
    {
        int healthUpgrade = 0;
        if (AlchemyLootValueDict.TryGetValue(organKvp.Key, out int num))
        {
            EventDeclarer.RepeatableQuestCompleted?.Invoke(Quest.RepeatableAccomplishments.UseOrgans, num);
            stats.ChangeAttribute(AttributeType.MaxHitpoints, num * organKvp.Value);
            healthUpgrade += num * organKvp.Value;
        }
        return healthUpgrade;
    }
    private int HandleCoreUse(LivingBeing stats, KeyValuePair<AlchemyLoot, int> coreKvp)
    {

        int powerUpgrade = 0;

        if (AlchemyLootValueDict.TryGetValue(coreKvp.Key, out int num))
        {
            EventDeclarer.RepeatableQuestCompleted?.Invoke(Quest.RepeatableAccomplishments.UseCores, num);

            stats.ChangeAttribute(AttributeType.MaxPower, num * coreKvp.Value);
            powerUpgrade += num * coreKvp.Value;
        }
        else Debug.Log($"Core = {coreKvp.Key}");
        return powerUpgrade;
    }
    private int HandleEtherUse(LivingBeing stats, KeyValuePair<AlchemyLoot, int> etherKvp, List<Element> elementList)
    {
        if (elementList.Count() == 0) return 0;
        Debug.Log($"ether type : {etherKvp.Key}. Amount : {etherKvp.Value}");
        int elementUpgrade = 0;
        if (AlchemyLootValueDict.TryGetValue(etherKvp.Key, out int num))
        {
            EventDeclarer.RepeatableQuestCompleted?.Invoke(Quest.RepeatableAccomplishments.UseEther, num);

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
            stats.SetName(nameModifier + " Elemental");
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
            EventDeclarer.newMinionAdded?.Invoke(livingBeing);
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
