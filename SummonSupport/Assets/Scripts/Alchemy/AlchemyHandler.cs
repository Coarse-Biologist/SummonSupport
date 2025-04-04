#region Imports
using UnityEngine;
using System.Collections.Generic;
using Alchemy;
using System.Linq;
using UnityEngine.Events;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using System;
#endregion
public class AlchemyHandler : MonoBehaviour
{
    #region Class Variables
    public string minionPrefabAddress { private set; get; } = "Assets/Prefabs/AIPrefab/MinionPrefab2.prefab";
    private GameObject craftedMinion;
    public GameObject minionPrefab;
    public UnityEvent<GameObject> requestInstantiation = new UnityEvent<GameObject>();
    [SerializeField] public List<GameObject> activeMinions = new List<GameObject>();
    public UnityEvent<GameObject> newMinionAdded;

    #endregion

    #region Crafting Minion

    public void HandleCraftingResults(Dictionary<AlchemyLoot, int> combinedIngredients, List<Elements> elementList)
    {
        if (combinedIngredients.Keys.Count > 0)
        {
            if (minionPrefab != null)
            {
                craftedMinion = Instantiate(minionPrefab, new Vector2(transform.position.x, transform.position.y), Quaternion.identity);

                UpgradeMinion(craftedMinion, combinedIngredients, elementList);
                AddActiveMinion(craftedMinion);
            }
            else Logging.Error("Crafted Minion is null, was he loaded promtly or correctly?");
        }
    }

    private int HandleOrganUse(LivingBeing stats, AlchemyLoot organ)
    {
        int healthUpgrade = 0;
        string organString = organ.ToString();
        if (organString.Contains("Wretched"))
        {
            stats.ChangeMaxHP(5);
            healthUpgrade += 5;
        }
        if (organString.Contains("Functional"))
        {
            stats.ChangeMaxHP(10);
            healthUpgrade += 10;
        }
        if (organString.Contains("Hulking"))
        {
            stats.ChangeMaxHP(20);
            healthUpgrade += 20;
        }
        return healthUpgrade;
    }
    private int HandleCoreUse(LivingBeing stats, AlchemyLoot core)
    {
        int powerUpgrade = 0;
        string coreString = core.ToString();
        if (coreString.Contains("Broken"))
        {
            stats.ChangeMaxPower(5);
            powerUpgrade += 5;
        }
        if (coreString.Contains("Functional"))
        {
            stats.ChangeMaxPower(10);
            powerUpgrade += 10;
        }
        if (coreString.Contains("Powerful"))
        {
            stats.ChangeMaxPower(20);
            powerUpgrade += 20;
        }
        if (coreString.Contains("Hulking"))
        {
            stats.ChangeMaxPower(30);
            powerUpgrade += 30;
        }
        return powerUpgrade;
    }
    private int HandleEtherUse(LivingBeing stats, AlchemyLoot ether, List<Elements> elementList)
    {
        int elementUpgrade = 0;
        string etherString = ether.ToString();
        if (etherString.Contains("Faint"))
            foreach (Elements element in elementList)
            {
                stats.GainAffinity(element, 10 / elementList.Count);
                elementUpgrade += 10 / elementList.Count;
            }
        if (etherString.Contains("Pure"))
            foreach (Elements element in elementList)
            {
                stats.GainAffinity(element, 30 / elementList.Count);
                elementUpgrade += 30 / elementList.Count;
            }
        if (etherString.Contains("Intense"))
            foreach (Elements element in elementList)
            {
                stats.GainAffinity(element, 60 / elementList.Count);
                elementUpgrade += 60 / elementList.Count;
            }
        return elementUpgrade;
    }



    public void UpgradeMinion(GameObject minion, Dictionary<AlchemyLoot, int> ingredients, List<Elements> elementList)
    {
        MinionStats stats = minion.GetComponent<MinionStats>();

        foreach (KeyValuePair<AlchemyLoot, int> kvp in ingredients)
        {
            if (kvp.Key.ToString().Contains("Organs")) HandleOrganUse(stats, kvp.Key);
            if (kvp.Key.ToString().Contains("Cores")) HandleCoreUse(stats, kvp.Key);
            if (kvp.Key.ToString().Contains("Ether")) HandleEtherUse(stats, kvp.Key, elementList);
        }
        stats.RestoreResources();
        stats.AlterColorByAffinity();
    }

    #endregion

    #region set Class Variable functions
    private void AddActiveMinion(GameObject minion)
    {
        if (!activeMinions.Contains(minion))
        {
            activeMinions.Add(minion);
            CommandMinion.SetSelectedMinion(minion); //Good idea?...
            newMinionAdded?.Invoke(minion);
        }
    }
    #endregion

    #region Invoke Unity Event
    private void RequestSpawnObject(GameObject minion)
    {
        requestInstantiation?.Invoke(minion);
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

                    Debug.Log($"Loaded: {address}");
                }
                else Debug.Log($"address {address} failed to Load");
            };
    }

    #endregion
}
#region garbage?

//public string CalculateCraftingResults(Dictionary<AlchemyLoot, int> combinedIngredients, List<Elements> elementList)
//{
//    if (combinedIngredients.Keys.Count > 0)
//    {
//        int healthUpgrade = 0;
//        int powerUpgrade = 0;
//        int elementUpgrade = 0;
//        string results = $"Combining these ingredients will resulted in a minion with {healthUpgrade} additional health, {powerUpgrade} additional power and {elementUpgrade} additional elemental affinity for each selected Element!";
//
//        if (minionPrefab != null)
//        {
//
//            craftedMinion = Instantiate(minionPrefab, new Vector2(transform.position.x, transform.position.y), Quaternion.identity);
//
//            MinionStats stats = craftedMinion.GetComponent<MinionStats>();
//
//            foreach (KeyValuePair<AlchemyLoot, int> kvp in combinedIngredients)
//            {
//                switch (kvp.Key)
//                {
//                    case AlchemyLoot.WretchedOrgans:
//                        stats.ChangeMaxHP(5);
//                        healthUpgrade += 5; break;
//                    case AlchemyLoot.FunctionalOrgans:
//                        stats.ChangeMaxHP(10);
//                        healthUpgrade += 10; break;
//                    case AlchemyLoot.HulkingOrgans:
//                        stats.ChangeMaxHP(20);
//                        healthUpgrade += 20; break;
//                    case AlchemyLoot.BrokenCores:
//                        stats.ChangeMaxPower(5);
//                        powerUpgrade += 5; break;
//                    case AlchemyLoot.WorkingCore:
//                        stats.ChangeMaxPower(10);
//                        powerUpgrade += 10; break;
//                    case AlchemyLoot.PowerfulCore:
//                        stats.ChangeMaxPower(20);
//                        powerUpgrade += 20; break;
//                    case AlchemyLoot.HulkingCore:
//                        stats.ChangeMaxPower(30);
//                        powerUpgrade += 30; break;
//                    case AlchemyLoot.FaintEther:
//                        foreach (Elements element in elementList)
//                        {
//                            stats.GainAffinity(element, 10 / elementList.Count);
//                            elementUpgrade += 10 / elementList.Count;
//                            Logging.Info($"Ether of type {element} used");
//
//                        }
//                        break;
//                    case AlchemyLoot.PureEther:
//                        foreach (Elements element in elementList)
//                        {
//                            stats.GainAffinity(element, 30 / elementList.Count);
//                            elementUpgrade += 30 / elementList.Count;
//                            Logging.Info($"Ether of type {element} used");
//                        }
//                        break;
//                    case AlchemyLoot.IntenseEther:
//                        foreach (Elements element in elementList)
//                        {
//                            stats.GainAffinity(element, 60 / elementList.Count);
//                            elementUpgrade += 60 / elementList.Count;
//                            Logging.Info($"Ether of type {element} used");
//
//                        }
//                        break;
//                    default:
//                        break;
//                }
//            }
//            AddActiveMinion(craftedMinion);
//
//        }
//        else Logging.Error("Crafted Minion is null, was he loaded promtly or correctly?");
//        return results;
//    }
//    else return "Select Ingredients and element to use for crafting!";
//}
#endregion