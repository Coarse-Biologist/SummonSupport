using UnityEngine;
using static AbilityLibrary_SO;

public static class AbilityLibrary
{

    public static AbilityLibrary_SO abilityLibrary { get; private set; } = null;
    public static void SetAbilityLibrary(AbilityLibrary_SO library_SO)
    {
        abilityLibrary = library_SO;
    }
    public static Ability GetAbility(Element element)
    {
        if (abilityLibrary != null)
        {
            foreach (ElementCategories elementCategory in abilityLibrary.entries)
            {
                if (elementCategory.Element == element)
                    return elementCategory.Abilities[Random.Range(0, elementCategory.Abilities.Count)];
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
