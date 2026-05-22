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

    public int maxHP;
    public int currentHP;
    public int hpRegen;
    public int powerRegen;

    public int maxPower;
    public int currentPower;
    public Dictionary<Element, int> Affinity = new();
    public Vector3 location;
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
    public Dictionary<int, Ability> SlottedAbilities = new();
    public List<Ability> abilities = new();
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
    public Dictionary<CraftingPotential, int> PlayerCraftingPotential = new();
    public Dictionary<Element, int> ElementalKnowledge = new();
    public List<AlchemyTool> KnownTools = new();
}
