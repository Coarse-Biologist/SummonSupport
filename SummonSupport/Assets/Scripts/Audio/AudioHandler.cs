using UnityEngine;
using System;
using SummonSupportEvents;
using UnityEditor.Timeline.Actions;

public class AudioHandler : MonoBehaviour
{
    public AudioHandler Instance;
    private AudioSource audioSource;
    public LayeredSongs_SO[] audioClips = new LayeredSongs_SO[5];
    [field: SerializeField] public AudioClip[] PlayerFootSteps { private set; get; } = new AudioClip[5];
    int roundedInt = 0;
    float newVolume = 0f;

    void Awake()
    {
        Instance = this;
        audioSource = GetComponent<AudioSource>();
        //SetMusicToIntensity(1.5f, LocationSong.level1);
        audioSource.loop = true;

    }
    void OnEnable()
    {
        EventDeclarer.PlayAbilityCastSound?.AddListener(PlayAbilityCastSound);
        EventDeclarer.PlayAbilityImpactSound?.AddListener(PlayAbilityImpactSound);
        EventDeclarer.PlayerFootstep?.AddListener(OnFootstep);


    }
    void OnDisable()
    {
        EventDeclarer.PlayAbilityCastSound?.RemoveListener(PlayAbilityCastSound);
        EventDeclarer.PlayAbilityImpactSound?.RemoveListener(PlayAbilityImpactSound);
        EventDeclarer.PlayerFootstep?.RemoveListener(OnFootstep);


    }


    public void SetMusicToIntensity(float intesity, LocationSong locationSong)
    {
        roundedInt = (int)Math.Floor(intesity);
        audioSource.clip = audioClips[(int)locationSong].layers[roundedInt];
        newVolume = intesity - roundedInt;
        audioSource.volume = newVolume;
        if (audioSource.clip != null)
            audioSource.Play();
    }

    private void PlayAbilityCastSound(Ability ability)
    {
        if (ability.Sounds != null && ability.Sounds.CastSound != null)
            audioSource.PlayOneShot(ability.Sounds.CastSound, 1);
    }
    private void PlayAbilityImpactSound(Ability ability)
    {
        if (ability.Sounds.ImpactSound != null)
            audioSource.PlayOneShot(ability.Sounds.ImpactSound, 1);
    }
    public void OnFootstep()
    {
        audioSource.PlayOneShot(PlayerFootSteps[UnityEngine.Random.Range(0, PlayerFootSteps.Length)], UnityEngine.Random.value);
    }

}
public enum LocationSong
{
    level1
}

//audioSource = GetComponent<AudioSource>();
//Invoke("BeginSound", 3f);
//audioSource.clip = audioClips[1];
//string clipName = "PfuelSelfConfidence";
