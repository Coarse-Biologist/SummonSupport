using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Drives a creature's look from its strongest elemental affinity:
///   affinity &lt; TintStart            base model, untinted
///   TintStart .. TintFull             tint strength ramps up (colors spread over the tint slots)
///   tier minAffinity (100, 150, ...)  base model swapped for the element model of that tier
///
/// Put this on the creature root, assign a library and make sure the base model has an
/// <see cref="ElementalModel"/>. Call <see cref="Refresh"/> (or <see cref="TryRefresh"/>) whenever affinities change.
///
/// Multiplayer: the look is worked out on every machine from the creature's affinities, which are already
/// shared (minions through MinionProfileData, enemies through NetworkEnemy). Colors are seeded from the
/// NetworkObjectId, so every machine picks the same ones.
/// </summary>
[DisallowMultipleComponent]
public class ElementalAppearance : MonoBehaviour
{
    [SerializeField] private ElementalVisualLibrary library;

    [Tooltip("The model the prefab ships with. Found in children if left empty.")]
    [SerializeField] private ElementalModel baseModel;

    [Tooltip("Where element models are created. Defaults to the base model's parent.")]
    [SerializeField] private Transform modelParent;

    [Tooltip("Refresh from the LivingBeing's affinities in Start (covers creatures placed in scenes).")]
    [SerializeField] private bool refreshOnStart = true;

    public ElementalVisualLibrary Library => library;
    public ElementalModel BaseModel => baseModel;
    public ElementalModel ActiveModel { get; private set; }
    public ElementalVisualState State { get; private set; }
    public bool IsConfigured => library != null && baseModel != null;

    /// <summary>Raised after the active model changes (old, new).</summary>
    public event Action<ElementalModel, ElementalModel> ModelChanged;

    private LivingBeing being;
    private NetworkObject networkObject;
    private ElementalModel spawnedModel;
    private ElementalModel spawnedFromPrefab;
    private bool hasApplied;
    private int localSeed;
    private static readonly List<IElementalModelListener> listenerBuffer = new();

    private void Awake()
    {
        being = GetComponent<LivingBeing>();
        networkObject = GetComponent<NetworkObject>();
        if (baseModel == null) baseModel = GetComponentInChildren<ElementalModel>(true);
        if (baseModel != null && baseModel.gameObject == gameObject)
        {
            Debug.LogError($"{name}: the ElementalModel must be on the model child, not the creature root (the root would be disabled on swap).", this);
            baseModel = null;
        }
        if (modelParent == null && baseModel != null) modelParent = baseModel.transform.parent;
        ActiveModel = baseModel;
        localSeed = UnityEngine.Random.Range(int.MinValue, int.MaxValue);

        if (!IsConfigured)
            Debug.LogWarning($"{name}: ElementalAppearance needs a library and an ElementalModel on the base model.", this);
    }

    private void Start()
    {
        if (refreshOnStart) Refresh();
    }

    /// <summary>Refreshes <paramref name="being"/> if it uses this system. False means the caller should fall back to legacy coloring.</summary>
    public static bool TryRefresh(LivingBeing being)
    {
        if (being == null || !being.TryGetComponent(out ElementalAppearance appearance) || !appearance.IsConfigured) return false;
        appearance.Refresh();
        return true;
    }

    /// <summary>Same on every machine once the creature is spawned on the network; random offline.</summary>
    private int Seed => networkObject != null && networkObject.IsSpawned ? unchecked((int)networkObject.NetworkObjectId) : localSeed;

    /// <summary>Recomputes the look from the creature's current affinities.</summary>
    public void Refresh()
    {
        if (being == null || !IsConfigured) return;

        Element element = being.GetHighestAffinity(out float value);
        Apply(new ElementalVisualState(element, Mathf.Clamp(Mathf.RoundToInt(value), 0, 200), Seed));
    }

    /// <summary>Applies a state directly (used by <see cref="Refresh"/> and the preview).</summary>
    public void Apply(ElementalVisualState next)
    {
        if (!IsConfigured) return;
        if (hasApplied && next.Equals(State)) return;
        hasApplied = true;
        State = next;

        ElementalVisualProfile profile = null;
        bool elemental = next.element != Element.None
                         && next.affinity >= library.TintStartAffinity
                         && library.TryGetProfile(next.element, out profile);

        ElementalVisualProfile.ModelTier tier = elemental ? profile.GetModelTier(next.affinity) : null;
        SwapModel(tier?.modelPrefab);

        float strength = elemental && (tier == null || tier.applyTint) ? library.GetTintStrength(next.affinity) : 0f;
        ActiveModel.ApplyTint(profile, library, strength, next.seed);
    }

    private void SwapModel(ElementalModel prefab)
    {
        if (prefab == spawnedFromPrefab && (prefab != null || ActiveModel == baseModel)) return;

        ElementalModel previous = ActiveModel;

        if (spawnedModel != null)
        {
            spawnedModel.gameObject.SetActive(false); // Destroy is deferred; hide it this frame.
            Destroy(spawnedModel.gameObject);
        }
        spawnedModel = null;
        spawnedFromPrefab = null;

        if (prefab == null)
        {
            baseModel.gameObject.SetActive(true);
            ActiveModel = baseModel;
        }
        else
        {
            Transform baseTransform = baseModel.transform;
            spawnedModel = Instantiate(prefab, modelParent);
            spawnedModel.transform.SetLocalPositionAndRotation(baseTransform.localPosition, baseTransform.localRotation);
            spawnedModel.transform.localScale = baseTransform.localScale;
            spawnedModel.name = prefab.name;
            spawnedFromPrefab = prefab;
            baseModel.gameObject.SetActive(false);
            ActiveModel = spawnedModel;
        }

        CarryOverAnimator(previous, ActiveModel);
        NotifyListeners();
        ModelChanged?.Invoke(previous, ActiveModel);
    }

    private static void CarryOverAnimator(ElementalModel from, ElementalModel to)
    {
        if (from == null || to == null || from == to) return;
        Animator a = from.Animator, b = to.Animator;
        if (a == null || b == null) return;
        if (b.runtimeAnimatorController == null) b.runtimeAnimatorController = a.runtimeAnimatorController;
        b.updateMode = a.updateMode;
        b.applyRootMotion = a.applyRootMotion;
        b.enabled = a.enabled;
    }

    private void NotifyListeners()
    {
        GetComponents(listenerBuffer);
        foreach (IElementalModelListener listener in listenerBuffer)
            listener.OnElementalModelChanged(ActiveModel, ActiveModel == baseModel);
        listenerBuffer.Clear();
    }

    #region Preview
    [Header("Preview (play mode)")]
    [SerializeField] private Element previewElement = Element.Heat;
    [SerializeField, Range(0, 200)] private int previewAffinity = 100;

    [ContextMenu("Preview Element")]
    private void Preview()
    {
        if (!Application.isPlaying) { Debug.Log("Enter play mode to preview."); return; }
        Apply(new ElementalVisualState(previewElement, previewAffinity, Seed));
    }
    #endregion
}
