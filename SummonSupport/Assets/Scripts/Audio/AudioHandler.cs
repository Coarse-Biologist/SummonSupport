using UnityEngine;
using System;


public class AudioHandler : MonoBehaviour
{
    public static AudioHandler Instance;
    public AudioSource audioSource;
    public LayeredSongs_SO[] audioClips = new LayeredSongs_SO[5];
    [field: SerializeField] public float FootstepVolume { private set; get; } = .2f;
    [field: SerializeField] public float GeneralGameVolume { private set; get; } = 1f;
    int roundedInt = 0;
    float newVolume = 0f;

    void Awake()
    {
        Instance = this;
        audioSource = GetComponent<AudioSource>();
        //SetMusicToIntensity(1.5f, LocationSong.level1);
        audioSource.loop = true;

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



    public string GetGeneralGameVolume()
    {
        return $"Volume: {GeneralGameVolume * 100}%";
    }

    #region  Adjust Volume Methods
    public void AdjustGeneralGameVolume(bool increase)
    {
        Debug.Log($"Adjusting volume. Increase: {increase}");
        GeneralGameVolume += increase ? 0.1f : -0.1f; // increase or decrease by 0.1 depedning on true or false
        GeneralGameVolume = Mathf.Clamp(GeneralGameVolume, 0f, 1f); // clamp between 0 and 1
        GeneralGameVolume = (float)Math.Round(GeneralGameVolume, 1); // round to 1 decimal place
    }

    #endregion

}
public enum LocationSong
{
    level1
}

//audioSource = GetComponent<AudioSource>();
//Invoke("BeginSound", 3f);
//audioSource.clip = audioClips[1];
//string clipName = "PfuelSelfConfidence";
