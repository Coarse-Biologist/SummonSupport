using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public static class ColorChanger
{


    private static ParticleSystem BleedEffect;
    private static GradientLibraryAsset colorGradientLibrary;
    private static bool ready = false;
    public static Dictionary<Element, Color> ElementToColorDict = new();
    public static Material[] GlowMaterials;


    public static void Setup(ParticleSystem bE, GradientLibraryAsset cGL, Material[] glowMaterials)
    {
        BleedEffect = bE;
        colorGradientLibrary = cGL;
        GlowMaterials = glowMaterials;
        ready = true;
        InitializeColorDict();
    }

    private static void InitializeColorDict()
    {
        if (ElementToColorDict.TryGetValue(Element.Cold, out Color color)) return;
        ElementToColorDict.Add(Element.Cold, Color.softBlue);
        ElementToColorDict.Add(Element.Water, Color.aquamarine);
        ElementToColorDict.Add(Element.Plant, Color.forestGreen);
        ElementToColorDict.Add(Element.Bacteria, Color.yellowGreen);
        ElementToColorDict.Add(Element.Poison, Color.oliveDrab);
        ElementToColorDict.Add(Element.Virus, Color.limeGreen);
        ElementToColorDict.Add(Element.Acid, Color.chartreuse);
        ElementToColorDict.Add(Element.Heat, Color.orangeRed);
        ElementToColorDict.Add(Element.Radiation, Color.orange);
        ElementToColorDict.Add(Element.Electricity, Color.yellow);
        ElementToColorDict.Add(Element.Psychic, Color.purple);
        ElementToColorDict.Add(Element.Earth, Color.saddleBrown);
        ElementToColorDict.Add(Element.Fungi, Color.slateGray);
        ElementToColorDict.Add(Element.Air, Color.lightGray);
        ElementToColorDict.Add(Element.Light, Color.lightGoldenRodYellow);

    }


    public static void ChangeParticleSystemColor(LivingBeing livingBeing, ParticleSystem particleSystem)
    {
        Element strongestElement = livingBeing.GetHighestAffinity(out float value);
        if (livingBeing.GetAffinity(strongestElement) > 0)
        {
            var colorGradient = colorGradientLibrary.GetGradientByElement(strongestElement);
            SetparticleSystemGradient(particleSystem, colorGradient);
        }
    }
    public static void ChangeParticleSystemColor(Element element, ParticleSystem particleSystem)
    {
        var colorGradient = colorGradientLibrary.GetGradientByElement(element);
        SetparticleSystemGradient(particleSystem, colorGradient);
    }

    private static void SetparticleSystemGradient(ParticleSystem ps, Gradient colorGradient)
    {
        var main = ps.main;                 // get a copy of the main module
        main.startColor = Color.white;
        var col = ps.colorOverLifetime;
        col.enabled = true;
        col.color = new ParticleSystem.MinMaxGradient(colorGradient);
    }

    public static Color GetColorFromElementDict(Element element)
    {
        if (ElementToColorDict.TryGetValue(element, out Color color)) return color;
        if (element != Element.None) Debug.LogWarning($"No color registered for element {element}.");
        return Color.white;
    }
    public static Color GetRandomColorFromElementGradient(Element element)
    {
        Gradient elementalGradient = colorGradientLibrary != null ? colorGradientLibrary.GetGradientByElement(element) : null;
        if (elementalGradient == null) return GetColorFromElementDict(element);
        return elementalGradient.Evaluate(UnityEngine.Random.value);
    }
    public static Material SetMaterialColor(Material mat, Color color)
    {
        mat.SetColor(EmissionColorId, color);
        // Make sure the shader's emission keyword is enabled
        mat.EnableKeyword("_EMISSION");
        return mat;
    }

    #region Cached element materials
    // Materials are created once per element (and per gradient sample) instead of on every call.
    // Previously every projectile shot / creature spawn allocated new Materials that were never destroyed.
    private const int GradientSamples = 4;
    private static readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor");
    private static readonly Dictionary<(Element, int), Material> elementMaterialCache = new();
    // The materials a renderer had before the first legacy recolor, so repeated calls start from scratch
    // instead of compounding (and so a changed element replaces the old one).
    private static ConditionalWeakTable<Renderer, Material[]> originalMaterials = new();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        elementMaterialCache.Clear();
        originalMaterials = new();
    }

    private static Material GetGlowTemplate(Element element)
    {
        if (GlowMaterials == null || GlowMaterials.Length != 4)
            throw new Exception("There are no glow materials to be assigned to minions.");
        switch (element)
        {
            case Element.Light: case Element.Electricity: case Element.Radiation: return GlowMaterials[0];
            case Element.Heat: case Element.Psychic: return GlowMaterials[1];
            case Element.Cold: case Element.Water: case Element.Acid: return GlowMaterials[2];
            default: return GlowMaterials[3];
        }
    }

    /// <param name="variant">-1 for the element's base color, 0..3 for a gradient sample.</param>
    private static Material GetElementMaterial(Element element, int variant = -1)
    {
        if (elementMaterialCache.TryGetValue((element, variant), out Material cached) && cached != null) return cached;

        Color color = GetColorFromElementDict(element);
        Gradient gradient = colorGradientLibrary != null ? colorGradientLibrary.GetGradientByElement(element) : null;
        if (variant >= 0 && gradient != null) color = gradient.Evaluate(variant / (float)(GradientSamples - 1));

        // Named "ElementalIndicator..." so the name-based slot detection below still finds it on later calls.
        Material mat = new(GetGlowTemplate(element)) { name = $"ElementalIndicator_{element}_{variant}" };
        SetMaterialColor(mat, color);
        elementMaterialCache[(element, variant)] = mat;
        return mat;
    }

    private static Material[] GetOriginalMaterials(Renderer renderer)
    {
        if (!originalMaterials.TryGetValue(renderer, out Material[] original))
        {
            original = renderer.sharedMaterials;
            originalMaterials.Add(renderer, original);
        }
        return original;
    }
    #endregion

    /// <summary>Shared, cached glow material for an element. Do not modify the returned material.</summary>
    public static Material GetGlowStrengthByElement(Element element) => GetElementMaterial(element);

    public static void ChangeAllMatsByAffinity(LivingBeing livingBeing)
    {
        Element element = livingBeing.GetHighestAffinity(out float value);

        Renderer meshRenderer = GetBodyRenderer(livingBeing);
        if (meshRenderer == null) throw new Exception($"No renderer found for {livingBeing.Name}");

        Material[] mats = (Material[])GetOriginalMaterials(meshRenderer).Clone();
        if (element != Element.None && value >= AlchemyHandler.ElementThreshhold)
        {
            for (int i = 0; i < mats.Length; i++)
            {
                // Chance per slot = affinity / 200.
                if (UnityEngine.Random.Range(0, 200) <= value)
                    mats[i] = GetElementMaterial(element, UnityEngine.Random.Range(0, GradientSamples));
            }
        }
        meshRenderer.sharedMaterials = mats;
    }

    public static void ChangeElementalIndicatorByAffinity(LivingBeing livingBeing)
    {
        Element element = livingBeing.GetHighestAffinity(out float value, 0);
        if (element == Element.None) return;
        Material newMaterial = GetElementMaterial(element, UnityEngine.Random.Range(0, GradientSamples));
        foreach (SkinnedMeshRenderer meshRenderer in livingBeing.GetComponentsInChildren<SkinnedMeshRenderer>())
            ReplaceIndicatorSlots(meshRenderer, newMaterial);
    }

    public static void ChangeMatByAffinity(Renderer meshRenderer, Material material)
    {
        if (meshRenderer == null)
        {
            Debug.Log("ChangeMatByAffinity was given no renderer.");
            return;
        }
        ReplaceIndicatorSlots(meshRenderer, material);
    }

    private static void ReplaceIndicatorSlots(Renderer meshRenderer, Material material)
    {
        // sharedMaterials: reading .materials would clone every material on the renderer.
        Material[] mats = meshRenderer.sharedMaterials;
        bool changed = false;
        for (int i = 0; i < mats.Length; i++)
        {
            if (mats[i] != null && mats[i].name.StartsWith("ElementalIndicator"))
            {
                mats[i] = material;
                changed = true;
            }
        }
        if (changed) meshRenderer.sharedMaterials = mats;
    }

    private static Renderer GetBodyRenderer(LivingBeing livingBeing)
    {
        // Skip particle / trail / UI renderers that might sit earlier in the hierarchy.
        foreach (Renderer r in livingBeing.GetComponentsInChildren<Renderer>())
            if (r is SkinnedMeshRenderer || r is MeshRenderer) return r;
        return null;
    }
}
