using System;
using System.Collections.Generic;
using UnityEngine;
using static AbilityLibrary_SO;

public static class AbilityLibrary
{

    public static AbilityLibrary_SO abilityLibrary { get; private set; } = null;
    public static PlayerAbilitiesByLevel playerAbilitiesByLevel { get; private set; } = new();
    [field: SerializeField] public static StatusEffectsLibrary StatusEffectsLibrary { get; private set; }



    public static void SetAbilityLibrary(AbilityLibrary_SO library_SO)
    {
        abilityLibrary = library_SO;
    }
    public static StatusEffectsLibrary GetStatusEffectLibrary()
    {
        if (StatusEffectsLibrary != null) return StatusEffectsLibrary;
        else return null;
    }
    public static void SetStatusEffectLibrary(StatusEffectsLibrary library)
    {
        if (library != null) StatusEffectsLibrary = library;
        else throw new System.Exception("No status effect library asset set by the asset manager.");
    }

    public static List<Ability> GetRandomAbilities(Element element, int number = 1)
    {
        //Debug.Log($"will be attempting to retrieve {number} abilities available for {element}");

        List<Ability> abilitiesList = new();
        Ability randomAbility = null;
        int maxAttempts = 10;
        if (abilityLibrary != null)
        {
            foreach (ElementCategories elementCategory in abilityLibrary.ElementalEntries)
            {
                if (elementCategory.Element == element)
                {
                    number = Math.Min(number, elementCategory.Abilities.Count);
                    //Debug.Log($"will be attempting to retrieve {number} abilities. {elementCategory.Abilities.Count} are available for {element}");
                    for (int i = number; i > 0; i--)
                    {
                        maxAttempts--;
                        randomAbility = elementCategory.Abilities[UnityEngine.Random.Range(0, elementCategory.Abilities.Count)];
                        if (!abilitiesList.Contains(randomAbility))
                        {
                            //Debug.Log($"Adding {randomAbility}");
                            abilitiesList.Add(randomAbility);
                        }
                        if (maxAttempts == 0 || abilitiesList.Count == number)
                        {
                            //Debug.Log($"breaking because max attempts reached");
                            break;
                        }
                    }
                    //Debug.Log("returning abilities list");

                    return abilitiesList;
                }
            }
            return null;
        }
        else
        {
            Debug.Log($"ability handler scriptable object is not placed in the Ability library.");
            return null;
        }
    }
    public static Ability GetElementalMeleeAbility(Element element, float affinityValue)
    {
        Ability meleeAbility = null;
        foreach (MeleeCategories meleeCategory in abilityLibrary.MeleeEntries)
        {
            if (meleeCategory.Element == element)
            {
                meleeAbility = meleeCategory.Abilities[0];
                break;
            }
        }
        Debug.Log($"Returning {meleeAbility.Name} in the get Elemental Melee ability function");
        return meleeAbility;
    }
    public static List<Ability> GetRandomAbilities(PhysicalType physical, int number = 1)
    {
        Debug.Log($"will be attempting to retrieve {number} abilities available for {physical}");

        List<Ability> abilitiesList = new();
        Ability randomAbility = null;
        int maxAttempts = 10;
        if (abilityLibrary != null)
        {
            foreach (PhysicalCategories physicalCategories in abilityLibrary.PhysicalEntries)
            {
                if (physicalCategories.PhysicalType == physical)
                {
                    number = Math.Min(number, physicalCategories.Abilities.Count);
                    Debug.Log($"will be attempting to retrieve {number} abilities. {physicalCategories.Abilities.Count} are available for {physical}");
                    for (int i = number; i > 0; i--)
                    {
                        maxAttempts--;
                        randomAbility = physicalCategories.Abilities[UnityEngine.Random.Range(0, physicalCategories.Abilities.Count)];
                        if (!abilitiesList.Contains(randomAbility))
                        {
                            Debug.Log($"Adding {randomAbility}");
                            abilitiesList.Add(randomAbility);
                        }
                        if (maxAttempts == 0 || abilitiesList.Count == number)
                        {
                            Debug.Log($"breaking because max attempts reached or because all possible abilities are added");
                            break;
                        }
                    }
                    Debug.Log("returning abilities list");
                    if (abilitiesList.Count == 0) abilitiesList.Add(abilityLibrary.defaultAttack);
                    return abilitiesList;
                }
            }
            return null;
        }
        else
        {
            Debug.Log($"ability handler scriptable object is not placed in the Ability library.");
            return null;
        }
    }


}
