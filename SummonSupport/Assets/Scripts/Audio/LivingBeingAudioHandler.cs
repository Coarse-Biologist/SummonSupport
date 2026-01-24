using System;
using System.Collections;
using UnityEngine;

public abstract class LivingBeingAudioHandler : MonoBehaviour
{
    protected AudioSource CreatureAudioSource;
    [field: SerializeField] public AudioClip[] CreatureDamagedClips { private set; get; } = new AudioClip[5];
    [field: SerializeField] public WaitForSeconds DamageSoundDelay { private set; get; } = new WaitForSeconds(.2f);

    public IEnumerator CreatureDamagedSound(float damageAmount)
    {
        damageAmount = Math.Abs(damageAmount);
        float volume = Math.Min(damageAmount / 100f, .5f);

        Debug.Log($"Triggering creature damaged sound.");
        yield return DamageSoundDelay;
        AudioClip clip = CreatureDamagedClips[UnityEngine.Random.Range(0, CreatureDamagedClips.Length)];
        if (clip != null)
        {
            CreatureAudioSource.PlayOneShot(clip, volume);
        }
    }

}
