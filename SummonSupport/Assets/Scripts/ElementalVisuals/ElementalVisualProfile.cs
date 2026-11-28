using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Everything needed to make a creature look like one element: colors, the tint material
/// and the element-specific models (tiers) that replace the base model at high affinity.
/// One asset per element per library (see <see cref="ElementalVisualLibrary"/>).
/// </summary>
[CreateAssetMenu(fileName = "ElementalVisualProfile", menuName = "Elemental Visuals/Element Profile")]
public class ElementalVisualProfile : ScriptableObject
{
    [Serializable]
    public class ModelTier
    {
        [Tooltip("Shown in the inspector and logs only.")]
        public string label = "Tier";

        [Tooltip("The model is used once the creature's affinity for this element reaches this value.")]
        [Range(0, 200)] public int minAffinity = 100;

        [Tooltip("Prefab of the model (an FBX from Blender wrapped in a prefab). Its root needs an ElementalModel component.")]
        public ElementalModel modelPrefab;

        [Tooltip("If false, the model is shown exactly as authored (no tint on top).")]
        public bool applyTint = true;
    }

    [field: SerializeField] public Element Element { get; private set; } = Element.None;

    [field: Header("Color")]
    [field: SerializeField, ColorUsage(false, false)] public Color PrimaryColor { get; private set; } = Color.white;

    [field: Tooltip("Optional. Each tint slot picks a color from this gradient (deterministically), so creatures get some variety.")]
    [field: SerializeField] public Gradient ColorVariation { get; private set; }

    [field: Tooltip("Emission multiplier at the lowest visible affinity.")]
    [field: SerializeField, Min(0)] public float MinEmission { get; private set; } = 0.25f;

    [field: Tooltip("Emission multiplier at full affinity.")]
    [field: SerializeField, Min(0)] public float MaxEmission { get; private set; } = 2f;

    [field: Header("Materials")]
    [field: Tooltip("Material copied onto 'Replace' tint slots. Should support _BaseColor/_EmissionColor (URP Lit does).")]
    [field: SerializeField] public Material TintMaterial { get; private set; }

    [field: Header("Models")]
    [Tooltip("Element-specific models, any order. The highest tier whose Min Affinity is reached (and has a prefab) wins.")]
    [SerializeField] private List<ModelTier> modelTiers = new()
    {
        new ModelTier { label = "Elemental", minAffinity = 100 },
        new ModelTier { label = "Greater Elemental", minAffinity = 150 },
    };

    public IReadOnlyList<ModelTier> ModelTiers => modelTiers;

    /// <summary>The best model tier for an affinity value, or null if the base model should be used.</summary>
    public ModelTier GetModelTier(int affinity)
    {
        ModelTier best = null;
        foreach (ModelTier tier in modelTiers)
        {
            if (tier == null || tier.modelPrefab == null || affinity < tier.minAffinity) continue;
            if (best == null || tier.minAffinity > best.minAffinity) best = tier;
        }
        return best;
    }

    /// <summary>Picks one of <paramref name="sampleCount"/> evenly spaced colors from the variation gradient.</summary>
    public Color GetColorSample(int sampleIndex, int sampleCount)
    {
        if (ColorVariation == null || sampleCount <= 1) return PrimaryColor;
        float t = sampleIndex / (float)(sampleCount - 1);
        return ColorVariation.Evaluate(t);
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (Element == Element.None) Debug.LogWarning($"{name}: ElementalVisualProfile has no element set.", this);
    }
#endif
}
