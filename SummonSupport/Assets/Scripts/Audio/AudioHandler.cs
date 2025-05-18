using UnityEngine;
using System.Collections.Generic;

public class AudioHandler : MonoBehaviour
{
    public AudioHandler Instance;
    private AudioSource audioSource;
    public AudioClip[] audioClips;

    void Awake()
    {
        Instance = this;
        audioSource = GetComponent<AudioSource>();

    }
    void Start()
    {
  
    }

    void BeginSound(string clipName)
    {
        AudioClip soughtClip = System.Array.Find(audioClips, c => c.name == clipName);
        if (soughtClip != null)
        {
            audioSource.clip = soughtClip;
            audioSource.Play();
        }
    }
}

    //audioSource = GetComponent<AudioSource>();
    //Invoke("BeginSound", 3f);
    //audioSource.clip = audioClips[1];
    //string clipName = "PfuelSelfConfidence";
