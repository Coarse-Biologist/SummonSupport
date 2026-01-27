using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.Events;
using Quest;
using SummonSupportEvents;
using UnityEditor.Build.Pipeline;

public class QuestHandler : MonoBehaviour
{
    [field: SerializeField] public Quest_SO sceneStartingQuest;
    public static QuestHandler Instance;
    public List<Quest_SO> CompletedQuests = new List<Quest_SO>();
    public List<BoolAccomplishments> CompletedBoolQuests;
    public List<Quest_SO> ActiveQuests = new List<Quest_SO>();
    public Dictionary<RepeatableAccomplishments, int> QuestRepTracker = new Dictionary<RepeatableAccomplishments, int>()
    {
        {RepeatableAccomplishments.EnemiesDefeated, 0},
        {RepeatableAccomplishments.KnowledgeGained, 0},
        {RepeatableAccomplishments.MinionsCrafted, 0},
        {RepeatableAccomplishments.OrgansUsed, 0},
        {RepeatableAccomplishments.CoresUsed, 0},
        {RepeatableAccomplishments.EtherUsed, 0},
    };
    public Dictionary<RepeatableAccomplishments, int> TotalRepTracker = new Dictionary<RepeatableAccomplishments, int>()
    {
        {RepeatableAccomplishments.EnemiesDefeated, 0},
        {RepeatableAccomplishments.KnowledgeGained, 0},
        {RepeatableAccomplishments.MinionsCrafted, 0},
        {RepeatableAccomplishments.OrgansUsed, 0},
        {RepeatableAccomplishments.CoresUsed, 0},
        {RepeatableAccomplishments.EtherUsed, 0},
    };
    private PlayerStats playerStats;

    void Awake()
    {
        Instance = this;
    }
    void Start()
    {
        playerStats = PlayerStats.Instance;
        EventDeclarer.QuestStarted?.Invoke(sceneStartingQuest);
    }

    void OnEnable()
    {
        EventDeclarer.RepeatableQuestCompleted.AddListener(IncrementIntQuest);
        EventDeclarer.EnemyDefeated?.AddListener(IncrementEnemyDefeated);
        EventDeclarer.QuestStarted?.AddListener(AddActiveQuest);
        EventDeclarer.QuestCompleted?.AddListener(HandleQuestCompleted);


    }
    void OnDisable()
    {
        EventDeclarer.RepeatableQuestCompleted?.RemoveListener(IncrementIntQuest);
        EventDeclarer.EnemyDefeated?.RemoveListener(IncrementEnemyDefeated);
        EventDeclarer.QuestStarted?.RemoveListener(AddActiveQuest);
        EventDeclarer.QuestCompleted?.RemoveListener(HandleQuestCompleted);
    }


    public bool CheckQuestCompletion(Quest_SO activeQuest)
    {
        bool complete = true;
        foreach (RepeatableQuestDict intQuest in activeQuest.IntQuestReqs)
        {
            if (QuestRepTracker.TryGetValue(intQuest.quest, out int reps) && reps >= intQuest.reps)
            {
                continue;
            }
            else
            {
                return false;
            }
        }
        foreach (BoolAccomplishments boolQuest in activeQuest.BoolQuestReqs)
        {
            if (CompletedBoolQuests.Contains(boolQuest))
            {
                continue;
            }
            else
            {
                return false;
            }
        }
        return complete;
    }
    public void AddActiveQuest(Quest_SO quest)
    {
        if (quest == null) throw new System.Exception("QuestHandler: AddActiveQuest called with null quest");
        //debuf.Log($"Active quest added {quest.QuestName}");
        if (!ActiveQuests.Contains(quest)) ActiveQuests.Add(quest);
        PlayerUIHandler.Instance.ShowQuestInfo(quest);
    }

    public void HandleQuestCompleted(Quest_SO quest)
    {
        if (ActiveQuests.Contains(quest)) ActiveQuests.Remove(quest);
        if (!CompletedQuests.Contains(quest)) CompletedQuests.Add(quest);
        PlayerUIHandler.Instance.ShowCompletedQuestInfo(quest);
        //EventDeclarer.QuestCompleted?.Invoke(quest);
        GrantCompletionRewards(quest);
    }
    public void GrantCompletionRewards(Quest_SO quest)
    {
        Debug.Log($"Granting rewards for quest: {quest.QuestName}");
        for (int i = 0; i < quest.BenefittedElements.Count; i++)
        {
            AlchemyInventory.IncemementElementalKnowledge(quest.BenefittedElements[i], quest.KnowledgeReward);
        }
        AlchemyInventory.GainTool(quest.AlchemyToolReward);
        foreach (AlchemyLoot ingredient in quest.AlchemyLootReward)
        {
            AlchemyInventory.AlterIngredientNum(ingredient, quest.AlchemyLootNum);
        }
        FloatingInfoHandler.Instance.DisplayXPGain(quest.XP_Reward);
        playerStats.GainXP(quest.XP_Reward);
    }
    public void IncrementIntQuest(RepeatableAccomplishments intQuest, int value = 1)
    {
        Debug.Log($"Increment func called for {intQuest}. change value  = {value}");


        if (QuestRepTracker.TryGetValue(intQuest, out int reps))
        {

            QuestRepTracker[intQuest] += value;

            // Debug.Log($"quest: {intQuest} increased by {value} Current total num = {reps + value}");
        }
        else
        {
            QuestRepTracker.Add(intQuest, value);
            // Debug.Log($"quest: {intQuest} increased by {value}. Current total num = {value}");
        }
        CheckQuestCompletionsForActiveQuests(intQuest);
    }
    private void CheckQuestCompletionsForActiveQuests(RepeatableAccomplishments updatedQuest)
    {
        foreach (Quest_SO activeQuest in ActiveQuests.ToList())
        {
            PlayerUIHandler.Instance.ShowQuestInfo(activeQuest);

            bool complete = CheckQuestCompletion(activeQuest);
            if (complete)
            {
                EventDeclarer.QuestCompleted?.Invoke(activeQuest);
            }
        }
    }
    public void IncrementEnemyDefeated(LivingBeing livingBeing)
    {
        QuestRepTracker[RepeatableAccomplishments.EnemiesDefeated]++;
        TotalRepTracker[RepeatableAccomplishments.EnemiesDefeated]++;
        CheckQuestCompletionsForActiveQuests(RepeatableAccomplishments.EnemiesDefeated);

    }
    public string GetQuestCompletionStats()
    {
        string stats = "";
        foreach (KeyValuePair<RepeatableAccomplishments, int> kvp in QuestRepTracker)
        {
            if (kvp.Value != 99)
            {
                stats += $"{GeneralFunctions.GetCleanEnumString(kvp.Key)}: {kvp.Value}\n";
            }
        }
        return stats;
    }
    public string GetQuestInfo(Quest_SO quest)
    {
        string info = $"{quest.QuestName}:\n";
        foreach (var req in quest.IntQuestReqs)
        {
            info += $"{GeneralFunctions.GetCleanEnumString<RepeatableAccomplishments>(req.quest)}: {QuestRepTracker[req.quest]}/{req.reps}\n";
        }
        foreach (var req in quest.BoolQuestReqs)
        {
            info += $"{GeneralFunctions.GetCleanEnumString<BoolAccomplishments>(req)}: {CompletedBoolQuests.Contains(req)}\n";
        }
        return info;
    }


}

