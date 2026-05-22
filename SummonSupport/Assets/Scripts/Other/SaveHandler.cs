using UnityEngine;
using System.Collections.Generic;
using System;
using System.IO;
using SummonSupportEvents;
public static class SaveHandler
{

    public static void SaveGameData(int slot)
    {
        SaveData saveData = new SaveData();

        saveData = SaveAlchemyData(saveData);
        saveData = SaveMinionData(saveData);
        saveData = SavePlayerData(saveData);

        string data = JsonUtility.ToJson(saveData, true);

        string folder = Path.Combine(
            Application.persistentDataPath,
            "Saves"
        );

        Directory.CreateDirectory(folder);

        string path = Path.Combine(
            folder,
            $"SummonSupport_SaveSlot{slot}.json"
        );

        File.WriteAllText(path, data);

        Debug.Log("Saved game to: " + path);
    }

    private static SaveData SaveAlchemyData(SaveData saveData)
    {
        saveData.alchemy.ElementalKnowledge = new Dictionary<Element, int>(AlchemyInventory.knowledgeDict);
        saveData.alchemy.KnownTools = new List<AlchemyTool>(AlchemyInventory.KnownTools);
        saveData.alchemy.PlayerCraftingPotential = new Dictionary<CraftingPotential, int>(AlchemyInventory.AvailableCraftingPotential);
        return saveData;
    }

    private static SaveData SaveMinionData(SaveData saveData)
    {
        saveData.minions.Clear();

        foreach (MinionStats minion in AlchemyHandler.Instance.activeMinions)
        {
            saveData = SaveNewMinionData(saveData, minion);
        }

        return saveData;
    }

    private static SaveData SavePlayerData(SaveData saveData)
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

        SavePlayerLevelData(saveData);

        return saveData;
    }

    private static SaveData SaveNewMinionData(SaveData saveData, MinionStats minionStats)
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

    private static AbilityData SaveAbilities(AbilityData abilityData, AbilityHandler abilityHandler)
    {
        abilityData.abilities = new List<Ability>(abilityData.abilities);
        return abilityData;
    }

    private static SaveData SavePlayerLevelData(SaveData saveData)
    {
        saveData.player.levelData.level = PlayerStats.Instance.CurrentLevel;
        saveData.player.levelData.currentXp = (int)PlayerStats.Instance.CurrentXP;
        saveData.player.levelData.maxXp = (int)PlayerStats.Instance.MaxXP;

        saveData.player.levelData.skillPoints = PlayerStats.Instance.SkillPoints;

        return saveData;
    }

    #region Handle loading data
    public static void LoadGameData(int slot)
    {

        string path = Path.Combine(
            Application.persistentDataPath,
            "Saves",
            $"SummonSupport_SaveSlot{slot}.json"
        );

        if (!File.Exists(path))
        {
            throw new Exception($"The save file path did not exist: {path}");
        }

        string data = File.ReadAllText(path);

        SaveData saveData = JsonUtility.FromJson<SaveData>(data);

        HandleLoadedData(saveData);
    }

    private static void HandleLoadedData(SaveData loadedData)
    {
        //#TODO
        GameObject player = SetupManager.Instance.SpawnPlayer(loadedData.player.statData.location);
        if (player.TryGetComponent(out LivingBeing livingBeing))
        {
            SetPlayerData(loadedData);
        }
        SetMinionData(loadedData);
    }

    private static void SetPlayerData(SaveData loadedData)
    {
        if (PlayerStats.Instance != null)
        {
            PlayerStats.Instance.SetAttribute(AttributeType.CurrentHitpoints, loadedData.player.statData.currentHP);
            PlayerStats.Instance.SetAttribute(AttributeType.MaxHitpoints, loadedData.player.statData.maxHP);
            PlayerStats.Instance.SetAttribute(AttributeType.CurrentPower, loadedData.player.statData.currentPower);
            PlayerStats.Instance.SetAttribute(AttributeType.MaxPower, loadedData.player.statData.maxPower);

            PlayerStats.Instance.SetRegeneration(AttributeType.CurrentHitpoints, loadedData.player.statData.hpRegen);
            PlayerStats.Instance.SetRegeneration(AttributeType.CurrentPower, loadedData.player.statData.powerRegen);
            PlayerStats.Instance.SetLevel(loadedData.player.levelData.level);
            PlayerStats.Instance.SetXp(loadedData.player.levelData.currentXp);
            PlayerStats.Instance.SetMaxXp(loadedData.player.levelData.maxXp);
            SetBeingLoadedAbilities(loadedData.player.abilityData, PlayerStats.Instance);
            SetBeingAffinities(loadedData.player.statData, PlayerStats.Instance);
            SetPlayerAbilitySlots(loadedData);


        }
        else throw new Exception("The Player does not exist to be modified.");
    }
    private static void SetMinionData(SaveData loadedData)
    {
        foreach (MinionData minionData in loadedData.minions)
        {
            GameObject minion = AlchemyHandler.Instance.SpawnMinion(minionData.statData.location);
            if (minion.TryGetComponent(out MinionStats minionStats))
            {
                SetMinionStats(minionData, minionStats);
                SetBeingLoadedAbilities(minionData.abilityData, minionStats);
                SetBeingAffinities(minionData.statData, minionStats);
            }
        }
    }
    private static void SetMinionStats(MinionData minionData, MinionStats minionStats)
    {
        minionStats.SetAttribute(AttributeType.CurrentHitpoints, minionData.statData.currentHP);
        minionStats.SetAttribute(AttributeType.MaxHitpoints, minionData.statData.maxHP);

    }
    private static void SetBeingAffinities(LivingBeingData lb_Data, LivingBeing being)
    {
        being.SetRegeneration(AttributeType.CurrentHitpoints, lb_Data.hpRegen);
        foreach (var kvp in lb_Data.Affinity)
        {
            being.SetAffinity(kvp.Key, kvp.Value);
        }
    }
    private static void SetBeingLoadedAbilities(AbilityData livingBeingAbilityData, LivingBeing lb)
    {
        foreach (Ability ability in livingBeingAbilityData.abilities)
        {
            lb.abilityHandler.LearnAbility(ability);
        }
    }

    private static void SetPlayerAbilitySlots(SaveData saveData)
    {
        foreach (var kvp in saveData.player.abilityData.SlottedAbilities)
        {
            EventDeclarer.SlotChanged?.Invoke(kvp.Key, kvp.Value);
        }
    }


    #endregion
}
