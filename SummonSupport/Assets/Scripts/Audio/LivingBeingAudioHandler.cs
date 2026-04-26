using System;
using System.Collections;
using UnityEngine;

public class LivingBeingAudioHandler : MonoBehaviour
{
    public AudioSource CreatureAudioSource { protected set; get; }
    [field: SerializeField] public AudioClip[] CreatureDamagedClips { private set; get; } = new AudioClip[5];
    [field: SerializeField] public WaitForSeconds DamageSoundDelay { private set; get; } = new WaitForSeconds(.2f);
    protected AudioHandler AudioHandlerInstance;

    private void Start()
    {
        AudioHandlerInstance = AudioHandler.Instance;
        if (AudioHandlerInstance == null)
        {
            throw new Exception("AudioHandler instance not found in scene.");
        }
        CreatureAudioSource = GetComponent<AudioSource>();
        if (CreatureAudioSource == null)
        {
            CreatureAudioSource = gameObject.AddComponent<AudioSource>();
        }

    }
    public IEnumerator CreatureDamagedSound(float damageAmount)
    {
        //Debug.Log(AudioHandlerInstance + "exists?");
        damageAmount = Math.Abs(damageAmount);
        float volume = Math.Min(damageAmount / 100f, .3f);

        //Debug.Log($"Triggering creature damaged sound.");
        yield return DamageSoundDelay;
        AudioClip clip = CreatureDamagedClips[UnityEngine.Random.Range(0, CreatureDamagedClips.Length)];
        if (clip != null)
        {
            CreatureAudioSource.PlayOneShot(clip, volume * AudioHandlerInstance.GeneralGameVolume);
        }
    }
    public void PlayAbilityCastSound(Ability ability)
    {
        AudioClip clip;
        if (ability.Sounds != null && ability.Sounds.CastSounds.Length != 0)
        {
            clip = ability.Sounds.GetCastSound();
            if (clip != null)
            {
                CreatureAudioSource.PlayOneShot(clip, .3f * AudioHandlerInstance.GeneralGameVolume);
            }
        }
    }
    public void PlayAbilityImpactSound(Ability ability)
    {
        AudioClip clip;
        if (ability.Sounds != null && ability.Sounds.ImpactSounds.Length != 0)
        {
            clip = ability.Sounds.GetImpactSound();
            if (clip != null)
            {
                CreatureAudioSource.PlayOneShot(clip, .5f * AudioHandlerInstance.GeneralGameVolume);
            }
        }
    }

}
