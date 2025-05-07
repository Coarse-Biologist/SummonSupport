using Quest;
using SummonSupportEvents;
using UnityEngine;

public class LocationBasedQuest : MonoBehaviour, IQuest
{
    [SerializeField] public Quest_SO completesQuest;
    [SerializeField] public Quest_SO grantsQuest;


    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Logging.Info("player in collider2d");

            CompleteQuest();
            GrantQuest();
        }
    }
    public void CompleteQuest()
    {
        if (completesQuest != null)
            EventDeclarer.QuestCompleted?.Invoke(completesQuest);
    }

    public void GrantQuest()
    {
        if (grantsQuest != null)
            EventDeclarer.QuestStarted?.Invoke(grantsQuest);
    }

}
