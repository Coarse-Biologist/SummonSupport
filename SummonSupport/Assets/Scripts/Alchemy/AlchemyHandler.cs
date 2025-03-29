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
    public List<GameObject> activeMinions = new List<GameObject>();

    #endregion

    #region Crafting Minion
    public string CalculateCraftingResults(Dictionary<AlchemyLoot, int> combinedIngredients, List<Elements> elementList)
    {
        craftedMinion = Instantiate(minionPrefab, new Vector2(transform.position.x, transform.position.y), Quaternion.identity);
        int healthUpgrade = 0;
        int powerUpgrade = 0;
        int elementUpgrade = 0;
        string results = $"Combining these ingredients will resulted in a minion with {healthUpgrade} additional health, {powerUpgrade} additional power and {elementUpgrade} additional elemental affinity for each selected Element!";

        if (minionPrefab != null)
        {
            MinionStats stats = craftedMinion.GetComponent<MinionStats>();

            foreach (KeyValuePair<AlchemyLoot, int> kvp in combinedIngredients)
            {
                switch (kvp.Key)
                {
                    case AlchemyLoot.WretchedOrgans:
                        stats.ChangeMaxHP(5);
                        healthUpgrade += 5; break;
                    case AlchemyLoot.FunctionalOrgans:
                        stats.ChangeMaxHP(10);
                        healthUpgrade += 10; break;
                    case AlchemyLoot.HulkingOrgans:
                        stats.ChangeMaxHP(20);
                        healthUpgrade += 20; break;
                    case AlchemyLoot.BrokenCores:
                        stats.ChangeMaxPower(5);
                        powerUpgrade += 5; break;
                    case AlchemyLoot.WorkingCore:
                        stats.ChangeMaxPower(10);
                        powerUpgrade += 10; break;
                    case AlchemyLoot.PowerfulCore:
                        stats.ChangeMaxPower(20);
                        powerUpgrade += 20; break;
                    case AlchemyLoot.HulkingCore:
                        stats.ChangeMaxPower(30);
                        powerUpgrade += 30; break;
                    case AlchemyLoot.FaintEther:
                        foreach (Elements element in elementList)
                        {
                            stats.GainAffinity(element, 10 / elementList.Count);
                            elementUpgrade += 10 / elementList.Count;
                        }
                        break;
                    case AlchemyLoot.PureEther:
                        foreach (Elements element in elementList)
                        {
                            stats.GainAffinity(element, 30 / elementList.Count);
                            elementUpgrade += 30 / elementList.Count;
                        }
                        break;
                    case AlchemyLoot.IntenseEther:
                        foreach (Elements element in elementList)
                        {
                            stats.GainAffinity(element, 60 / elementList.Count);
                            elementUpgrade += 60 / elementList.Count;
                        }
                        break;
                    default:
                        break;
                }
            }
            AddActiveMinion(craftedMinion);
        }
        else Logging.Error("Crafted Minion is null, was he loaded promtly or correctly?");
        return results;
    }


    #endregion

    #region set Class Variable functions
    private void AddActiveMinion(GameObject minion)
    {
        if (!activeMinions.Contains(minion)) activeMinions.Add(minion);
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