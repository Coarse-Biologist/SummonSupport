using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Makes a new character from the start-screen choices: the counterpart of loading a saved one
/// (SaveHandler.ApplyCharacterWhenReady). Whoever would load the local player's save runs this instead
/// when there is no save: SetupManager offline, NetworkPlayer online.
/// Runs an ordered list of <see cref="ICharacterCreationStep"/>s; a failing step is logged and the rest still run.
/// </summary>
public static class CharacterCreationPipeline
{
    private static readonly List<ICharacterCreationStep> steps = CreateDefaultSteps();
    private static CharacterCreationProfile_SO runtimeDefault;

    /// <summary>Raised after every step has run, for systems that react to a new character.</summary>
    public static event Action<CharacterCreationContext> CharacterCreated;

    /// <summary>The order of creation. Add new steps here.</summary>
    public static List<ICharacterCreationStep> CreateDefaultSteps() => new()
    {
        new PlayerNameStep(),
        new StartingAffinityStep(),
        new StartingKnowledgeStep(),
        new StarterMinionStep(),
    };

    /// <summary>Loads the local player's saved character, or makes a new one from the start screen choices if there is none.</summary>
    public static IEnumerator LoadOrCreateWhenReady(SaveData savedCharacter) =>
        savedCharacter != null ? SaveHandler.ApplyCharacterWhenReady(savedCharacter) : CreateWhenReady(CharacterSelection.Current);

    /// <summary>Makes the local player a new character once the player and the level have started.</summary>
    public static IEnumerator CreateWhenReady(CharacterCreationData data)
    {
        // Same readiness as loading a save: the player, the level's alchemy and the player's own Start.
        while (PlayerStats.Instance == null || AlchemyHandler.Instance == null) yield return null;
        yield return null;
        Create(PlayerStats.Instance, data);
    }

    public static void Create(PlayerStats player, CharacterCreationData data)
    {
        if (player == null) return;

        CharacterCreationContext context = new(player, data, ResolveProfile());
        Debug.Log($"[CharacterCreation] creating {data}.");
        foreach (ICharacterCreationStep step in steps)
        {
            try
            {
                step.Apply(context);
            }
            catch (Exception e)
            {
                Debug.LogError($"[CharacterCreation] {step.GetType().Name} failed: {e}");
            }
        }
        CharacterCreated?.Invoke(context);
    }

    private static CharacterCreationProfile_SO ResolveProfile()
    {
        if (SetupManager.Instance != null && SetupManager.Instance.CharacterCreationProfile != null)
            return SetupManager.Instance.CharacterCreationProfile;

        Debug.LogWarning("[CharacterCreation] The SetupManager has no CharacterCreationProfile; using the default values.");
        if (runtimeDefault == null) runtimeDefault = CharacterCreationProfile_SO.CreateRuntimeDefault();
        return runtimeDefault;
    }
}
