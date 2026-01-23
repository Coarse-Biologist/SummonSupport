using SummonSupportEvents;
using UnityEngine;

public class PlayerAudioHandler : MonoBehaviour
{
    private AudioSource PlayerAudioSource;

    private void Start()
    {
        PlayerAudioSource = GetComponent<AudioSource>();
        if (PlayerAudioSource == null)
        {
            PlayerAudioSource = gameObject.AddComponent<AudioSource>();
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
            PlayerAudioSource.PlayOneShot(dialogue.playerAudio, 1f);
        }
    }
}
