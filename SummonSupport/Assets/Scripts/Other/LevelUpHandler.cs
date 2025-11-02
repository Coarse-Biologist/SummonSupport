using System.Collections.Generic;
using UnityEngine;

public static class LevelUpHandler
{
    public static Dictionary<int, List<LevelRewards>> LevelRewardsDict = new()
    {
        {2, new (){LevelRewards.SkillPoint}},
        {3, new(){LevelRewards.SkillPoint, LevelRewards.MaximumHealth}},
        {4, new(){LevelRewards.SkillPoint, LevelRewards.MaximumPower}},
        {5, new(){LevelRewards.SkillPoint, LevelRewards.TotalControlllableMinions, LevelRewards.HealthRegeneration}},
        {6, new(){LevelRewards.SkillPoint, LevelRewards.PowerRegeneration}},
        {7, new(){LevelRewards.SkillPoint, LevelRewards.AbilitySlot}},

    };

    public static Dictionary<LevelRewards, string> RewardsDescriptionDict = new()
    {
    {LevelRewards.SkillPoint, "+1 Skill-point!"},
    {LevelRewards.MaximumHealth, "+1% Maximum health!"},
    {LevelRewards.MaximumPower, "+ 1% Maximum power!"},
    {LevelRewards.HealthRegeneration, "+ 1 HP per second!"},
    {LevelRewards.PowerRegeneration, "+ 1 Power per second!"},
    {LevelRewards.ElementalAffinity, "+ 10 Elemental affinity"},
    {LevelRewards.AbilitySlot, "+ 1 Ability slot"},
    {LevelRewards.TotalControlllableMinions, "+ 1 Total controllable minions"},
    };


    public static List<LevelRewards> GetLevelRewards(int level)
    {
        if (!LevelRewardsDict.TryGetValue(level, out List<LevelRewards> rewards))
        {
            Debug.LogWarning($"There are no rewards for level {level}, yet you are trying to access them in the LevelUpHandler");
            return null;
        }

        else return rewards;
    }

    public static List<string> GetLevelRewardString(int level)
    {
        List<string> stringDescriptions = new();
        List<LevelRewards> rewards = GetLevelRewards(level);
        if (rewards != null)
        {
            foreach (LevelRewards reward in rewards)
            {
                if (RewardsDescriptionDict.TryGetValue(reward, out string description))
                    stringDescriptions.Add(description);
                else Debug.LogWarning($"The reward {reward} was not found in the Reward description dict in the Level up handler.");
            }
            return stringDescriptions;
        }
        else
        {
            Debug.LogWarning($"The rewards list was null when returned by the GetlevelRewards function in the Level up handler.");
            return stringDescriptions;
        }
    }
}

