using System.Collections.Generic;
using UnityEngine;
using System;

/// <summary>
/// One save slot: a whole session, so it can hold every player who took part.
/// Written by the host (or the single player) to Saves/SummonSupport_Slot{n}.json.
/// </summary>
[Serializable]
public class SessionSaveData
{
    public const int CurrentVersion = 2;

    public int version = CurrentVersion;
    public string saveTime = "";
    public string sceneName = "";
    public Difficulty difficulty = Difficulty.Novice;
    public List<PlayerSaveRecord> players = new();

    public PlayerSaveRecord HostRecord => players.Find(p => p.isHost);
    public PlayerSaveRecord FindPlayer(string playerId) => players.Find(p => !p.isHost && p.playerId == playerId);
}

/// <summary>One player's character in a session save. The host's character is the one marked isHost.</summary>
[Serializable]
public class PlayerSaveRecord
{
    public string playerId = "";
    public string playerName = "";
    public bool isHost;
    public string saveTime = "";
    public SaveData data = new();
}

/// <summary>
/// One player's character: built on that player's own machine, which owns its level, abilities,
/// alchemy inventory and minions (see SaveHandler.BuildCharacter).
/// </summary>
[Serializable]
public class SaveData
{
    public PlayerData player = new();
    public List<MinionData> minions = new();
    public AlchemyData alchemy = new();
}

[Serializable]
public class LivingBeingData
{
    public string Name;
    public int maxHP;
    public int currentHP;
    public int hpRegen;
    public int powerRegen;

    public int maxPower;
    public int currentPower;
    public List<SS_Structs.ElementAffinity> Affinity = new();
    public Vector3 location;
    public Quaternion rotation;
}

[Serializable]
public class PlayerLevelData
{
    public int currentXp;
    public int maxXp;
    public int level;
    public int skillPoints;
}

/// <summary>Abilities are stored as ability registry indices (SetupManager.AllAbilities), which stay valid between runs.</summary>
[Serializable]
public class AbilityData
{
    public List<SavedAbilitySlot> SlottedAbilities = new();
    public List<int> knownAbilities = new();
}

[Serializable]
public struct SavedAbilitySlot
{
    public int slot;
    public int ability;
}

/// <summary>A minion is saved as the same profile its crafter sends to the host online, plus where it was.</summary>
[Serializable]
public class MinionData
{
    public MinionProfileData profile;
    public Vector3 location;
    public Quaternion rotation;
}

[Serializable]
public class PlayerData
{
    public PlayerLevelData levelData = new();
    public LivingBeingData statData = new();
    public AbilityData abilityData = new();
}

[Serializable]
public class AlchemyData
{
    public List<SS_Structs.CraftingPotentialDict> PlayerCraftingPotential = new();
    public List<SS_Structs.ElementKnowledge> ElementalKnowledge = new();
    public List<AlchemyTool> KnownTools = new();
}
