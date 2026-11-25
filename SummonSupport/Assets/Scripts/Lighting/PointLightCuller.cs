using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Caps the number of live real-time point lights in URP Forward+.
///
/// Forward+ builds a per-tile light list every frame for every visible additional
/// light. In Map1 all eleven point lights are enabled at once, which makes
/// "Setup Forward Lights" cost ~2.8 ms/frame on the main thread even when nothing
/// is happening in the game. This component keeps only the lights nearest the
/// camera enabled and disables the rest, refreshing at a low frequency so the
/// light list stays small without per-frame work of its own.
///
/// It installs itself after scene load when a scene actually has enough real-time
/// point lights to be worth culling, so it needs no scene wiring.
/// </summary>
[DisallowMultipleComponent]
public sealed class PointLightCuller : MonoBehaviour
{
    private const int MinimumLightsToEnable = 5;
    private const float RescanInterval = 5f;

    [SerializeField, Tooltip("Culling origin. Defaults to the main camera, which is what actually sees the lights.")]
    private Transform target;

    [SerializeField, Min(1), Tooltip("Maximum managed point lights left enabled. Mirrors URP's Additional Lights Per Object Limit.")]
    private int maxActiveLights = 4;

    [SerializeField, Min(0.05f), Tooltip("Seconds between culling passes. Deliberately low frequency.")]
    private float refreshInterval = 0.25f;

    [SerializeField, Min(0f), Tooltip("Extra distance an enabled light keeps before a closer light replaces it. Prevents flicker.")]
    private float switchHysteresis = 2f;

    [SerializeField, Min(0f), Tooltip("Lights with a range at least this large are level fill, not props; they are never culled.")]
    private float alwaysOnRangeThreshold = 25f;

    private readonly List<Light> managedLights = new List<Light>();
    private readonly List<Light> candidates = new List<Light>();
    private Comparison<Light> byDistance;
    private float nextRefresh;
    private float nextRescan;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Install()
    {
        if (FindFirstObjectByType<PointLightCuller>() != null)
            return;

        int realtimePointLights = 0;
        foreach (Light light in FindObjectsByType<Light>(FindObjectsSortMode.None))
        {
            if (light != null && light.type == LightType.Point && light.lightmapBakeType != LightmapBakeType.Baked)
                realtimePointLights++;
        }

        // Menus and small scenes should keep their lights untouched.
        if (realtimePointLights <= MinimumLightsToEnable)
            return;

        new GameObject("Point Light Culler").AddComponent<PointLightCuller>();
    }

    private void Awake()
    {
        byDistance = (a, b) => DistanceScore(a).CompareTo(DistanceScore(b));
        ResolveTarget();
        Rescan();
    }

    private void Update()
    {
        if (target == null)
        {
            ResolveTarget();
            if (target == null)
                return;
        }

        float now = Time.unscaledTime;
        if (now >= nextRescan)
        {
            Rescan();
            nextRescan = now + RescanInterval;
        }

        if (now < nextRefresh)
            return;

        nextRefresh = now + refreshInterval;
        Cull();
    }

    private void ResolveTarget()
    {
        if (target != null)
            return;

        if (Camera.main != null)
            target = Camera.main.transform;
        else
        {
            Camera anyCamera = FindFirstObjectByType<Camera>();
            if (anyCamera != null)
                target = anyCamera.transform;
        }
    }

    /// <summary>
    /// Rebuilds the managed light list. Cheap, and only runs at load and every few seconds
    /// so lights created by abilities are picked up without scanning every frame.
    /// </summary>
    private void Rescan()
    {
        managedLights.Clear();

        foreach (Light light in FindObjectsByType<Light>(FindObjectsSortMode.None))
        {
            if (light == null || light.type != LightType.Point)
                continue;
            if (light.lightmapBakeType == LightmapBakeType.Baked)
                continue;
            if (light.range >= alwaysOnRangeThreshold)
                continue; // Broad fill light: culling it would visibly change the level.

            managedLights.Add(light);
        }
    }

    private void Cull()
    {
        candidates.Clear();
        for (int i = 0; i < managedLights.Count; i++)
        {
            if (managedLights[i] != null)
                candidates.Add(managedLights[i]);
        }

        candidates.Sort(byDistance);

        for (int i = 0; i < candidates.Count; i++)
            candidates[i].enabled = i < maxActiveLights;
    }

    private float DistanceScore(Light light)
    {
        float sqrDistance = (light.transform.position - target.position).sqrMagnitude;
        float hysteresis = switchHysteresis * switchHysteresis;
        return light.enabled ? sqrDistance - hysteresis : sqrDistance;
    }
}
