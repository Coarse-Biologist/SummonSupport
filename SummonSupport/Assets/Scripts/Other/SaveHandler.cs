using UnityEngine;
using System.Collections.Generic;
using System;
using System.IO;
using SummonSupportEvents;
using UnityEngine.InputSystem;
using SS_Structs;
using Unity.Entities.UniversalDelegates;
using Unity.VisualScripting;
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

        Debug.Log(JsonUtility.ToJson(saveData, true));
    }

    private static SaveData SaveAlchemyData(SaveData saveData)
    {
        foreach (var kvp in AlchemyInventory.knowledgeDict)
        {
            SS_Structs.ElementKnowledge knowledgeStruct = new()
            {
                elementType = kvp.Key,
                value = kvp.Value
            };
            saveData.alchemy.ElementalKnowledge.Add(knowledgeStruct);
        }

        foreach (var kvp in AlchemyInventory.AvailableCraftingPotential)
        {
            SS_Structs.CraftingPotentialDict craftingPotentialStruct = new()
            {
                craftingPotential = kvp.Key,
                value = kvp.Value
            };
            saveData.alchemy.PlayerCraftingPotential.Add(craftingPotentialStruct);

        }
        saveData.alchemy.KnownTools = new List<AlchemyTool>(AlchemyInventory.KnownTools);


        return saveData;
    }

    private static SaveData SaveMinionData(SaveData saveData)
    {
        saveData.minions.Clear();

        foreach (MinionStats minion in AlchemyHandler.Instance.activeMinions)
        {
            Debug.Log($"savedata = {saveData}");

            SaveNewMinionData(saveData, minion);
        }
        Debug.Log($"savedata = {saveData}");

        return saveData;
    }

    private static SaveData SavePlayerData(SaveData saveData)
    {

        SavePlayerAffinity(saveData);

        saveData.player.statData.currentHP = (int)PlayerStats.Instance.GetAttribute(AttributeType.CurrentHitpoints);
        saveData.player.statData.maxHP = (int)PlayerStats.Instance.GetAttribute(AttributeType.MaxHitpoints);
        saveData.player.statData.hpRegen = (int)PlayerStats.Instance.HealthRegeneration;
        saveData.player.statData.location = PlayerStats.Instance.transform.position;
        saveData.player.statData.powerRegen = (int)PlayerStats.Instance.PowerRegeneration;
        saveData.player.statData.currentPower = (int)PlayerStats.Instance.GetAttribute(AttributeType.CurrentPower);
        saveData.player.statData.maxPower = (int)PlayerStats.Instance.GetAttribute(AttributeType.MaxPower);
        SaveAbilities(saveData.player.abilityData, PlayerStats.Instance.abilityHandler);
        SaveSlottedAbilities(saveData.player.abilityData);
        SavePlayerLevelData(saveData);

        return saveData;
    }

    private static void SavePlayerAffinity(SaveData saveData)
    {
        saveData.player.statData.Affinity.Clear();
        foreach (Element element in Enum.GetValues(typeof(Element)))
        {
            SS_Structs.ElementAffinity affinityStruct = new();
            affinityStruct.elementType = element;
            affinityStruct.value = PlayerStats.Instance.GetAffinity(element);
            saveData.player.statData.Affinity.Add(affinityStruct);
        }
    }

    private static List<ElementAffinity> SaveMinionAffinity(SaveData saveData, MinionStats minionStats)
    {
        List<ElementAffinity> affinityData = new();
        foreach (Element element in Enum.GetValues(typeof(Element)))
        {
            ElementAffinity data = new()
            {
                elementType = element,
                value = PlayerStats.Instance.GetAffinity(element)
            };
            affinityData.Add(data);
        }
        return affinityData;
    }

    private static void SaveNewMinionData(SaveData saveData, MinionStats minionStats)
    {
        MinionData minionData = new();
        saveData.minions.Add(minionData);

        List<ElementAffinity> affinityData = SaveMinionAffinity(saveData, minionStats);

        minionData.statData.Affinity = affinityData;

        minionData.statData.currentHP = (int)minionStats.GetAttribute(AttributeType.CurrentHitpoints);
        minionData.statData.maxHP = (int)minionStats.GetAttribute(AttributeType.MaxHitpoints);
        minionData.statData.hpRegen = (int)minionStats.HealthRegeneration;
        minionData.statData.location = minionStats.transform.position;

        minionData.abilityData = SaveAbilities(minionData.abilityData, minionStats.abilityHandler);
        saveData.minions.Add(minionData);
    }

    private static AbilityData SaveAbilities(AbilityData abilityData, AbilityHandler abilityHandler)
    {
        abilityData.knownAbilities = new List<Ability>(abilityHandler.Abilities);
        return abilityData;
    }
    private static AbilityData SaveSlottedAbilities(AbilityData abilityData)
    {
        foreach (var kvp in PlayerStats.Instance.abilityHandler.SlottedAbilities)
        {
            SS_Structs.SlottedAbilities slottedAbilitiesStruct = new()
            {
                ability = kvp.Value,
                slot = kvp.Key
            };
            abilityData.SlottedAbilities.Add(slottedAbilitiesStruct);
        }
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

    #region load
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
        player.transform.position = loadedData.player.statData.location;
        if (player.TryGetComponent(out LivingBeing livingBeing))
        {
            SetPlayerData(loadedData);
        }
        SetMinionData(loadedData);
        SetAlchemyData(loadedData.alchemy);
    }
    private static void SetAlchemyData(AlchemyData data)
    {
        foreach (var dataStruct in data.ElementalKnowledge)
        {
            AlchemyInventory.SetElementalKnowledge(dataStruct.elementType, dataStruct.value);
        }
        foreach (var dataStruct in data.PlayerCraftingPotential)
        {
            AlchemyInventory.SetCraftingPotential(dataStruct.craftingPotential, dataStruct.value);
        }
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
        for (int i = 0; i < AlchemyHandler.Instance.activeMinions.Count; i++)
        {
            MinionStats minionToUnload = (MinionStats)AlchemyHandler.Instance.activeMinions[i];
            minionToUnload.TrueDeath();
        }
        foreach (MinionData minionData in loadedData.minions)
        {
            GameObject minion = AlchemyHandler.Instance.SpawnMinion(minionData.statData.location);
            SetupManager.Instance.DebugLocation(minion.transform.position + Vector3.left, Color.orange);

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
        foreach (var kvp in lb_Data.Affinity)
        {
            being.SetAffinity(kvp.elementType, kvp.value);
        }
    }
    private static void SetBeingLoadedAbilities(AbilityData livingBeingAbilityData, LivingBeing lb)
    {
        foreach (Ability ability in livingBeingAbilityData.knownAbilities)
        {
            if (!lb.TryGetComponent(out AbilityHandler aHandler))
            {
                throw new Exception("You wont see this message because every minion has it");
            }
            else lb.SetAbilityHandler(aHandler);
            SetupManager.Instance.DebugLocation(lb.transform.position, Color.red);
            lb.abilityHandler.LearnAbility(ability); //#TODO null reference
        }
    }

    private static void SetPlayerAbilitySlots(SaveData saveData)
    {
        foreach (var kvp in saveData.player.abilityData.SlottedAbilities)
        {
            EventDeclarer.SlotChanged?.Invoke(kvp.slot, kvp.ability);
        }
    }


    #endregion
}
