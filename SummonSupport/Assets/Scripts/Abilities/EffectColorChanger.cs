using UnityEditor;
using UnityEngine;

public static class EffectColorChanger
{
    private static ParticleSystem BleedEffect;
    private static GradientLibraryAsset colorGradientLibrary;
    private static bool ready = false;

    public static void Setup(ParticleSystem bE, GradientLibraryAsset cGL)
    {
        BleedEffect = bE;
        colorGradientLibrary = cGL;
        ready = true;
    }


    public static void SetImmersiveBleedEffect(ParticleSystem particleSystem, LivingBeing livingBeing)
    {

        if (!ready || particleSystem == null) return;
        {
            if (colorGradientLibrary != null)
            {
                ChangeColor(livingBeing, particleSystem);
            }
        }
    }

    private static void ChangeColor(LivingBeing livingBeing, ParticleSystem particleSystem)
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
}
