/// <summary>
/// Holds the local player's start-screen choices across the scene change into the level.
/// Written by <see cref="CharacterCreationMenu"/>; read by <see cref="CharacterCreationPipeline"/> when a new
/// character is made (a new game, or joining a game that has no saved character for this player).
/// </summary>
public static class CharacterSelection
{
    // No name keeps the player prefab's name, no element skips the element steps (e.g. starting a level in the editor).
    public static CharacterCreationData Current { get; private set; } = new("", Element.None);

    public static void Confirm(CharacterCreationData data)
    {
        Current = data;
    }
}
