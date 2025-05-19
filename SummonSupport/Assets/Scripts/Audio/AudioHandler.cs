using UnityEngine;
using System;
using SummonSupportEvents;

public class AudioHandler : MonoBehaviour
{
    public AudioHandler Instance;
    private AudioSource audioSource;
    public LayeredSongs_SO[] audioClips;
    int roundedInt = 0;
    float newVolume = 0f;

    void Awake()
    {
        Instance = this;
        audioSource = GetComponent<AudioSource>();
        SetMusicToIntensity(1.5f, LocationSong.level1);
        audioSource.loop = true;

    }
    void OnEnable()
    {
        //EventDeclarer.
    }
    void OnDisable()
    {

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

}
public enum LocationSong
{
    level1
}

//audioSource = GetComponent<AudioSource>();
//Invoke("BeginSound", 3f);
//audioSource.clip = audioClips[1];
//string clipName = "PfuelSelfConfidence";
