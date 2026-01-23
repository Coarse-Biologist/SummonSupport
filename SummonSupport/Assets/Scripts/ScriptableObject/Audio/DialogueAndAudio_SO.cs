using UnityEngine;

[CreateAssetMenu(fileName = "DialogueAndAudio_SO", menuName = "DialogueAndAudio/DialogueAndAudio")]

public class DialogueAndAudio_SO : ScriptableObject
{
    [field: SerializeField] public string playerDialogue { get; private set; }
    [field: SerializeField] public AudioClip playerAudio { get; private set; }
}
