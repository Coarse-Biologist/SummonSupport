using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AbilityLibrary", menuName = "Ability/Ability Library")]
public class AbilityLibrary_SO : ScriptableObject
{
    [System.Serializable]
    public struct ElementCategories
    {
        public Element Element;
        public List<Ability> Abilities;
    }

    public ElementCategories[] entries;

    [System.Serializable]
    public struct PlayerAbilitiesByLevel
    {
        public int Level;
        public List<Ability> Abilities;
    }

    public PlayerAbilitiesByLevel[] abilitiesByLevelEntries;
}

