using Quest;
using SummonSupportEvents;
using UnityEngine;

public class LocationBasedQuest : MonoBehaviour, IQuest
{
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
        }
    }
    public void TriggerDialogue()
    {
        if (AudioAndDialogue != null)
            EventDeclarer.PlayerDialogue?.Invoke(AudioAndDialogue);
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
