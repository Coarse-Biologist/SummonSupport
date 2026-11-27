using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using SummonSupportEvents;
using SS_Structs;

/// <summary>
/// Five save slots, each holding a whole session (see SessionSaveData).
/// Each player's machine builds its own character (BuildCharacter), because it owns that player's level,
/// abilities, alchemy inventory and minions. Online, clients send theirs to the host (NetworkPlayer), and the
/// host writes the slot with everyone's newest copy (WriteSession). Loading happens when a game starts:
/// the host (or single player) loads its own character, and the host sends each joining player theirs.
/// </summary>
public static class SaveHandler
{
    public const int SlotCount = 5;
    private const string LocalPlayerIdKey = "SummonSupport.LocalPlayerId";

    private static string Folder => Path.Combine(Application.persistentDataPath, "Saves");
    private static string SlotPath(int slot) => Path.Combine(Folder, $"SummonSupport_Slot{slot}.json");

    #region Slots

    public static SessionSaveData ReadSlot(int slot)
    {
        string path = SlotPath(slot);
        if (!File.Exists(path)) return null;
        try
        {
            SessionSaveData save = JsonUtility.FromJson<SessionSaveData>(File.ReadAllText(path));
            return save != null && save.version == SessionSaveData.CurrentVersion ? save : null;
        }
        catch (Exception e)
        {
            Debug.LogWarning($"Could not read save slot {slot}: {e.Message}");
            return null;
        }
    }

    public static string DescribeSlot(int slot)
    {
        SessionSaveData save = ReadSlot(slot);
        if (save == null) return $"Slot {slot}: Empty";

        PlayerSaveRecord host = save.HostRecord;
        string character = host != null ? $"Lv {host.data.player.levelData.level} {host.playerName}" : "No host character";
        string players = save.players.Count == 1 ? "1 player" : $"{save.players.Count} players";
        return $"Slot {slot}: {character} - {save.difficulty} - {players} - {save.saveTime}";
    }

    public static void EraseSlot(int slot)
    {
        string path = SlotPath(slot);
        if (File.Exists(path)) File.Delete(path);
    }

    /// <summary>
    /// Writes the session to a slot: the host's character as it is now, and the newest copy of every other
    /// player's, including players who have left and players from the loaded save who did not join this time.
    /// </summary>
    public static bool WriteSession(int slot)
    {
        if (slot < 1 || slot > SlotCount) throw new ArgumentOutOfRangeException(nameof(slot));
        // Without the host's own character this would wipe it from the slot (e.g. the host is on the start screen).
        SaveData hostCharacter = BuildCharacter();
        if (hostCharacter == null)
        {
            Debug.LogWarning($"No local player to save, so slot {slot} was left as it was.");
            return false;
        }
        string now = DateTime.Now.ToString("yyyy-MM-dd HH:mm");

        SessionSaveData save = new()
        {
            saveTime = now,
            sceneName = SceneManager.GetActiveScene().name,
            difficulty = CurrentDifficulty()
        };
        save.players.Add(new PlayerSaveRecord
        {
            playerId = LocalPlayerId,
            playerName = PlayerStats.Instance.Name,
            isHost = true,
            saveTime = now,
            data = hostCharacter
        });
        save.players.AddRange(GameSession.GuestRecords);

        Directory.CreateDirectory(Folder);
        File.WriteAllText(SlotPath(slot), JsonUtility.ToJson(save));
        GameSession.SetActiveSlot(slot);
        Debug.Log($"Saved {save.players.Count} character(s) to slot {slot}.");
        return true;
    }

    private static Difficulty CurrentDifficulty()
    {
        if (GameSession.Difficulty.HasValue) return GameSession.Difficulty.Value;
        EnemySpawnHandler spawner = UnityEngine.Object.FindFirstObjectByType<EnemySpawnHandler>();
        return spawner != null ? spawner.CurrentDifficulty : Difficulty.Novice;
    }

    /// <summary>Who this machine's player is between sessions: the Unity Authentication player id when signed in.</summary>
    public static string LocalPlayerId
    {
        get
        {
            string servicesId = RelayConnectionManager.Instance != null ? RelayConnectionManager.Instance.LocalPlayerId : null;
            if (!string.IsNullOrEmpty(servicesId)) return servicesId;
            if (!PlayerPrefs.HasKey(LocalPlayerIdKey)) PlayerPrefs.SetString(LocalPlayerIdKey, Guid.NewGuid().ToString("N"));
            return PlayerPrefs.GetString(LocalPlayerIdKey);
        }
    }

    #endregion

    #region Building the local character

    /// <summary>The local player's character as it is now, or null when there is no local player.</summary>
    public static SaveData BuildCharacter()
    {
        if (PlayerStats.Instance == null) return null;
        SaveData saveData = new();
        SaveAlchemyData(saveData);
        SaveMinionData(saveData);
        SavePlayerData(saveData);
        return saveData;
    }

    private static void SaveAlchemyData(SaveData saveData)
    {
        foreach (var kvp in AlchemyInventory.knowledgeDict)
            saveData.alchemy.ElementalKnowledge.Add(new ElementKnowledge { elementType = kvp.Key, value = kvp.Value });

        foreach (var kvp in AlchemyInventory.AvailableCraftingPotential)
            saveData.alchemy.PlayerCraftingPotential.Add(new CraftingPotentialDict { craftingPotential = kvp.Key, value = kvp.Value });

        saveData.alchemy.KnownTools = new List<AlchemyTool>(AlchemyInventory.KnownTools);
    }

    private static void SaveMinionData(SaveData saveData)
    {
        if (AlchemyHandler.Instance == null) return;
        foreach (MinionStats minion in AlchemyHandler.Instance.activeMinions)
        {
            if (minion == null) continue;
            saveData.minions.Add(new MinionData
            {
                profile = MinionProfileData.Read(minion),
                location = minion.transform.position,
                rotation = minion.transform.rotation
            });
        }
    }

    private static void SavePlayerData(SaveData saveData)
    {
        PlayerStats stats = PlayerStats.Instance;
        LivingBeingData statData = saveData.player.statData;

        statData.Name = stats.Name;
        statData.currentHP = (int)stats.GetAttribute(AttributeType.CurrentHitpoints);
        statData.maxHP = (int)stats.GetAttribute(AttributeType.MaxHitpoints);
        statData.hpRegen = (int)stats.HealthRegeneration;
        statData.currentPower = (int)stats.GetAttribute(AttributeType.CurrentPower);
        statData.maxPower = (int)stats.GetAttribute(AttributeType.MaxPower);
        statData.powerRegen = (int)stats.PowerRegeneration;
        statData.location = stats.transform.position;
        statData.rotation = stats.transform.rotation;

        foreach (Element element in Enum.GetValues(typeof(Element)))
            statData.Affinity.Add(new ElementAffinity { elementType = element, value = stats.GetAffinity(element) });

        foreach (Ability ability in stats.abilityHandler.Abilities)
        {
            int index = AbilityRegistry.IndexOf(ability);
            if (index >= 0) saveData.player.abilityData.knownAbilities.Add(index);
        }
        foreach (SlottedAbilities slotted in stats.abilityHandler.SlottedAbilities)
        {
            int index = AbilityRegistry.IndexOf(slotted.ability);
            if (index >= 0) saveData.player.abilityData.SlottedAbilities.Add(new SavedAbilitySlot { slot = slotted.slot, ability = index });
        }

        saveData.player.levelData.level = stats.CurrentLevel;
        saveData.player.levelData.currentXp = (int)stats.CurrentXP;
        saveData.player.levelData.maxXp = (int)stats.MaxXP;
        saveData.player.levelData.skillPoints = stats.SkillPoints;
    }

    #endregion

    #region Loading a character into the local player

    /// <summary>Loads a saved character into the local player once the player and the level have started.</summary>
    public static IEnumerator ApplyCharacterWhenReady(SaveData character)
    {
        if (character == null) yield break;
        while (PlayerStats.Instance == null || AlchemyHandler.Instance == null) yield return null;
        yield return null; // let the player's own Start run first
        ApplyCharacter(character);
    }

    public static void ApplyCharacter(SaveData character)
    {
        PlayerStats stats = PlayerStats.Instance;
        if (stats == null || character == null) return;

        MovePlayer(stats, character.player.statData.location, character.player.statData.rotation);
        SetPlayerData(stats, character.player);
        SetAlchemyData(character.alchemy);
        foreach (MinionData minion in character.minions) AlchemyHandler.Instance.RestoreSavedMinion(minion);
    }

    private static void MovePlayer(PlayerStats stats, Vector3 location, Quaternion rotation)
    {
        if (location == Vector3.zero) return;
        // The owner moves its own player online, so moving it here moves it for everyone.
        if (stats.TryGetComponent(out Unity.Netcode.Components.NetworkTransform networkTransform) && networkTransform.IsSpawned)
            networkTransform.Teleport(location, rotation, stats.transform.localScale);
        else stats.transform.SetPositionAndRotation(location, rotation);
        if (stats.TryGetComponent(out Rigidbody body)) body.position = location;
    }

    private static void SetPlayerData(PlayerStats stats, PlayerData data)
    {
        LivingBeingData statData = data.statData;
        stats.SetAttribute(AttributeType.MaxHitpoints, statData.maxHP);
        stats.SetAttribute(AttributeType.MaxPower, statData.maxPower);
        stats.SetRegeneration(AttributeType.CurrentHitpoints, statData.hpRegen);
        stats.SetRegeneration(AttributeType.CurrentPower, statData.powerRegen);
        foreach (ElementAffinity affinity in statData.Affinity) stats.SetAffinity(affinity.elementType, affinity.value);

        stats.SetLevel(data.levelData.level);
        stats.SetMaxXp(data.levelData.maxXp);
        stats.SetXp(data.levelData.currentXp);
        stats.SetSkillPoints(data.levelData.skillPoints);

        foreach (int index in data.abilityData.knownAbilities)
            if (AbilityRegistry.AbilityAt(index) is Ability ability) stats.abilityHandler.LearnAbility(ability);
        foreach (SavedAbilitySlot slot in data.abilityData.SlottedAbilities)
            if (AbilityRegistry.AbilityAt(slot.ability) is Ability ability) EventDeclarer.SlotChanged?.Invoke(slot.slot, ability);

        if (NetworkSession.IsOnline)
        {
            // The host owns current hitpoints and power; have it fill them to the loaded maximums.
            if (stats.TryGetComponent(out NetworkPlayer networkPlayer)) networkPlayer.RequestRestoreResources();
            if (NetworkSession.IsAuthority) stats.RestoreResources();
        }
        else
        {
            stats.SetAttribute(AttributeType.CurrentHitpoints, Mathf.Max(1, statData.currentHP));
            stats.SetAttribute(AttributeType.CurrentPower, statData.currentPower);
        }
    }

    private static void SetAlchemyData(AlchemyData data)
    {
        foreach (ElementKnowledge knowledge in data.ElementalKnowledge)
            AlchemyInventory.SetElementalKnowledge(knowledge.elementType, knowledge.value);
        foreach (CraftingPotentialDict potential in data.PlayerCraftingPotential)
            AlchemyInventory.SetCraftingPotential(potential.craftingPotential, potential.value);
        foreach (AlchemyTool tool in data.KnownTools)
            AlchemyInventory.GainTool(tool);
    }

    #endregion
}
