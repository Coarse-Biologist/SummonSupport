using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Designer-tunable values for a new character, read by the creation steps. Assigned on the SetupManager
/// (level) and the StartMenuController (start screen). The abilities a player spawns with are the player
/// prefab's own ability list.
/// </summary>
[CreateAssetMenu(fileName = "CharacterCreationProfile", menuName = "Character Creation/Profile")]
public class CharacterCreationProfile_SO : ScriptableObject
{
    [Tooltip("Elements offered on the start screen. Leave empty to offer every element except None.")]
    [SerializeField] private List<Element> selectableElements = new();

    [Header("Starting values for the chosen element")]
    [SerializeField] private int startingKnowledge = 10;
    [SerializeField] private float startingAffinity = 10;

    [Header("Starting minions")]
    [SerializeField] private List<StarterMinionDefinition> starterMinions = new() { new StarterMinionDefinition() };

    public int StartingKnowledge => startingKnowledge;
    public float StartingAffinity => startingAffinity;
    public IReadOnlyList<StarterMinionDefinition> StarterMinions => starterMinions;

    public List<Element> GetSelectableElements()
    {
        List<Element> elements = new();
        if (selectableElements != null)
            foreach (Element element in selectableElements)
                if (element != Element.None && !elements.Contains(element)) elements.Add(element);
        if (elements.Count > 0) return elements;

        foreach (Element element in Enum.GetValues(typeof(Element)))
            if (element != Element.None) elements.Add(element);
        return elements;
    }

    /// <summary>Used when no profile asset is assigned, so the game still runs with the default values above.</summary>
    public static CharacterCreationProfile_SO CreateRuntimeDefault()
    {
        CharacterCreationProfile_SO profile = CreateInstance<CharacterCreationProfile_SO>();
        profile.name = "CharacterCreationProfile (runtime default)";
        return profile;
    }
}

[Serializable]
public class StarterMinionDefinition
{
    [SerializeField] private string displayName = "Flesh Atronach";
    [SerializeField, Min(0)] private int count = 1;
    [Tooltip("Abilities the minion knows and casts. Leave empty for the ability library's default attack (Slash).")]
    [SerializeField] private List<Ability> abilities = new();
    [SerializeField] private float spawnDistanceFromPlayer = 2f;

    public string DisplayName => displayName;
    public int Count => count;
    public IReadOnlyList<Ability> Abilities => abilities;
    public float SpawnDistanceFromPlayer => spawnDistanceFromPlayer;
}
