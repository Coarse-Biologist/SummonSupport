using System.Collections;
using SummonSupportEvents;
using UnityEngine;

public class PlayerAudioHandler : LivingBeingAudioHandler
{


    private void Start()
    {
        CreatureAudioSource = GetComponent<AudioSource>();
        if (CreatureAudioSource == null)
        {
            CreatureAudioSource = gameObject.AddComponent<AudioSource>();
        }
    }
    void OnEnable()
    {
        EventDeclarer.PlayerDialogue?.AddListener(PlayDialogueSound);
    }
    void OnDisable()
    {
        EventDeclarer.PlayerDialogue?.RemoveListener(PlayDialogueSound);
    }
    public void PlayDialogueSound(DialogueAndAudio_SO dialogue)
    {
        if (dialogue != null && dialogue != null)
        {
            CreatureAudioSource.PlayOneShot(dialogue.playerAudio, .3f);
        }
    }


}
