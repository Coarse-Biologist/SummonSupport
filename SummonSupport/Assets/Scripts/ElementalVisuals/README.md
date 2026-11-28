# Elemental Visuals

Changes a creature's model and materials based on its strongest elemental affinity.

| Affinity | Result (default numbers, all editable) |
|---|---|
| < 25 | Base model, no tint |
| 25 – 200 | Tint strength ramps from 1/8 to full. More tint slots show, emission grows |
| ≥ 100 | Base model swapped for the element's tier-1 model (tint keeps ramping on it) |
| ≥ 150 | Tier-2 model, if one exists. Otherwise tier 1 stays |

## Pieces

| Type | What it is |
|---|---|
| `ElementalVisualLibrary` (asset) | One per creature body type (minion, wolf, ...). Holds the affinity curve and one profile per element |
| `ElementalVisualProfile` (asset) | One per element: colors, tint material, emission range, model tiers |
| `ElementalModel` (component) | On the root of every model (the base model child and every Blender model prefab). Lists tint slots, Animator and ragdoll parts |
| `ElementalAppearance` (component) | On the creature root. Reads affinity, picks the model, applies the tint |
| `ElementalMaterialCache` | Shared material variants, quantized so creatures share materials. No per-creature copies |

The look depends only on `ElementalVisualState` (element, affinity, seed). Multiplayer needs no extra
component:
- Minions share their affinities through `MinionProfileData`. `ApplyTo` refreshes the look on every machine.
- Enemies share their strongest affinity through `NetworkEnemy`. The host sets it at spawn, and clients apply it
  and refresh.
- The color seed is the `NetworkObjectId`, so every machine picks the same colors.

## Suggested asset layout

```
Assets/Art/Elementals/
  Minion/                       <- one folder per body type
    MinionVisuals.asset         <- ElementalVisualLibrary
    Heat/
      Heat_Profile.asset        <- ElementalVisualProfile
      Heat_Tier1.fbx / .prefab  <- prefab root has ElementalModel
      Heat_Tier2.fbx / .prefab
      Heat_Tint.mat
    Cold/ ...
```

## Setting up a creature prefab

1. Select the base model child (the FBX instance under `Minion3D`) and add **ElementalModel**.
   Right-click the component header, then:
   - **Auto-Detect Tint Slots**
   - **Collect Animator And Ragdoll Parts**
2. On the creature root, add **ElementalAppearance** and assign the library.
3. Play, then right-click ElementalAppearance and choose **Preview Element** to test any element and affinity.

Prefabs without `ElementalAppearance` keep using the old `ColorChanger` path. You can migrate one prefab at a time.

## Making models in Blender

- **Rig:** reuse the base minion armature (same bone names) so the existing Animator Controller and
  Avatar work. If a model has no controller, it inherits the base model's controller at swap time.
- **Material slot names** drive Auto-Detect:
  - `ElementalIndicator...`: shown from the first visible affinity (25).
  - `Elemental_0`, `Elemental_1`, ... `Elemental_N`: revealed in order as affinity grows
    (0 first, N last). Example: four slots reveal at 0%, 25%, 50% and 75% tint strength.
  - `ElementalBlend...`: keeps its own textures. A shader spreads the element color over it (see below).
  - Anything else is never touched.
- **Import:** wrap each FBX in a prefab, add `ElementalModel` on the prefab root and run both context menus.
  Then drag the prefab into a tier slot on the element's profile.

## Smooth "spreading" tint (optional, Blend slots)

Replace slots switch whole material slots on. For a gradual spread across one surface, make a Shader Graph with these
properties (reference names matter):

| Reference | Type | Set by code |
|---|---|---|
| `_ElementColor` | Color | Element color sample |
| `_ElementBlend` | Float 0..1 | Tint strength for this slot |
| `_ElementEmission` | Float | Emission multiplier |

In Blender, paint a grayscale mask: vertex color, or a texture in a spare channel. Dark areas should turn first.
In the graph, use `step(mask, _ElementBlend)` (or `smoothstep` for a soft edge) to lerp between the base albedo and
`_ElementColor`, and add `_ElementColor * _ElementEmission * thatMask` to emission.

## Tuning

- `StrengthSteps` (default 8): how finely tint strength is quantized. More steps means smoother but more materials.
  At most (steps × color samples) variants are cached per element per source material.
- `StrengthCurve` reshapes the 25→200 ramp, for example to make early affinity show more strongly.
- Tier thresholds are per profile, so an element can have its tier 1 model at 90 and tier 2 at 175 if needed.
- `applyTint = false` on a tier shows that model exactly as authored.

## Calling it from code

```csharp
// After changing affinities (crafting, upgrading, loading, enemy spawning):
if (!ElementalAppearance.TryRefresh(livingBeing))
    ColorChanger.ChangeAllMatsByAffinity(livingBeing); // legacy fallback
```

Components that cache the Animator or bones should implement `IElementalModelListener`.
`AnimationControllerScript` and `Ragdoll` already do.
