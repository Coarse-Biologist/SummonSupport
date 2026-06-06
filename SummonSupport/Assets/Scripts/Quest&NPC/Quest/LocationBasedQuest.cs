using Quest;
using SummonSupportEvents;
using UnityEngine;

public class LocationBasedQuest : MonoBehaviour, IQuest
{
    [field: SerializeField] public string Description { private set; get; } = "";
    [SerializeField] public Quest_SO completesQuest;
    [SerializeField] public Quest_SO grantsQuest;
    [field: SerializeField] private DialogueAndAudio_SO AudioAndDialogue;

    void OnTriggerEnter(Collider collision)
    {
        if (collision.gameObject.TryGetComponent(out PlayerStats playerAudio))
        {
            //Logging.Info("player in collider2d");
            //AudioHandler.Instance.PlaySound(QuestAudioClip);
            TriggerDialogue();
            CompleteQuest();
            GrantQuest();
            Destroy(gameObject);
        }
    }
    public void TriggerDialogue()
    {
        if (AudioAndDialogue != null)
            EventDeclarer.PlayerDialogue?.Invoke(AudioAndDialogue);
    }
    public void CompleteQuest()
    {
        if (QuestHandler.HasActiveQuest(completesQuest))
        {
            if (completesQuest != null)
                EventDeclarer.QuestCompleted?.Invoke(completesQuest);
        }
    }

    public void GrantQuest()
    {
        if (QuestHandler.HasCompletedQuest(completesQuest) & !QuestHandler.HasActiveQuest(grantsQuest))
        {
            if (grantsQuest != null)
                EventDeclarer.QuestStarted?.Invoke(grantsQuest);
        }
    }

}
