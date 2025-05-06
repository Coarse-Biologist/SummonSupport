using Quest;
using SummonSupportEvents;
using UnityEngine;

public class LocationBasedQuest : MonoBehaviour, IQuest
{
    [SerializeField] public Quest_SO quest;

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Logging.Info("player in collider2d");

            CompleteQuest();
        }
    }
    public void CompleteQuest()
    {
        EventDeclarer.QuestCompleted?.Invoke(quest);
    }

    public void GrantQuest()
    {
        //throw new System.NotImplementedException();
    }


}
