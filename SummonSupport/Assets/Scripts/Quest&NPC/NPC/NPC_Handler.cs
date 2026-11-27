using UnityEngine;
using Quest;
using SummonSupportEvents;

public class NPC_Handler : MonoBehaviour
{

    public bool dialogueUnlocked { private set; get; } = false;
    [SerializeField] public NPC_SO npcData;
    private Dialogue_SO npcDialogue;

    void Awake()
    {
        npcDialogue = npcData.Dialogue;
    }


    void OnEnable()
    {
        EventDeclarer.QuestCompleted.AddListener(CheckUnlockDialogue);
        // The quest may have been done before this NPC loaded (a loaded save, or a game joined part way).
        if (npcDialogue != null && npcDialogue.questToUnlockDialogue != null && QuestHandler.HasCompletedQuest(npcDialogue.questToUnlockDialogue))
            UnlockDialogue();
    }
    void OnDisable()
    {
        EventDeclarer.QuestCompleted.RemoveListener(CheckUnlockDialogue);
    }

    private void CheckUnlockDialogue(Quest_SO quest)
    {
        if (quest == npcDialogue.questToUnlockDialogue)
        {
            UnlockDialogue();
        }
    }

    private void UnlockDialogue()
    {
        //Logging.Info("Dialogue unlocked");
        dialogueUnlocked = true;
    }


}