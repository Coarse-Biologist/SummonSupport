#region Imports
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
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
    [field: SerializeField] public int ManaToAbilityRatio { get; private set; } = 50;
    [field: SerializeField] public float RecycleExchangeRate { get; private set; } = .05f;
    [field: SerializeField] public float KnowledgeGainRate { get; private set; } = 1f;
    [field: SerializeField] public int SizeScalar { get; private set; } = 20;
    [SerializeField] public List<LivingBeing> activeMinions = new List<LivingBeing>();
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

                UpgradeMinion(craftedMinion.GetComponent<LivingBeing>(), combinedIngredients, elementList);
                AddActiveMinion(craftedMinion);
                //Debug.Log("Add ACtive minion");
                int knowledgeGain = GainKnowledge(elementList, combinedIngredients);

                //Logging.Info($"You have just gained a total of {knowledgeGain} knowledge from alchemic work.");
            }
            else Logging.Error("Crafted Minion is null, was he loaded promtly or correctly?");
        }
    }

    public static void HandleMinionRecycling(LivingBeing minion)
    {
        if (minion == null) throw new SystemException("Minionstats is null when trying to recycle a minion");
        EventDeclarer.minionRecycled?.Invoke(minion.gameObject);


        float minionPower = minion.MaxHP - 100f;
        float minionHP = minion.MaxHP - 100f;
        float minionAffinity = GetCombinedElementValues(minion);

        AlchemyInventory.AlterIngredientNum(AlchemyLoot.WeakCore, (int)(minionPower * Instance.RecycleExchangeRate));
        AlchemyInventory.AlterIngredientNum(AlchemyLoot.FaintEther, (int)(minionAffinity * Instance.RecycleExchangeRate));
        AlchemyInventory.AlterIngredientNum(AlchemyLoot.WretchedOrgans, (int)(minionHP * Instance.RecycleExchangeRate));

        EventDeclarer.minionDied?.Invoke(minion.gameObject);
        CommandMinion.RemoveActiveMinions(minion);


        Destroy(minion);
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
    private int HandleOrganUse(LivingBeing stats, KeyValuePair<AlchemyLoot, int> organKvp)
    {
        int healthUpgrade = 0;
        if (AlchemyLootValueDict.TryGetValue(organKvp.Key, out int num))
        {
            EventDeclarer.RepeatableQuestCompleted?.Invoke(Quest.RepeatableAccomplishments.UseOrgans, num);
            stats.ChangeAttribute(AttributeType.MaxHitpoints, num * organKvp.Value);
            healthUpgrade += num * organKvp.Value;
            AlterSizeByOrganNum(stats, num * organKvp.Value);
        }
        return healthUpgrade;
    }
    private void AlterSizeByOrganNum(LivingBeing stats, int organValue)
    {
        float sizeChangeScalar = organValue / SizeScalar;
        if (sizeChangeScalar > 1)
        {
            Debug.Log($"changing sie of {stats.Name} by {sizeChangeScalar}");
            stats.gameObject.transform.localScale *= organValue / SizeScalar;
        }
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

    public void UpgradeMinion(LivingBeing minion, Dictionary<AlchemyLoot, int> ingredients, List<Element> elementList)
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
        AddMeleeAbilityByElement(stats);
    }

    private void AddAbilitiesByElement(LivingBeing livingBeing)
    {
        CreatureAbilityHandler abilityHandler = livingBeing.gameObject.GetComponent<CreatureAbilityHandler>();
        if (abilityHandler == null) return;
        Element strongestElement = livingBeing.GetHighestAffinity(out float value);
        if (strongestElement != Element.None)
        {
            List<Ability> abilities = AbilityLibrary.GetRandomAbilities(strongestElement, (int)(livingBeing.GetAttribute(AttributeType.MaxPower) / ManaToAbilityRatio));

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
    private void AddMeleeAbilityByElement(LivingBeing livingBeing)
    {
        CreatureAbilityHandler abilityHandler = livingBeing.gameObject.GetComponent<CreatureAbilityHandler>();
        if (abilityHandler == null) return;
        Element strongestElement = livingBeing.GetHighestAffinity(out float value);
        if (strongestElement != Element.None)
        {
            Ability meleeAbility = AbilityLibrary.GetElementalMeleeAbility(strongestElement, value);

            if (meleeAbility != null)
            {

                abilityHandler.LearnAbility(meleeAbility);

            }
            abilityHandler.SetAbilityLists();
        }
    }

    private void AlterMinionByElement(LivingBeing stats)
    {
        Element strongestElement = stats.GetHighestAffinity(out float value);
        string nameModifier;

        if (strongestElement != Element.None && stats.GetAffinity(strongestElement) > 50)
        {
            ColorChanger.ChangeAllMatsByAffinity(stats);
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
        if (!activeMinions.Contains(livingBeing))
        {
            activeMinions.Add(livingBeing);
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
