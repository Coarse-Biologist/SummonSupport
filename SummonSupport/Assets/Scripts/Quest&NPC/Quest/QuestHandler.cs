using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.Events;
using Quest;
using SummonSupportEvents;
//using UnityEditor.Build.Pipeline;

/// <summary>
/// The quests of the game, shared by everyone in it. The host (or the single player) decides them:
/// - progress made on a client (collecting, crafting) is sent to the host, which counts it (IncrementIntQuest);
/// - the host tells every client when a quest starts or completes and how far each count has got (NetworkPlayer.Quests.cs);
/// - each machine gives its own player a completed quest's rewards;
/// - a joining player is sent the whole state, and the state is saved with the session (QuestProgressData).
/// </summary>
public class QuestHandler : MonoBehaviour
{
    [field: SerializeField] public Quest_SO sceneStartingQuest;
    [field: SerializeField] public List<Questline_Pair> Questline_Chain;

    [field: SerializeField] public List<Quest_LocationGoal> QuestGoalLocDict;

    public static QuestHandler Instance;
    public static List<Quest_SO> CompletedQuests = new List<Quest_SO>();
    public static List<BoolAccomplishments> CompletedBoolQuests = new();
    public static List<Quest_SO> ActiveQuests = new List<Quest_SO>();
    public static Dictionary<RepeatableAccomplishments, int> QuestRepTracker = new Dictionary<RepeatableAccomplishments, int>()

    {
        {RepeatableAccomplishments.EnemiesDefeated, 0},
        {RepeatableAccomplishments.KnowledgeGained, 0},
        {RepeatableAccomplishments.MinionsCrafted, 0},
        {RepeatableAccomplishments.CoresCollected, 0},
        {RepeatableAccomplishments.OrgansCollected, 0},
        {RepeatableAccomplishments.EtherCollected, 0},
        {RepeatableAccomplishments.CorePowerUsed, 0},
        {RepeatableAccomplishments.OrganMassUsed, 0},
        {RepeatableAccomplishments.EtherDensityUsed, 0},
    };
    public Dictionary<RepeatableAccomplishments, int> TotalRepTracker = new Dictionary<RepeatableAccomplishments, int>()
    {
        {RepeatableAccomplishments.EnemiesDefeated, 0},
        {RepeatableAccomplishments.KnowledgeGained, 0},
        {RepeatableAccomplishments.MinionsCrafted, 0},
        {RepeatableAccomplishments.CoresCollected, 0},
        {RepeatableAccomplishments.OrgansCollected, 0},
        {RepeatableAccomplishments.EtherCollected, 0},
        {RepeatableAccomplishments.CorePowerUsed, 0},
        {RepeatableAccomplishments.OrganMassUsed, 0},
        {RepeatableAccomplishments.EtherDensityUsed, 0},

    };
    // Quests from a save or the host that aren't loaded here (e.g. another level's), kept so saving doesn't lose them.
    private static readonly List<string> unknownActiveQuests = new();
    private static readonly List<string> unknownCompletedQuests = new();
    private static Dictionary<string, Quest_SO> questsByName;

    void Awake()
    {
        Instance = this;
    }
    void Start()
    {
        if (!NetworkSession.IsAuthority) return;
        QuestProgressData saved = GameSession.TakeQuestProgress();
        if (saved != null)
        {
            ApplyProgress(saved);
            NetworkPlayer.BroadcastQuestProgress(); // e.g. a restart: everyone goes back to the saved quests
        }
        if (sceneStartingQuest != null && !HasActiveQuest(sceneStartingQuest) && !HasCompletedQuest(sceneStartingQuest))
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
    public static bool HasActiveQuest(Quest_SO quest) => ActiveQuests.Contains(quest);
    public static bool HasCompletedQuest(Quest_SO quest) => CompletedQuests.Contains(quest);
    public static bool HasActiveLocationQuest(out Transform goalTransform)
    {
        goalTransform = null;
        foreach (Quest_SO q in ActiveQuests)
        {
            if (!q.HasGoalLocation) continue;

            foreach (Quest_LocationGoal kvp in Instance.QuestGoalLocDict)
            {
                if (kvp.quest == q)
                {
                    goalTransform = kvp.GoalLocation;
                    return true;
                }
            }
        }
        return false;

    }

    public bool CheckQuestCompletion(Quest_SO activeQuest)
    {
        bool complete = true;
        if (activeQuest.IntQuestReqs.Count == 0 && activeQuest.BoolQuestReqs.Count == 0) return false;
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
    // On a client this only hears quests the host started (see NetworkPlayer.QuestStartedClientRpc).
    public void AddActiveQuest(Quest_SO quest)
    {
        if (quest == null) throw new System.Exception("QuestHandler: AddActiveQuest called with null quest");
        //debuf.Log($"Active quest added {quest.QuestName}");
        if (!ActiveQuests.Contains(quest))
        {
            ActiveQuests.Add(quest);
            if (NetworkSession.IsHost) NetworkPlayer.BroadcastQuestStarted(quest);
        }
        if (PlayerUIHandler.Instance != null) PlayerUIHandler.Instance.ShowQuestInfo(quest);
    }

    public void HandleQuestCompleted(Quest_SO quest)
    {
        if (ActiveQuests.Contains(quest)) // if this quest was even something the player was allowed to be striving after
        {
            Debug.Log($"This quest was completeded: {quest}");
            ActiveQuests.Remove(quest);
            if (!CompletedQuests.Contains(quest)) CompletedQuests.Add(quest);
            if (NetworkSession.IsHost) NetworkPlayer.BroadcastQuestCompleted(quest);
            if (PlayerUIHandler.Instance != null) PlayerUIHandler.Instance.ShowCompletedQuestInfo(quest);
            GrantCompletionRewards(quest);
            if (PlayerStats.Instance != null) FloatingInfoHandler.Instance.ShowDialogue(PlayerStats.Instance.transform, "Quest Completed!");
        }
    }
    // Runs on every machine for its own player, so everyone in the game gets the rewards.
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
            AlchemyInventory.ConvertIngredientsToPotential(ingredient, quest.AlchemyLootNum);
        }
        FloatingInfoHandler.Instance.DisplayXPGain(quest.XP_Reward);
        if (PlayerStats.Instance != null) PlayerStats.Instance.GainXP(quest.XP_Reward);
    }
    public void IncrementIntQuest(RepeatableAccomplishments intQuest, int value = 1)
    {
        if (!NetworkSession.IsAuthority)
        {
            // Done on this client (collecting, crafting): the host counts it for everyone.
            if (value != 0) NetworkPlayer.ReportQuestProgress(intQuest, value);
            return;
        }
        //Debug.Log($"Increment func called for {intQuest}. change value  = {value}");


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
        if (NetworkSession.IsHost) NetworkPlayer.BroadcastQuestCount(intQuest, QuestRepTracker[intQuest]);
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
                CheckForNextQuest(activeQuest);
            }
        }
    }
    private void CheckForNextQuest(Quest_SO completedQuest)
    {
        foreach (Questline_Pair pair in Questline_Chain)
        {
            if (pair.CompletedQuest == completedQuest)
            {
                if (pair.NextQuest != null) EventDeclarer.QuestStarted?.Invoke(pair.NextQuest);
                break; //#TODO are you sure about this? Limits possibility of quest branching
            }
        }
    }
    public void IncrementEnemyDefeated(LivingBeing livingBeing)
    {
        if (!NetworkSession.IsAuthority) return;
        QuestRepTracker[RepeatableAccomplishments.EnemiesDefeated]++;
        TotalRepTracker[RepeatableAccomplishments.EnemiesDefeated]++;
        if (NetworkSession.IsHost) NetworkPlayer.BroadcastQuestCount(RepeatableAccomplishments.EnemiesDefeated, QuestRepTracker[RepeatableAccomplishments.EnemiesDefeated]);
        CheckQuestCompletionsForActiveQuests(RepeatableAccomplishments.EnemiesDefeated);

    }
    public static string GetQuestCompletionStats()
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
    public static string GetQuestInfo(Quest_SO quest)
    {
        string info = $"{quest.QuestName}\n";
        info += $"{quest.PresentationString}\n";
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




    #region Shared state: saves, joining players and the host's messages

    /// <summary>Forgets all quest progress, e.g. when a game ends or is reloaded from its save.</summary>
    public static void ResetProgress()
    {
        ActiveQuests.Clear();
        CompletedQuests.Clear();
        CompletedBoolQuests.Clear();
        unknownActiveQuests.Clear();
        unknownCompletedQuests.Clear();
        foreach (RepeatableAccomplishments accomplishment in QuestRepTracker.Keys.ToList())
            QuestRepTracker[accomplishment] = 0;
    }

    public static QuestProgressData BuildProgress()
    {
        QuestProgressData progress = new();
        progress.activeQuests.AddRange(ActiveQuests.Where(q => q != null).Select(q => q.name));
        progress.activeQuests.AddRange(unknownActiveQuests);
        progress.completedQuests.AddRange(CompletedQuests.Where(q => q != null).Select(q => q.name));
        progress.completedQuests.AddRange(unknownCompletedQuests);
        progress.completedBoolQuests.AddRange(CompletedBoolQuests);
        foreach (KeyValuePair<RepeatableAccomplishments, int> kvp in QuestRepTracker)
            progress.counts.Add(new QuestCount { accomplishment = kvp.Key, count = kvp.Value });
        return progress;
    }

    /// <summary>
    /// Replaces the quest progress with a saved or sent one. Doors, NPCs and the quest display are told about
    /// each quest again so they catch up; that grants no rewards, as those quests are already done or still going.
    /// </summary>
    public static void ApplyProgress(QuestProgressData progress)
    {
        ResetProgress();
        if (progress == null) return;

        foreach (string questName in progress.completedQuests)
        {
            Quest_SO quest = FindQuest(questName);
            if (quest == null) { if (!unknownCompletedQuests.Contains(questName)) unknownCompletedQuests.Add(questName); }
            else if (!CompletedQuests.Contains(quest)) CompletedQuests.Add(quest);
        }
        foreach (string questName in progress.activeQuests)
        {
            Quest_SO quest = FindQuest(questName);
            if (quest == null) { if (!unknownActiveQuests.Contains(questName)) unknownActiveQuests.Add(questName); }
            else if (!CompletedQuests.Contains(quest) && !ActiveQuests.Contains(quest)) ActiveQuests.Add(quest);
        }
        foreach (BoolAccomplishments accomplishment in progress.completedBoolQuests)
            if (!CompletedBoolQuests.Contains(accomplishment)) CompletedBoolQuests.Add(accomplishment);
        foreach (QuestCount count in progress.counts)
            QuestRepTracker[count.accomplishment] = count.count;

        foreach (Quest_SO quest in CompletedQuests.ToList()) EventDeclarer.QuestCompleted?.Invoke(quest);
        foreach (Quest_SO quest in ActiveQuests.ToList()) EventDeclarer.QuestStarted?.Invoke(quest);
    }

    /// <summary>Client side: the host started a quest.</summary>
    public static void ApplyQuestStarted(string questName)
    {
        Quest_SO quest = FindQuest(questName);
        if (quest == null) return;
        if (Instance != null) EventDeclarer.QuestStarted?.Invoke(quest);
        else if (!ActiveQuests.Contains(quest)) ActiveQuests.Add(quest);
    }

    /// <summary>Client side: the host completed a quest. Its rewards go to this machine's player.</summary>
    public static void ApplyQuestCompleted(string questName)
    {
        Quest_SO quest = FindQuest(questName);
        if (quest == null) return;
        if (Instance != null) EventDeclarer.QuestCompleted?.Invoke(quest);
        else
        {
            ActiveQuests.Remove(quest);
            if (!CompletedQuests.Contains(quest)) CompletedQuests.Add(quest);
        }
    }

    /// <summary>Client side: the host's count for an accomplishment.</summary>
    public static void ApplyQuestCount(RepeatableAccomplishments accomplishment, int count)
    {
        QuestRepTracker[accomplishment] = count;
        if (PlayerUIHandler.Instance == null) return;
        foreach (Quest_SO quest in ActiveQuests)
            if (quest.IntQuestReqs.Exists(req => req.quest == accomplishment)) PlayerUIHandler.Instance.ShowQuestInfo(quest);
    }

    // Every quest a level uses is loaded with it, because the level's objects refer to it.
    public static Quest_SO FindQuest(string questName)
    {
        if (string.IsNullOrEmpty(questName)) return null;
        if (questsByName != null && questsByName.TryGetValue(questName, out Quest_SO known) && known != null) return known;

        questsByName = new Dictionary<string, Quest_SO>();
        foreach (Quest_SO quest in Resources.FindObjectsOfTypeAll<Quest_SO>())
        {
            if (questsByName.TryGetValue(quest.name, out Quest_SO other) && other != quest)
                Debug.LogWarning($"Two quests are named {quest.name}; online and in saves only one of them is used.");
            else questsByName[quest.name] = quest;
        }
        return questsByName.TryGetValue(questName, out Quest_SO found) ? found : null;
    }

    #endregion
}

