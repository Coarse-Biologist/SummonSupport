using System.Collections.Generic;
using Unity.Entities.UniversalDelegates;
using Unity.VisualScripting;
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



    public static Dictionary<Element, StatusEffectType> ElementToEffectDict = new()
    {
        {Element.Acid, StatusEffectType.Dissolving},

        {Element.Air, StatusEffectType.Whirlwind},

        {Element.Bacteria, StatusEffectType.Lethargic},

        {Element.Cold, StatusEffectType.Chilled},

        {Element.Earth, StatusEffectType.KnockBack},

        {Element.Electricity, StatusEffectType.Electrified},

        {Element.Fungi, StatusEffectType.Madness},

        {Element.Heat, StatusEffectType.Overheated},

        {Element.Light, StatusEffectType.Blinded},

        {Element.Plant, StatusEffectType.Pulled},

        {Element.Poison, StatusEffectType.Poisoned},

        {Element.Psychic, StatusEffectType.Charmed},

        {Element.Radiation, StatusEffectType.Ionized},

        {Element.Virus, StatusEffectType.Infected},

        {Element.Water, StatusEffectType.Slipping},






    };
}

