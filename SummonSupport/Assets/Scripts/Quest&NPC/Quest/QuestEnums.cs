using UnityEngine;
namespace Quest
{
    public enum BoolAccomplishments // quests which can only be done once and are therfore either complete (true) or incomplete (false)
    {
        CompleteLevel1
    }

    public enum RepeatableAccomplishments // quests which can and may be required to be completed X times.
    {
        DefeatEnemies,
        CraftMinions,
        LootOrgans,
        LootCores,
        LootEther,
        UseOrgans,
        UseCores,
        UseEther,
        GainKnowledge,

    }
}