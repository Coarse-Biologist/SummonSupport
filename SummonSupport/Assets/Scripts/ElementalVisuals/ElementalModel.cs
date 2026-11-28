using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

/// <summary>
/// Put on the root of every creature model (the base model inside the creature prefab and every
/// element model prefab made in Blender). Describes which material slots react to elemental tint and
/// which Animator / ragdoll parts belong to the model, so the model can be swapped at runtime.
///
/// Blender naming convention picked up by "Auto-Detect Tint Slots":
///   ElementalIndicator*      Replace slot, shown from the first visible affinity.
///   Elemental_0 .. _N        Replace slots revealed in order as affinity grows (0 first).
///   ElementalBlend*          Blend slot: keeps its own material, the shader spreads the element color.
/// </summary>
[DisallowMultipleComponent]
public class ElementalModel : MonoBehaviour
{
    public enum TintMode
    {
        /// <summary>Slot material is swapped for the element's tint material once revealed.</summary>
        Replace,
        /// <summary>Slot keeps its material; _ElementColor/_ElementBlend drive a shader-side blend.</summary>
        Blend,
    }

    [Serializable]
    public struct TintSlot
    {
        public Renderer renderer;
        [Min(0)] public int materialIndex;
        public TintMode mode;
        [Tooltip("Tint strength (0..1) at which this slot starts showing the element.")]
        [Range(0, 1)] public float revealAt;
    }

    [field: SerializeField] public Animator Animator { get; private set; }
    [SerializeField] private List<TintSlot> tintSlots = new();

    [Header("Ragdoll (optional)")]
    [SerializeField] private List<Rigidbody> ragdollBodies = new();
    [SerializeField] private List<Collider> ragdollColliders = new();

    public IReadOnlyList<Rigidbody> RagdollBodies => ragdollBodies;
    public IReadOnlyList<Collider> RagdollColliders => ragdollColliders;
    public IReadOnlyList<TintSlot> TintSlots => tintSlots;

    private class RendererState
    {
        public Renderer renderer;
        public Material[] original;
        public Material[] working;
    }

    private readonly Dictionary<Renderer, RendererState> renderers = new();
    private bool initialized;
    private bool tinted;

    private void Awake() => Initialize();

    private void Initialize()
    {
        if (initialized) return;
        initialized = true;
        if (Animator == null) Animator = GetComponentInChildren<Animator>(true);

        foreach (TintSlot slot in tintSlots)
        {
            if (slot.renderer == null || renderers.ContainsKey(slot.renderer)) continue;
            Material[] original = slot.renderer.sharedMaterials;
            renderers.Add(slot.renderer, new RendererState
            {
                renderer = slot.renderer,
                original = original,
                working = (Material[])original.Clone(),
            });
        }
    }

    /// <summary>
    /// Tints the model. Deterministic for a given (profile, strength, seed), so every client that
    /// receives the same replicated state renders the same thing.
    /// </summary>
    public void ApplyTint(ElementalVisualProfile profile, ElementalVisualLibrary library, float strength, int seed)
    {
        Initialize();
        if (profile == null || library == null || strength <= 0f)
        {
            ClearTint();
            return;
        }

        foreach (RendererState state in renderers.Values)
            Array.Copy(state.original, state.working, state.original.Length);

        int steps = library.StrengthSteps;
        for (int i = 0; i < tintSlots.Count; i++)
        {
            TintSlot slot = tintSlots[i];
            if (slot.renderer == null || !renderers.TryGetValue(slot.renderer, out RendererState state)) continue;
            if (slot.materialIndex >= state.working.Length) continue;
            if (strength < slot.revealAt) continue;

            Material source = slot.mode == TintMode.Replace ? profile.TintMaterial : state.original[slot.materialIndex];
            if (source == null) continue;

            // Blend slots ramp from their reveal point to full; Replace slots glow with the overall strength.
            float slotStrength = slot.mode == TintMode.Blend
                ? Mathf.InverseLerp(slot.revealAt, 1f, strength)
                : strength;
            slotStrength = Mathf.Max(1, Mathf.CeilToInt(slotStrength * steps)) / (float)steps;

            int colorIndex = PickColorIndex(seed, i, library.ColorSamples);
            state.working[slot.materialIndex] = ElementalMaterialCache.Get(
                source, profile, slot.mode, slotStrength, steps, colorIndex, library.ColorSamples);
        }

        foreach (RendererState state in renderers.Values)
            state.renderer.sharedMaterials = state.working;
        tinted = true;
    }

    public void ClearTint()
    {
        Initialize();
        if (!tinted) return;
        foreach (RendererState state in renderers.Values)
            if (state.renderer != null) state.renderer.sharedMaterials = state.original;
        tinted = false;
    }

    private static int PickColorIndex(int seed, int slotIndex, int count)
    {
        if (count <= 1) return 0;
        unchecked
        {
            uint h = (uint)seed * 2654435761u ^ (uint)(slotIndex + 1) * 2246822519u;
            h ^= h >> 15; h *= 2246822519u; h ^= h >> 13;
            return (int)(h % (uint)count);
        }
    }

    #region Editor helpers
    private static readonly Regex OrderedSlot = new(@"^Elemental_(\d+)", RegexOptions.IgnoreCase);

    [ContextMenu("Auto-Detect Tint Slots")]
    private void AutoDetectTintSlots()
    {
        List<(Renderer r, int index, int order)> ordered = new();
        tintSlots.Clear();

        foreach (Renderer r in GetComponentsInChildren<Renderer>(true))
        {
            if (r is ParticleSystemRenderer || r is TrailRenderer || r is LineRenderer) continue;
            Material[] mats = r.sharedMaterials;
            for (int i = 0; i < mats.Length; i++)
            {
                if (mats[i] == null) continue;
                string n = mats[i].name;
                if (n.StartsWith("ElementalIndicator", StringComparison.OrdinalIgnoreCase))
                    tintSlots.Add(new TintSlot { renderer = r, materialIndex = i, mode = TintMode.Replace, revealAt = 0f });
                else if (n.StartsWith("ElementalBlend", StringComparison.OrdinalIgnoreCase))
                    tintSlots.Add(new TintSlot { renderer = r, materialIndex = i, mode = TintMode.Blend, revealAt = 0f });
                else if (OrderedSlot.Match(n) is { Success: true } m)
                    ordered.Add((r, i, int.Parse(m.Groups[1].Value)));
            }
        }

        int maxOrder = 0;
        foreach (var o in ordered) maxOrder = Mathf.Max(maxOrder, o.order);
        foreach (var o in ordered)
            tintSlots.Add(new TintSlot { renderer = o.r, materialIndex = o.index, mode = TintMode.Replace, revealAt = o.order / (float)(maxOrder + 1) });

        Debug.Log($"{name}: found {tintSlots.Count} tint slots.", this);
        MarkDirty();
    }

    [ContextMenu("Collect Animator And Ragdoll Parts")]
    private void CollectParts()
    {
        Animator = GetComponentInChildren<Animator>(true);
        ragdollBodies.Clear();
        ragdollColliders.Clear();
        foreach (Rigidbody rb in GetComponentsInChildren<Rigidbody>(true))
            if (rb.transform != transform) ragdollBodies.Add(rb);
        foreach (Collider c in GetComponentsInChildren<Collider>(true))
            if (c.transform != transform) ragdollColliders.Add(c);
        MarkDirty();
    }

    private void MarkDirty()
    {
#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
#endif
    }
    #endregion
}
