using System.Collections.Generic;
using System.Linq;
using Unity.AppUI.Core;
using Unity.Entities.UniversalDelegates;
using Unity.Rendering.Authoring;
using Unity.VisualScripting;
using UnityEditor.Build.Pipeline;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class SetupManager : MonoBehaviour
{
    [field: Header("Setup particle effect color handler")]

    [field: SerializeField] public ParticleSystem BleedEffect { get; private set; } = null;
    [field: SerializeField] public GradientLibraryAsset colorGradientLibrary { get; private set; } = null;
    [field: SerializeField] public Material[] GlowMaterials { get; private set; } = null;



    [field: Header("Setup ability library")]

    [field: SerializeField] public AbilityLibrary_SO ElementToAbilityLibrary_SO { get; private set; } = null;
    [field: SerializeField] public List<Ability> AllAbilities { get; private set; } = null;
    [field: SerializeField] public StatusEffectsLibrary StatusEffectsLibrary { get; private set; }






    void Awake()
    {
        if (colorGradientLibrary != null && BleedEffect != null && GlowMaterials.Count() == 4)
        {
            ColorChanger.Setup(BleedEffect, colorGradientLibrary, GlowMaterials);
        }
        else throw new System.Exception("there were too few Glow shaders to initialize");

        if (ElementToAbilityLibrary_SO != null)
        {
            AbilityLibrary.SetAbilityLibrary(ElementToAbilityLibrary_SO);
        }
        else throw new System.Exception("The ability library scriptable object is not loaded into the SetupManager");
        if (StatusEffectsLibrary.entries.Count() != 0)
        {
            AbilityLibrary.SetStatusEffectLibrary(StatusEffectsLibrary);
        }
        else throw new System.Exception("The status effects scriptable objects are not loaded into the SetupManager");

    }
    public StatusEffectsLibrary GetStatusEffectLibrary()
    {
        return StatusEffectsLibrary;
    }

    //public static StatusEffects GetStatusEffect(StatusEffectType type)
    //{
    //    //if (StatusEffectDict.TryGetValue(type, out StatusEffects status)) return status;
    //    //else throw new System.Exception($"The status effect type {type} you have tried to search was  ot present in tge setup manager script");
    //
    //}

}
