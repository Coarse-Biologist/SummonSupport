using System.Collections.Generic;
using UnityEngine;
namespace Quest
{
    [CreateAssetMenu(menuName = "Quests")]
    public class Quest_SO : ScriptableObject
    {

        [SerializeField] public string QuestName;
        [SerializeField] public QuestName QuestEnum;

        [Header("Requirements")]
        [SerializeField] public List<RepeatableQuestDict> IntQuestReqs = new List<RepeatableQuestDict>();
        [SerializeField] public List<BoolAccomplishments> BoolQuestReqs = new List<BoolAccomplishments>();

        [Header("String values")]
        [SerializeField] public string PresentationString = "";
        [SerializeField] public string ProgressString = "";
        [SerializeField] public string CompletionString = "";

        [Header("Int values")]
        [SerializeField] public int GoldReward = 1;
        [SerializeField] public int XP_Reward = 1;
        [SerializeField] public int KnowledgeReward = 0;
        [SerializeField] public List<Element> BenefittedElements; // to which elements is knowledge given?

        [Header("Alchemy values")]

        [SerializeField] public int AlchemyLootNum;

        [SerializeField] public List<AlchemyLoot> AlchemyLootReward;
        [SerializeField] public bool RewardsTool;
        [SerializeField] public AlchemyTool AlchemyToolReward;


    }

    public enum QuestName
    {
        None,
        Quest1,
        Quest2,
    }
}

