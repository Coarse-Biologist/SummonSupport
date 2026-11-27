using System;

/// <summary>
/// The choices a player makes on the start screen for a new character.
/// To add a choice (appearance, starting class, ...): add a field here, a row in
/// <see cref="CharacterCreationMenu"/>, and a step that reads it (<see cref="ICharacterCreationStep"/>).
/// </summary>
[Serializable]
public struct CharacterCreationData
{
    public const int MaxNameLength = 24;

    public string PlayerName { get; }
    public Element Element { get; }

    public CharacterCreationData(string playerName, Element element)
    {
        PlayerName = SanitizeName(playerName);
        Element = element;
    }

    public static string SanitizeName(string name)
    {
        string trimmed = (name ?? "").Trim();
        return trimmed.Length > MaxNameLength ? trimmed.Substring(0, MaxNameLength) : trimmed;
    }

    public override string ToString() => $"{PlayerName} ({Element})";
}
