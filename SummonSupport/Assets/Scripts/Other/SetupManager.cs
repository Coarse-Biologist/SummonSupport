using UnityEngine;

public class SetupManager : MonoBehaviour
{
    [field: SerializeField] public ParticleSystem BleedEffect { get; private set; } = null;
    [field: SerializeField] public GradientLibraryAsset colorGradientLibrary { get; private set; } = null;

    void Awake()
    {
        if (colorGradientLibrary != null && BleedEffect != null)
            EffectColorChanger.Setup(BleedEffect, colorGradientLibrary);
    }
}
