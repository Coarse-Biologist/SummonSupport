using System.Collections.Generic;
using UnityEngine;
using System;

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

[Serializable]
public class AbilityData
{
    public List<SS_Structs.SlottedAbilities> SlottedAbilities = new();
    public List<Ability> knownAbilities = new();
}

[Serializable]
public class MinionData
{
    public LivingBeingData statData = new();
    public AbilityData abilityData = new();
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
