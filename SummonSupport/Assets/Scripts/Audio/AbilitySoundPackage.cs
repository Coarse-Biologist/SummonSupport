using UnityEngine;

[CreateAssetMenu(fileName = "AbilitySoundPackage", menuName = "ScriptableObjects/AbilitySoundPackage", order = 1)]
public class AbilitySoundPackage : ScriptableObject
{
    public AudioClip[] CastSounds = new AudioClip[3];
    public AudioClip ActiveSound;
    public AudioClip[] ImpactSounds = new AudioClip[3];

    public AudioClip GetCastSound()
    {
        AudioClip clip = CastSounds[Random.Range(0, CastSounds.Length)];
        return clip;
        //if (clip != null) return clip;
        //else throw new System.Exception("No cast sound found in AbilitySoundPackage");
    }
    public AudioClip GetActiveSound()
    {
        if (ActiveSound != null) return ActiveSound;
        else
            return ActiveSound;
    }
    public AudioClip GetImpactSound()
    {
        AudioClip clip = ImpactSounds[Random.Range(0, ImpactSounds.Length)];
        return clip;
        //if (clip != null) return clip;
        //else throw new System.Exception("No impact sound found in AbilitySoundPackage");
    }

}
