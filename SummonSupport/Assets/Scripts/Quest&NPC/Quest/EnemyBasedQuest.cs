using UnityEngine;
using Quest;
using SummonSupportEvents;

public class EnemyBasedQuest : MonoBehaviour, IQuest
{
    [SerializeField] public Quest_SO quest;

    public void CompleteQuest()
    {
        EventDeclarer.QuestCompleted?.Invoke(quest);
    }

    public void GrantQuest()
    {
        //throw new System.NotImplementedException();
    }

    void OnDestroy()
    {
        // The enemy is destroyed on every machine; the host decides the quests and tells everyone (see QuestHandler).
        if (NetworkSession.IsAuthority) CompleteQuest();
    }
}
