using System.Collections.Generic;
using Alchemy;
using UnityEngine;

[CreateAssetMenu(menuName = "Quests")]
public class Quest_SO : ScriptableObject
{

    [SerializeField] public string QuestName;
    [SerializeField] public QuestName QuestEnum;

    [Header("Requirements")]
    [SerializeField] public List<RepeatableQuestDict> IntQuestReqs;
    [SerializeField] public List<BoolAccomplishments> BoolQuestReqs;
    [SerializeField] public Vector2 TargetLocation;

    [Header("String values")]
    [SerializeField] public string PresentationString = "";
    [SerializeField] public string ProgressString = "";
    [SerializeField] public string CompletionString = "";

    [Header("Int values")]
    [SerializeField] public int GoldReward = 1;
    [SerializeField] public int XP_Reward = 1;
    [SerializeField] public int KnowledgeReward = 0;
    [SerializeField] public List<Elements> BenefittedElements; // in which elements is knowledge given?

    [Header("Alchemy values")]
    [SerializeField] public AlchemyLoot AlchemyLootReward;
    [SerializeField] public AlchemyTools AlchemyToolReward;

}
public enum BoolAccomplishments // quests which can only be done once and are therfore either complete (true) or incomplete (false)
{
    CompleteLevel1
}

public enum RepeatableAccomplishments // quests which can and may be required to be completed X times.
{
    DefeatEnemies,
    CraftMinions,
}
public enum QuestName
{
    None,
    Quest1,
    Quest2,
}

