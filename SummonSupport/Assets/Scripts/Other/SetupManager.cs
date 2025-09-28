using System.Collections.Generic;
using System.Linq;
using Unity.Rendering.Authoring;
using Unity.VisualScripting;
using UnityEngine;

public class SetupManager : MonoBehaviour
{
    [field: Header("Setup particle effect color handler")]

    [field: SerializeField] public ParticleSystem BleedEffect { get; private set; } = null;
    [field: SerializeField] public GradientLibraryAsset colorGradientLibrary { get; private set; } = null;
    [field: SerializeField] public Material[] GlowMaterials { get; private set; } = null;



    [field: Header("Setup ability library")]

    [field: SerializeField] public AbilityLibrary_SO ElementToAbilityLibrary_SO { get; private set; } = null;
    [field: SerializeField] public List<Ability> AllAbilities { get; private set; } = null;






    void Awake()
    {
        if (colorGradientLibrary != null && BleedEffect != null && GlowMaterials.Count() == 4)
        {
            EffectColorChanger.Setup(BleedEffect, colorGradientLibrary, GlowMaterials);
        }
        else throw new System.Exception("there were too few Glow shaders to initialize");

        if (ElementToAbilityLibrary_SO != null)
        {
            AbilityLibrary.SetAbilityLibrary(ElementToAbilityLibrary_SO);
        }
        else throw new System.Exception("The ability library scriptable object is not loaded into the SetupManager");
    }
}
