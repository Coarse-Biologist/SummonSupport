using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Shared, quantized material variants for elemental tinting.
/// Every creature with the same (source material, element, strength step, color sample) uses the
/// same Material object, so tinting never leaks per-creature material copies and the SRP batcher
/// can still batch them.
///
/// Shader contract for <see cref="ElementalModel.TintMode.Blend"/> slots (make these in Shader Graph,
/// the reference names matter): _ElementColor (Color), _ElementBlend (Float 0..1),
/// _ElementEmission (Float). The shader decides *where* the color appears, typically by comparing a
/// mask (vertex color or texture painted in Blender) against _ElementBlend.
/// </summary>
public static class ElementalMaterialCache
{
    public static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
    public static readonly int ColorId = Shader.PropertyToID("_Color");
    public static readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor");
    public static readonly int ElementColorId = Shader.PropertyToID("_ElementColor");
    public static readonly int ElementBlendId = Shader.PropertyToID("_ElementBlend");
    public static readonly int ElementEmissionId = Shader.PropertyToID("_ElementEmission");

    private readonly struct Key : System.IEquatable<Key>
    {
        private readonly int source, profile, step, color;
        private readonly ElementalModel.TintMode mode;

        public Key(Material source, ElementalVisualProfile profile, int step, int color, ElementalModel.TintMode mode)
        {
            this.source = source.GetInstanceID();
            this.profile = profile.GetInstanceID();
            this.step = step;
            this.color = color;
            this.mode = mode;
        }

        public bool Equals(Key o) => source == o.source && profile == o.profile && step == o.step && color == o.color && mode == o.mode;
        public override bool Equals(object obj) => obj is Key k && Equals(k);
        public override int GetHashCode() => System.HashCode.Combine(source, profile, step, color, (int)mode);
    }

    private static readonly Dictionary<Key, Material> cache = new();

    /// <param name="source">Template (Replace) or the slot's original material (Blend).</param>
    /// <param name="strength">Already quantized slot strength, 0..1.</param>
    public static Material Get(Material source, ElementalVisualProfile profile, ElementalModel.TintMode mode,
                               float strength, int steps, int colorIndex, int colorCount)
    {
        int step = Mathf.Clamp(Mathf.RoundToInt(strength * steps), 0, steps);
        Key key = new(source, profile, step, colorIndex, mode);

        // Null check covers materials destroyed by a scene unload / domain-reload-disabled play mode.
        if (cache.TryGetValue(key, out Material cached) && cached != null) return cached;

        float quantized = step / (float)steps;
        Color color = profile.GetColorSample(colorIndex, colorCount);
        float emission = Mathf.Lerp(profile.MinEmission, profile.MaxEmission, quantized);

        Material variant = new(source)
        {
            name = $"ElementalIndicator_{profile.Element}_{mode}_{step}_{colorIndex}",
            hideFlags = HideFlags.DontSave,
        };

        if (mode == ElementalModel.TintMode.Replace)
        {
            if (variant.HasProperty(BaseColorId)) variant.SetColor(BaseColorId, color);
            else if (variant.HasProperty(ColorId)) variant.SetColor(ColorId, color);
            if (variant.HasProperty(EmissionColorId))
            {
                variant.SetColor(EmissionColorId, color * emission);
                variant.EnableKeyword("_EMISSION");
                variant.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
            }
        }
        else
        {
            if (!variant.HasProperty(ElementBlendId))
                Debug.LogWarning($"Material '{source.name}' is used as a Blend tint slot but its shader has no _ElementBlend property.", source);
            variant.SetColor(ElementColorId, color);
            variant.SetFloat(ElementBlendId, quantized);
            variant.SetFloat(ElementEmissionId, emission);
        }

        cache[key] = variant;
        return variant;
    }

    /// <summary>Destroys all cached variants. Only call when no creature is using them (e.g. on returning to the menu).</summary>
    public static void Clear()
    {
        foreach (Material m in cache.Values)
            if (m != null) Object.Destroy(m);
        cache.Clear();
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics() => cache.Clear();
}
