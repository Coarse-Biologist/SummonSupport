using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "AbilityLibrary", menuName = "Ability/Ability Library")]
public class AbilityLibrary_SO : ScriptableObject
{
    [field: SerializeField] public Ability defaultAttack;
    [field: SerializeField] public List<Ability> HealingAbilities = new(); // Only heal type abilities


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

    public List<Ability> GetElementalAbilitiesBelowLevel(int level, List<Element> elements, bool includeKnown = false)
    {
        List<Ability> abilities = new();
        foreach (ElementCategories category in ElementalEntries)
        {
            if (elements.Contains(category.Element))
            {

                foreach (Ability ability in category.Abilities)
                {
                    if (!includeKnown && PlayerStats.Instance.abilityHandler.Abilities.Contains(ability)) continue;
                    else if (Ability.GetCoreCraftingCost(ability) <= level * LevelUpHandler.CraftingPowerScalar) abilities.Add(ability);
                }
            }
        }
        return abilities;
    }

}

