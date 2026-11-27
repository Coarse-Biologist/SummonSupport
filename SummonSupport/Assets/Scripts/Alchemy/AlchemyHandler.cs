#region Imports
using UnityEngine;
using System.Collections;
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
    [field: SerializeField] public static int ManaToRegenRatio { get; private set; } = 100;

    [field: SerializeField] public static int ManaToAbilityRatio { get; private set; } = 100;
    [field: SerializeField] public static float RecycleExchangeRate { get; private set; } = .2f;
    [field: SerializeField] public static float KnowledgeGainRate { get; private set; } = .03f;
    [field: SerializeField] public static float HealthScalar { get; private set; } = 2f;
    [field: SerializeField] public static int ElementThreshhold { get; private set; } = 50;
    [field: SerializeField] public static float AffinityToColorScalar { private set; get; } = .01f; // likelihood per affinity that a portion of the elemental will show affinity in their material.



    [field: SerializeField] public float SizeScalar { get; private set; } = .1f;
    [SerializeField] public List<MinionStats> activeMinions = new List<MinionStats>();
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

    // Online crafting: what the local player paid for each minion it asked the host to spawn.
    private uint nextCraftRequestId;
    private readonly Dictionary<uint, (Dictionary<CraftingPotential, int> potential, List<Element> elements)> pendingCrafts = new();
    // Saved minions that the host is spawning for the local player; built from their profile instead of a craft.
    private readonly Dictionary<uint, MinionProfileData> pendingSavedMinions = new();


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
    void OnEnable()
    {
        EventDeclarer.newMinionAdded?.AddListener(AddActiveMinion);

    }
    void OnDisable()
    {
        EventDeclarer.newMinionAdded?.RemoveListener(AddActiveMinion);
    }

    #region Crafting Minion

    #region HandleCraftingResults
    /// <summary>
    /// Crafts a minion and spawns it by the alchemy bench.
    /// </summary>
    /// <param name="combinedPotential">The selected ingredient types.</param>
    /// <param name="elementList">The list of elements which the ether will affect.</param>
    /// <returns> A description string of the resuls of crafting.</returns>
    /// 
    public string HandleCraftingResults(Dictionary<CraftingPotential, int> combinedPotential, List<Element> elementList)
    {
        string craftingResults = "";

        if (!AlchemyInventory.CanAfford(combinedPotential))
            return "You do not have enough crafting potential for that combination.";

        AlchemyInventory.ExpendCraftingPotential(combinedPotential);

        if (combinedPotential.Keys.Count > 0)
        {
            if (minionPrefab != null)
            {
                Vector3 spawnPos = PlayerStats.Instance.transform.position;
                if (SpawnLocation != null) spawnPos = SpawnLocation.position;

                if (NetworkSession.IsOnline) return RequestNetworkCraft(combinedPotential, elementList, spawnPos);

                craftedMinion = Instantiate(minionPrefab, spawnPos, Quaternion.identity);
                MinionStats minionStats = craftedMinion.GetComponent<MinionStats>();
                craftingResults += UpgradeMinion(minionStats, combinedPotential, elementList);

                AddActiveMinion(minionStats);

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

    /// <summary>
    /// Online: asks the host to spawn a minion that belongs to the local player. When it arrives, the minion is
    /// built from what the player paid (<see cref="CompleteNetworkCraft"/>), and the result is sent to everyone.
    /// </summary>
    private string RequestNetworkCraft(Dictionary<CraftingPotential, int> combinedPotential, List<Element> elementList, Vector3 spawnPos)
    {
        NetworkPlayer requester = PlayerStats.Instance.GetComponent<NetworkPlayer>();
        if (requester == null) throw new Exception("The local player has no NetworkPlayer, so the minion cannot be crafted online.");

        uint requestId = ++nextCraftRequestId;
        pendingCrafts[requestId] = (new Dictionary<CraftingPotential, int>(combinedPotential), new List<Element>(elementList));
        requester.RequestCraftMinion(spawnPos, requestId);

        int knowledgeGain = AlchemyInventory.GainKnowledge(elementList, combinedPotential);
        EventDeclarer.RepeatableQuestCompleted?.Invoke(Quest.RepeatableAccomplishments.KnowledgeGained, knowledgeGain);
        return $"Your minion is taking shape by the bench.\nYou have gained {knowledgeGain} alchemic knowledge.\n";
    }

    /// <summary>Host side: spawns a crafted minion that belongs to the player who crafted it.</summary>
    public void SpawnNetworkedMinion(Vector3 spawnPos, ulong crafterClientId, uint requestId)
    {
        if (minionPrefab == null) throw new Exception($"Minion prefab was null when trying to craft minion");

        GameObject minion = Instantiate(minionPrefab, spawnPos, Quaternion.identity);
        if (!minion.TryGetComponent(out NetworkEnemy networkEnemy) || !minion.TryGetComponent(out Unity.Netcode.NetworkObject networkObject))
        {
            Destroy(minion);
            throw new Exception($"{minionPrefab.name} needs a NetworkObject and a NetworkEnemy to be crafted online.");
        }
        networkEnemy.SetCrafter(crafterClientId, requestId);
        networkObject.Spawn();
    }

    /// <summary>Crafter side: builds a minion the host spawned for the local player, from what the player paid.</summary>
    public void CompleteNetworkCraft(MinionStats minionStats, uint requestId)
    {
        if (pendingSavedMinions.TryGetValue(requestId, out MinionProfileData savedProfile))
        {
            pendingSavedMinions.Remove(requestId);
            BuildSavedMinion(minionStats, savedProfile);
            return;
        }
        if (!pendingCrafts.TryGetValue(requestId, out var craft)) return;
        pendingCrafts.Remove(requestId);

        string results = UpgradeMinion(minionStats, craft.potential, craft.elements);
        AddActiveMinion(minionStats);
        Debug.Log($"Crafted {minionStats.Name}:\n{results}");
        if (FloatingInfoHandler.Instance != null) FloatingInfoHandler.Instance.DisplayGoldenLetters($"{minionStats.Name} has been crafted!", 2f);
    }

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

        // Online, the host runs the minion, so it ends it.
        if (NetworkSession.IsOnline && !NetworkSession.IsAuthority && minionStats.TryGetComponent(out NetworkEnemy networkEnemy) && networkEnemy.IsSpawned)
            networkEnemy.RequestRecycle();
        else
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
            stats.ScaleBody(sizeChangeScalar);
        }
    }
    private int HandleCoreUse(MinionStats stats, int value)
    {
        int powerUpgrade = 0;

        EventDeclarer.RepeatableQuestCompleted?.Invoke(Quest.RepeatableAccomplishments.CorePowerUsed, value); //#TODO this should be elswehere

        stats.ChangeAttribute(AttributeType.MaxPower, value);
        stats.ChangeHealthRegeneration(value / ManaToRegenRatio);
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
            int moddedValue = (int)(value * (AlchemyInventory.GetElementalKnowledge(element) / 100f));
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
        AddAbilitiesToCraftedMinion(minionStats);
        AlterMinionByElement(minionStats);

        return upgradeResults;
    }
    #region Give Minions Abilities
    private void AddAbilitiesToCraftedMinion(MinionStats minionStats)
    {
        CreatureAbilityHandler abilityHandler = minionStats.GetComponent<CreatureAbilityHandler>();
        int powerInt = ((int)minionStats.GetAttribute(AttributeType.MaxPower) / ManaToAbilityRatio) - abilityHandler.SlottedAbilities.Count;
        Debug.Log($"power int = {powerInt} for {minionStats}");
        //for (int i = powerInt; i > 0; i--)
        //{
        //    abilityHandler.AddAbilitySlot(i, null);
        //}
        Element element = minionStats.GetHighestAffinity(out float dontCare);
        if (element != Element.None)
            AbilityLibrary.AddElementalAbilities(powerInt, minionStats, element);
        else AbilityLibrary.AddPhysicalAbilities(powerInt, minionStats, PhysicalType.Slashing);


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
            EventDeclarer.renameMinion.Invoke(stats);
        }
        else stats.SetName("Flesh Atronach");
    }


    #endregion

    #region set Class Variable functions
    /// <summary>
    /// Called by an Alchemy Inventory when a tool is gained.
    /// Modifies crafting scalars based on the number of tools the player has.
    /// </summary>
    /// <summary>Puts the tool-based crafting scalars back to how they are with no tools (see AlchemyInventory.ResetInventory).</summary>
    public static void ResetToolBasedScaling()
    {
        ManaToAbilityRatio = 100;
        RecycleExchangeRate = .2f;
        KnowledgeGainRate = .03f;
        HealthScalar = 2f;
    }

    public static void ModifyToolBasedScaling(int numOfToolsKnown)
    {
        ManaToAbilityRatio -= numOfToolsKnown * 3;
        RecycleExchangeRate += (float)(numOfToolsKnown * .04);
        KnowledgeGainRate += (float)(numOfToolsKnown * .01);
        HealthScalar += (float)(numOfToolsKnown * .01);
    }
    public GameObject SpawnMinion(Vector3 location, Quaternion rotation)
    {
        GameObject minion = Instantiate(minionPrefab, location, rotation);

        return minion;
    }

    /// <summary>
    /// Brings back a minion from a save for the local player. Online the host spawns it, like a crafted minion,
    /// and this machine builds it from the saved profile when it arrives (see CompleteNetworkCraft).
    /// </summary>
    public void RestoreSavedMinion(MinionData minionData)
    {
        if (NetworkSession.IsOnline)
        {
            NetworkPlayer requester = PlayerStats.Instance != null ? PlayerStats.Instance.GetComponent<NetworkPlayer>() : null;
            if (requester == null) return;
            uint requestId = ++nextCraftRequestId;
            pendingSavedMinions[requestId] = minionData.profile;
            requester.RequestCraftMinion(minionData.location, requestId);
            return;
        }

        GameObject minion = SpawnMinion(minionData.location, minionData.rotation);
        if (minion.TryGetComponent(out MinionStats minionStats)) StartCoroutine(BuildSavedMinionWhenReady(minionStats, minionData.profile));
    }

    private IEnumerator BuildSavedMinionWhenReady(MinionStats minionStats, MinionProfileData profile)
    {
        yield return null; // let the minion's Start run first
        if (minionStats != null) BuildSavedMinion(minionStats, profile);
    }

    private void BuildSavedMinion(MinionStats minionStats, MinionProfileData profile)
    {
        profile.ApplyTo(minionStats);
        minionStats.RestoreResources();
        AddActiveMinion(minionStats);
    }
    public void AddActiveMinion(MinionStats minion)
    {
        Debug.Log($"attempting to add Active minion: {minion.name}");

        if (!activeMinions.Contains(minion))
        {
            Debug.Log($"Indeed adding Active minion: {minion.Name}");

            activeMinions.Add(minion);
            EventDeclarer.newMinionAdded?.Invoke(minion);
        }
    }
    public void RemoveActiveMinion(MinionStats minion)
    {
        activeMinions.Remove(minion);

    }

    #endregion




}
