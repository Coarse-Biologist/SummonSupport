using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class QuestHandler : MonoBehaviour
{
    public List<Quest_SO> CompletedQuests = new List<Quest_SO>();
    public List<BoolAccomplishments> CompletedBoolQuests;
    public List<Quest_SO> ActiveQuests = new List<Quest_SO>();
    public List<RepeatableQuestDict> QuestRepTracker = new List<RepeatableQuestDict>();

    void OnEnable()
    {
        //events for different quest progress
    }
    void OnDisable()
    {
        // unsubscribe to events for different quest progress
    }



    public bool CheckQuestCompletion(Quest_SO activeQuest)
    {
        Dictionary<RepeatableAccomplishments, int> accomplishedReps = QuestRepTracker.ToDictionary(item => item.quest, item => item.reps);
        Dictionary<RepeatableAccomplishments, int> goalReps = activeQuest.IntQuestReqs.ToDictionary(item => item.quest, item => item.reps);
        if (goalReps.Keys.Count > accomplishedReps.Keys.Count) return false;
        bool complete = true;
        foreach (KeyValuePair<RepeatableAccomplishments, int> kvp in goalReps)
        {
            if (accomplishedReps.TryGetValue(kvp.Key, out int reps) && reps >= kvp.Value) continue;
            else return false;
        }
        return complete;
    }
}
