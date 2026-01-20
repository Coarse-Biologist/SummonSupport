using UnityEngine;

[CreateAssetMenu(fileName = "AbilitySoundPackage", menuName = "ScriptableObjects/AbilitySoundPackage", order = 1)]
public class AbilitySoundPackage : ScriptableObject
{
    public AudioClip CastSound;
    public AudioClip ActiveSound;
    public AudioClip ImpactSound;

}
