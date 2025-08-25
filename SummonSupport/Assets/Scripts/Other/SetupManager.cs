using System.Collections.Generic;
using UnityEngine;

public class SetupManager : MonoBehaviour
{
    [field: Header("Setup particle effect color handler")]

    [field: SerializeField] public ParticleSystem BleedEffect { get; private set; } = null;
    [field: SerializeField] public GradientLibraryAsset colorGradientLibrary { get; private set; } = null;

    [field: Header("Setup ability library")]

    [field: SerializeField] public AbilityLibrary_SO ElementToAbilityLibrary_SO { get; private set; } = null;
    [field: SerializeField] public List<Ability> AllAbilities { get; private set; } = null;



    void Awake()
    {
        if (colorGradientLibrary != null && BleedEffect != null)
            EffectColorChanger.Setup(BleedEffect, colorGradientLibrary);
        if (ElementToAbilityLibrary_SO != null)
        {
            AbilityLibrary.SetAbilityLibrary(ElementToAbilityLibrary_SO);
        }
    }
}
