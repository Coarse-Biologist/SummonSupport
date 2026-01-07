using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AbilityLibrary", menuName = "Ability/Ability Library")]
public class AbilityLibrary_SO : ScriptableObject
{
    [field: SerializeField] public Ability defaultAttack;

    [System.Serializable]
    public struct ElementCategories
    {
        public Element Element;
        public List<Ability> Abilities;
    }
    [System.Serializable]
    public struct MeleeCategories
    {
        public Element Element;
        public List<Ability> Abilities; // Only melee type abilities
    }
    public MeleeCategories[] MeleeEntries;


    public ElementCategories[] ElementalEntries;

    [System.Serializable]

    public struct PhysicalCategories
    {
        public PhysicalType PhysicalType;
        public List<Ability> Abilities;
    }

    public PhysicalCategories[] PhysicalEntries;

    [System.Serializable]
    public struct PlayerAbilitiesByLevel
    {
        public int Level;
        public List<Ability> Abilities;
    }

    public PlayerAbilitiesByLevel[] abilitiesByLevelEntries;
    public Ability GetAbilityOfElementType(Element element)
    {
        Ability ability = null;
        foreach (ElementCategories category in ElementalEntries)
        {
            if (category.Element == element) ability = category.Abilities[0];
        }
        return ability;
    }
    public Ability GetAbilityOfPhysicalType(PhysicalType physicalType)
    {
        Ability ability = null;
        foreach (PhysicalCategories category in PhysicalEntries)
        {
            if (category.PhysicalType == physicalType) ability = category.Abilities[0];
        }
        return ability;
    }
    public Ability GetPhysicalAbilityOfElementType(Element element)
    {
        throw new System.Exception("This function does nothing but complain about being useless.");
    }
}

