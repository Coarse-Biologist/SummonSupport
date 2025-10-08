using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using UnityEditor;
using UnityEngine;

public static class EffectColorChanger
{
    private static ParticleSystem BleedEffect;
    private static GradientLibraryAsset colorGradientLibrary;
    private static bool ready = false;
    public static Dictionary<Element, float[]> ElementToColorDict = new();
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
        if (ElementToColorDict.TryGetValue(Element.Cold, out float[] floatArray)) return;
        else { Debug.Log("initializing color dict"); }
        ElementToColorDict.Add(Element.Cold, new float[4] { 0.3f, 0.7f, 1f, 1f });   // icy blue
        ElementToColorDict.Add(Element.Water, new float[4] { 0f, 0.4f, 1f, 1f });     // deep water blue
        ElementToColorDict.Add(Element.Plant, new float[4] { 0.1f, 0.6f, 0.1f, 1f }); // natural green
        ElementToColorDict.Add(Element.Bacteria, new float[4] { 0.4f, 0.6f, 0.2f, 1f }); // swampy green
        ElementToColorDict.Add(Element.Poison, new float[4] { 0.3f, 0.6f, 0.2f, 1f }); // swampy green with less blue
        ElementToColorDict.Add(Element.Virus, new float[4] { 0.2f, 0.8f, 0.4f, 1f }); // sickly neon green
        ElementToColorDict.Add(Element.Acid, new float[4] { 0.6f, 1f, 0.2f, 1f });   // toxic yellow-green
        ElementToColorDict.Add(Element.Heat, new float[4] { 1f, 0.3f, 0f, 1f });     // fiery red-orange
        ElementToColorDict.Add(Element.Radiation, new float[4] { 1f, 0.5f, 0f, 1f });     // stunning orange
        ElementToColorDict.Add(Element.Electricity, new float[4] { 1f, 1f, 0.2f, 1f });     // bright yellow
        ElementToColorDict.Add(Element.Psychic, new float[4] { 0.6f, 0.2f, 0.8f, 1f }); // deep purple
        ElementToColorDict.Add(Element.Earth, new float[4] { 0.5f, 0.3f, 0.1f, 1f }); // earthy brown
        ElementToColorDict.Add(Element.Fungi, new float[4] { 0.5f, 0.6f, 0.6f, 1f }); // sporey grey
        ElementToColorDict.Add(Element.Air, new float[4] { 0.6f, 0.6f, 0.6f, 1f }); // sporey grey
        ElementToColorDict.Add(Element.Light, new float[4] { 1f, 1f, 0.85f, 1f });    // radiant soft white-yellow
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

    public static void ChangeParticleSystemColor(LivingBeing livingBeing, ParticleSystem particleSystem)
    {
        //Debug.Log($"Trying to change color of {particleSystem} to match {livingBeing}s resistences.");
        Element strongestElement = livingBeing.GetHighestAffinity();
        if (livingBeing.Affinities[strongestElement].Get() > 50)
        {
            var colorGradient = colorGradientLibrary.GetGradientByElement(strongestElement);
            SetGradient(particleSystem, colorGradient);
        }

    }

    private static void SetGradient(ParticleSystem ps, Gradient colorGradient)
    {
        var main = ps.main;           // get a copy of the main module
        main.startColor = Color.white;
        var col = ps.colorOverLifetime;
        col.enabled = true;
        col.color = new ParticleSystem.MinMaxGradient(colorGradient);
    }

    public static float[] GetColorFromElement(Element element)
    {
        if (ElementToColorDict.TryGetValue(element, out float[] colorArray)) return colorArray;
        else throw new Exception("element not found in find color by element function");
    }
    public static void SetColor(SpriteRenderer sr, float[] rgbaValues)
    {
        float r = rgbaValues[0];
        float g = rgbaValues[1];
        float b = rgbaValues[2];
        float a = rgbaValues[3];
        sr.color = new Color(r, g, b, a);
    }
    public static Material GetGlowByElement(Element element)
    {
        if (GlowMaterials == null || GlowMaterials.Count() != 4)
            throw new Exception("There are no glow materials to be assigned to minions.");
        if (element == Element.Light || element == Element.Electricity || element == Element.Radiation)
        {
            return GlowMaterials[0];
        }
        else if (element == Element.Heat || element == Element.Psychic)
        {
            return GlowMaterials[1];
        }
        else if (element == Element.Cold || element == Element.Water || element == Element.Acid)
        {
            return GlowMaterials[2];
        }
        else
        {
            return GlowMaterials[3];
        }
    }
}

