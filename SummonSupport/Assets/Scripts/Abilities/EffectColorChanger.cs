using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using UnityEditor;
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

    public static void SetImmersiveBleedEffect(ParticleSystem particleSystem, LivingBeing livingBeing)
    {
        if (!ready || particleSystem == null) return;
        {
            if (colorGradientLibrary != null)
            {
                ChangeParticleSystemColor(livingBeing, particleSystem);
            }
        }
    }
    public static void ChangeObjectsParticleSystemColor(LivingBeing livingBeing, GameObject potentialParticleSystemObject)
    {
        ParticleSystem ps = potentialParticleSystemObject.GetComponentInChildren<ParticleSystem>();
        if (ps != null) ChangeParticleSystemColor(livingBeing, ps);
    }
    public static void ChangeObjectsParticleSystemColor(Element element, GameObject potentialParticleSystemObject)
    {
        ParticleSystem ps = potentialParticleSystemObject.GetComponentInChildren<ParticleSystem>();
        if (ps != null) ChangeParticleSystemColor(element, ps);
    }

    public static void ChangeParticleSystemColor(LivingBeing livingBeing, ParticleSystem particleSystem)
    {
        Element strongestElement = livingBeing.GetHighestAffinity(out float value);
        if (livingBeing.Affinities[strongestElement].Get() > 0)
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
        else throw new Exception($"element not found in find color by element function {element}");
    }
    public static Color GetRandomColorFromElementGradient(Element element)
    {
        Gradient elementalGradient = colorGradientLibrary.GetGradientByElement(element);
        return elementalGradient.Evaluate(UnityEngine.Random.value);
    }
    public static Material SetMaterialColor(Material mat, Color color)
    {
        mat.SetColor("_EmissionColor", color);

        // Make sure the shader’s emission keyword is enabled
        mat.EnableKeyword("_EMISSION");
        // float r = rgbaValues[0];
        // float g = rgbaValues[1];
        // float b = rgbaValues[2];
        // float a = rgbaValues[3];
        // mat.color = new Color(r, g, b, a);
        return mat;
    }
    public static Material GetGlowStrengthByElement(Element element)
    {
        if (GlowMaterials == null || GlowMaterials.Count() != 4)
            throw new Exception("There are no glow materials to be assigned to minions.");
        if (element == Element.Light || element == Element.Electricity || element == Element.Radiation)
        {
            return SetMaterialColor(new Material(GlowMaterials[0]), GetColorFromElementDict(element));
        }
        else if (element == Element.Heat || element == Element.Psychic)
        {
            return SetMaterialColor(new Material(GlowMaterials[1]), GetColorFromElementDict(element));
        }
        else if (element == Element.Cold || element == Element.Water || element == Element.Acid)
        {
            return SetMaterialColor(new Material(GlowMaterials[2]), GetColorFromElementDict(element));
        }
        else
        {
            return SetMaterialColor(new Material(GlowMaterials[3]), GetColorFromElementDict(element));
        }
    }
    public static void ChangeAllMatsByAffinity(LivingBeing livingBeing)
    {
        Element element = livingBeing.GetHighestAffinity(out float value);
        if (element == Element.None) return;

        Renderer meshRenderer = livingBeing.GetComponentInChildren<Renderer>();
        //SkinnedMeshRenderer skinnedMeshRenderer = livingBeing.GetComponentInChildren<SkinnedMeshRenderer>();


        if (meshRenderer != null)
        {
            Material[] mats = meshRenderer.materials;
            for (int i = 0; i < mats.Length; i++)
            {
                Material newMaterial = new(GetGlowStrengthByElement(element));
                SetMaterialColor(newMaterial, GetRandomColorFromElementGradient(element));

                mats[i] = newMaterial;
            }
            meshRenderer.materials = mats;
        }
        else
            Debug.Log($"No renderer found for {livingBeing.Name}");

    }
    public static void ChangeElementalIndicatorByAffinity(LivingBeing livingBeing)
    {
        Element element = livingBeing.GetHighestAffinity(out float value);
        if (element == Element.None) return;
        Material newMaterial = new(GetGlowStrengthByElement(element));
        SetMaterialColor(newMaterial, GetRandomColorFromElementGradient(element));
        Renderer meshRenderer = livingBeing.GetComponentInChildren<Renderer>();
        //SkinnedMeshRenderer skinnedMeshRenderer = livingBeing.GetComponentInChildren<SkinnedMeshRenderer>();


        if (meshRenderer != null)
        {
            Material[] mats = meshRenderer.materials;
            for (int i = 0; i < mats.Length; i++)
            {
                SetMaterialColor(newMaterial, GetRandomColorFromElementGradient(element));

                if (mats[i].name.StartsWith("ElementalIndicator"))  // Unity adds "(Instance)"
                {
                    Debug.Log($"Material found: {mats[i].name}, assigning material {newMaterial} which has color {newMaterial.color}");
                    mats[i] = newMaterial;
                }
            }
            meshRenderer.materials = mats;
        }
        else
            Debug.Log($"No renderer found for {livingBeing.Name}");

    }
    public static void ChangeMatByAffinity(Renderer meshRenderer, Material material)
    {

        if (meshRenderer != null)
        {
            Material[] mats = meshRenderer.materials;
            for (int i = 0; i < mats.Length; i++)
            {
                if (mats[i].name.StartsWith("ElementalIndicator"))  // Unity adds "(Instance)"
                {
                    //Debug.Log($"Material found: {mats[i].name}, assigning material {material} which has color {material.color}");
                    mats[i] = material;
                }
            }
            meshRenderer.materials = mats;
        }
        else
            Debug.Log($"No renderer found for {meshRenderer.name}");

    }
}

