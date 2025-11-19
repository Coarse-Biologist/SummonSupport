using System.Collections.Generic;
using System.Linq;

using UnityEngine;

public class SetupManager : MonoBehaviour
{
    [field: Header("Setup particle effect color handler")]
    public static SetupManager Instance { get; private set; }
    [field: SerializeField] public ParticleSystem BleedEffect { get; private set; } = null;
    [field: SerializeField] public GradientLibraryAsset colorGradientLibrary { get; private set; } = null;
    [field: SerializeField] public Material[] GlowMaterials { get; private set; } = null;



    [field: Header("Setup ability library")]

    [field: SerializeField] public AbilityLibrary_SO ElementToAbilityLibrary_SO { get; private set; } = null;
    [field: SerializeField] public List<Ability> AllAbilities { get; private set; } = null;
    [field: SerializeField] public StatusEffectsLibrary StatusEffectsLibrary { get; private set; }
    [field: SerializeField] public GameObject LocSphere { get; private set; }









    void Awake()
    {
        if (Instance != null) Destroy(this);
        else Instance = this;

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
    public void DebugLocation(Vector3 loc, Color specialColorRequest)
    {
        GameObject instance = Instantiate(LocSphere, loc, Quaternion.identity);
        instance.GetComponent<MeshRenderer>().material.color = specialColorRequest;
        Destroy(instance, 3f);
    }

    //public static StatusEffects GetStatusEffect(StatusEffectType type)
    //{
    //    //if (StatusEffectDict.TryGetValue(type, out StatusEffects status)) return status;
    //    //else throw new System.Exception($"The status effect type {type} you have tried to search was  ot present in tge setup manager script");
    //
    //}

}
