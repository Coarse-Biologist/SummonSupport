using UnityEngine;

public class CreatureAudioHandler : LivingBeingAudioHandler
{

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        CreatureAudioSource = AudioHandler.Instance.audioSource;
        if (CreatureAudioSource == null)
        {
            CreatureAudioSource = gameObject.AddComponent<AudioSource>();
        }
    }


}
