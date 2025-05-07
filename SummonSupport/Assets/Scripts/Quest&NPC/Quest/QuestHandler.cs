using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.Events;
using Quest;
using SummonSupportEvents;

public class QuestHandler : MonoBehaviour
{
    public static QuestHandler Instance;
    public List<Quest_SO> CompletedQuests = new List<Quest_SO>();
    public List<BoolAccomplishments> CompletedBoolQuests;
    public List<Quest_SO> ActiveQuests = new List<Quest_SO>();
    public Dictionary<RepeatableAccomplishments, int> QuestRepTracker = new Dictionary<RepeatableAccomplishments, int>();
    private PlayerStats playerStats;

    void Awake()
    {
        Instance = this;
        QuestRepTracker.Add(RepeatableAccomplishments.DefeatEnemies, 0);
    }
    void Start()
    {
        playerStats = PlayerStats.Instance;
    }

    void OnEnable()
    {
        EventDeclarer.RepeatableQuestCompleted.AddListener(IncrementIntQuest);
        EventDeclarer.EnemyDefeated.AddListener(IncrementEnemyDefeated);
        EventDeclarer.QuestStarted.AddListener(AddActiveQuest);
        EventDeclarer.QuestCompleted.AddListener(HandleQuestCompleted);


    }
    void OnDisable()
    {
        EventDeclarer.RepeatableQuestCompleted.RemoveListener(IncrementIntQuest);
        EventDeclarer.EnemyDefeated.RemoveListener(IncrementEnemyDefeated);
        EventDeclarer.QuestStarted.RemoveListener(AddActiveQuest);
        EventDeclarer.QuestCompleted.RemoveListener(HandleQuestCompleted);
    }

    public bool CheckQuestCompletion(Quest_SO activeQuest)
    {
        Dictionary<RepeatableAccomplishments, int> goalReps = activeQuest.IntQuestReqs.ToDictionary(item => item.quest, item => item.reps);
        if (goalReps.Keys.Count > QuestRepTracker.Keys.Count)
        {
            Logging.Info($"Dict<repeatable quests, completed repetitions>().keys was not to the quest {activeQuest.QuestName} number of required quests ");
            return false;
        }
        else
        {
            Logging.Info($"Dict<repeatable quests, completed repetitions>().keys was equal to the quest {activeQuest.QuestName} number of required quests ");
        }
        bool complete = true;
        foreach (KeyValuePair<RepeatableAccomplishments, int> kvp in goalReps)
        {
            if (QuestRepTracker.TryGetValue(kvp.Key, out int reps) && reps >= kvp.Value)
            {
                Logging.Info($"Repetitions of quest {kvp.Key} was {reps} which is >=  the quest number of required repetitions ({kvp.Value} ) ");
                continue;
            }
            else
            {
                Logging.Info($"Repetitions of quest {kvp.Key} was {reps} which is <=  the quest number of required repetitions ({kvp.Value} ) ");
                return false;
            }
        }
        return complete;
    }

    public void AddActiveQuest(Quest_SO quest)
    {
        Logging.Info($"Active quest added {quest.QuestName}");
        if (!ActiveQuests.Contains(quest)) ActiveQuests.Add(quest);
    }

    public void HandleQuestCompleted(Quest_SO quest)
    {
        if (ActiveQuests.Contains(quest)) ActiveQuests.Remove(quest);
        if (!CompletedQuests.Contains(quest)) CompletedQuests.Add(quest);
        //EventDeclarer.QuestCompleted?.Invoke(quest);
        GrantCompletionRewards(quest);
    }
    public void GrantCompletionRewards(Quest_SO quest)
    {
        for (int i = 0; i < quest.BenefittedElements.Count; i++)
        {
            AlchemyInventory.IncemementElementalKnowledge(quest.BenefittedElements[i], quest.KnowledgeReward);
        }
        AlchemyInventory.GainTool(quest.AlchemyToolReward);
        AlchemyInventory.AlterIngredientNum(quest.AlchemyLootReward, quest.AlchemyLootNum);
    }
    public void IncrementIntQuest(RepeatableAccomplishments intQuest, int value = 1)
    {
        Logging.Info($"Increment func called");

        if (QuestRepTracker.TryGetValue(intQuest, out int reps))
        {
            QuestRepTracker[intQuest] += value;
            Logging.Info($"quest: {intQuest} increased by {value} Current total num = {reps + value}");

        }
        else
        {
            QuestRepTracker.Add(intQuest, value);
            Logging.Info($"quest: {intQuest} increased by {value}. Current total num = {value}");
        }
    }
    public void IncrementEnemyDefeated()
    {
        QuestRepTracker[RepeatableAccomplishments.DefeatEnemies]++;
    }

}

