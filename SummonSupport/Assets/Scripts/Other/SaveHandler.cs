using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using System;
using UnityEngine.AI;

public class SaveHandler
{
    public void SaveGameData(int slot)
    {
        SaveData saveData = new SaveData();
        saveData = SaveAlchemyData(saveData);
        saveData = SaveMinionData(saveData);
        saveData = SavePlayerData(saveData);
        string data = JsonUtility.ToJson(saveData);
        //save this somewhere
    }
    public void LoadGameData(int slot)
    {
        string gameData = "info from some slot";
        SaveData saveData = JsonUtility.FromJson<SaveData>(gameData);
        //Set all player, minion and alchemy data as it is read from the save data
    }

    private SaveData SaveAlchemyData(SaveData saveData)
    {
        saveData.alchemy.ElementalKnowledge = new Dictionary<Element, int>(AlchemyInventory.knowledgeDict);
        saveData.alchemy.KnownTools = new List<AlchemyTool>(AlchemyInventory.KnownTools);
        saveData.alchemy.PlayerCraftingPotential = new Dictionary<CraftingPotential, int>(AlchemyInventory.AvailableCraftingPotential);
        return saveData;
    }


    private SaveData SaveMinionData(SaveData saveData)
    {
        saveData.minions.Clear();

        foreach (MinionStats minion in AlchemyHandler.Instance.activeMinions)
        {
            saveData = SaveNewMinionData(saveData, minion);
        }

        return saveData;
    }


    private SaveData SavePlayerData(SaveData saveData)
    {

        Dictionary<Element, int> affinitiesData = new();
        foreach (Element element in Enum.GetValues(typeof(Element)))
        {
            affinitiesData.Add(element, PlayerStats.Instance.GetAffinity(element));
        }

        saveData.player.statData.Affinity = affinitiesData;
        saveData.player.statData.currentHP = (int)PlayerStats.Instance.GetAttribute(AttributeType.CurrentHitpoints);
        saveData.player.statData.maxHP = (int)PlayerStats.Instance.GetAttribute(AttributeType.MaxHitpoints);
        saveData.player.statData.hpRegen = (int)PlayerStats.Instance.HealthRegeneration;
        saveData.player.statData.location = PlayerStats.Instance.transform.position;
        saveData.player.statData.powerRegen = (int)PlayerStats.Instance.PowerRegeneration;
        saveData.player.statData.currentPower = (int)PlayerStats.Instance.GetAttribute(AttributeType.CurrentPower);
        saveData.player.statData.currentPower = (int)PlayerStats.Instance.GetAttribute(AttributeType.MaxPower);
        return saveData;
    }

    private SaveData SaveNewMinionData(SaveData saveData, MinionStats minionStats)
    {
        MinionData minionData = new();
        saveData.minions.Add(minionData);
        Dictionary<Element, int> affinitiesData = new();
        foreach (Element element in Enum.GetValues(typeof(Element)))
        {
            affinitiesData.Add(element, PlayerStats.Instance.GetAffinity(element));
        }

        minionData.statData.Affinity = affinitiesData;
        minionData.statData.currentHP = (int)minionStats.GetAttribute(AttributeType.CurrentHitpoints);
        minionData.statData.maxHP = (int)minionStats.GetAttribute(AttributeType.MaxHitpoints);
        minionData.statData.hpRegen = (int)minionStats.HealthRegeneration;
        minionData.statData.location = minionStats.transform.position;

        minionData.abilityData = SaveAbilities(minionData.abilityData, minionStats.abilityHandler);

        return saveData;
    }

    private AbilityData SaveAbilities(AbilityData abilityData, AbilityHandler abilityHandler)
    {
        abilityData.abilities = new List<Ability>(abilityData.abilities);
        return abilityData;
    }
}
