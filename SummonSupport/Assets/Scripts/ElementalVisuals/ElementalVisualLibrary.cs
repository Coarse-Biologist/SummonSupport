using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The set of element profiles for one creature archetype, plus the affinity curve that drives tinting.
/// Use one library per base body (e.g. "MinionVisuals", "WolfVisuals") so every archetype can have
/// its own Blender models while sharing the same code.
/// </summary>
[CreateAssetMenu(fileName = "ElementalVisualLibrary", menuName = "Elemental Visuals/Visual Library")]
public class ElementalVisualLibrary : ScriptableObject
{
    [field: Header("Affinity curve")]
    [field: Tooltip("Below this affinity the creature looks un-elemental.")]
    [field: SerializeField, Range(0, 200)] public int TintStartAffinity { get; private set; } = 25;

    [field: Tooltip("At or above this affinity the tint is at full strength.")]
    [field: SerializeField, Range(1, 200)] public int TintFullAffinity { get; private set; } = 200;

    [field: Tooltip("Optional shaping of tint strength between start and full (x and y in 0..1). Leave empty for linear.")]
    [field: SerializeField] public AnimationCurve StrengthCurve { get; private set; } = AnimationCurve.Linear(0, 0, 1, 1);

    [field: Header("Material variants")]
    [field: Tooltip("Tint strength is rounded to this many steps so creatures share materials (keeps batching and avoids per-creature material copies).")]
    [field: SerializeField, Range(2, 32)] public int StrengthSteps { get; private set; } = 8;

    [field: Tooltip("How many distinct colors are sampled from each profile's gradient.")]
    [field: SerializeField, Range(1, 8)] public int ColorSamples { get; private set; } = 4;

    [SerializeField] private List<ElementalVisualProfile> profiles = new();

    private Dictionary<Element, ElementalVisualProfile> lookup;

    public IReadOnlyList<ElementalVisualProfile> Profiles => profiles;

    public bool TryGetProfile(Element element, out ElementalVisualProfile profile)
    {
        if (lookup == null) BuildLookup();
        return lookup.TryGetValue(element, out profile) && profile != null;
    }

    /// <summary>0 at <see cref="TintStartAffinity"/> or below, 1 at <see cref="TintFullAffinity"/>, quantized.</summary>
    public float GetTintStrength(int affinity)
    {
        if (affinity < TintStartAffinity) return 0f;
        float t = Mathf.InverseLerp(TintStartAffinity, Mathf.Max(TintStartAffinity + 1, TintFullAffinity), affinity);
        if (StrengthCurve != null && StrengthCurve.length > 0) t = Mathf.Clamp01(StrengthCurve.Evaluate(t));
        // Ceil so that anything at or above the start affinity is visibly tinted.
        return Mathf.Max(1, Mathf.CeilToInt(t * StrengthSteps)) / (float)StrengthSteps;
    }

    private void BuildLookup()
    {
        lookup = new Dictionary<Element, ElementalVisualProfile>();
        foreach (ElementalVisualProfile profile in profiles)
        {
            if (profile == null) continue;
            if (lookup.ContainsKey(profile.Element))
            {
                Debug.LogWarning($"{name}: more than one profile for {profile.Element}; using the first.", this);
                continue;
            }
            lookup.Add(profile.Element, profile);
        }
    }

    private void OnEnable() => lookup = null;

#if UNITY_EDITOR
    private void OnValidate()
    {
        lookup = null;
        if (TintFullAffinity <= TintStartAffinity)
            Debug.LogWarning($"{name}: Tint Full Affinity should be greater than Tint Start Affinity.", this);
    }
#endif
}
