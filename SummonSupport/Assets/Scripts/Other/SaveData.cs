using System.Collections.Generic;
using UnityEngine;

public class SaveData
{
    public PlayerData player;
    public List<MinionData> minions;
    public AlchemyData alchemy;

}

public class LivingBeingData
{

    public int maxHP;
    public int currentHP;
    public int hpRegen;
    public int powerRegen;

    public int maxPower;
    public int currentPower;
    public Dictionary<Element, int> Affinity;
    public Vector3 location;
}

public class PlayerLevelData
{
    public int xp;
    public int level;
    public int skillPoints;
}

public class AbilityData
{
    public Dictionary<int, Ability> SlottedAbilities;
    public List<Ability> abilities;
}

public class MinionData
{
    public LivingBeingData statData;
    public AbilityData abilityData;
}

public class PlayerData
{
    public PlayerLevelData levelData;
    public LivingBeingData statData;
    public AbilityData abilityData;
}
public class AlchemyData
{
    public Dictionary<CraftingPotential, int> PlayerCraftingPotential;
    public Dictionary<Element, int> ElementalKnowledge;
    public List<AlchemyTool> KnownTools;
}
