using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "StatuEffectsLibraryAssets", menuName = "StatusEffectLibraryAsset")]
public class StatusEffectsLibrary : ScriptableObject
{
    [System.Serializable]
    public struct EffectEntry
    {
        public StatusEffectType Type;
        public StatusEffects Effect;
    }

    public EffectEntry[] entries;

}

