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

    /// <summary>The name a new character gets unless the player types one, e.g. "Heat Alchemist".</summary>
    public static string DefaultName(Element element) => element == Element.None ? "Alchemist" : $"{element} Alchemist";

    /// <summary>True if <paramref name="name"/> is the default name of some element, i.e. the player didn't choose it.</summary>
    public static bool IsDefaultName(string name)
    {
        string sanitized = SanitizeName(name);
        foreach (Element element in Enum.GetValues(typeof(Element)))
            if (sanitized == DefaultName(element)) return true;
        return false;
    }

    public override string ToString() => $"{PlayerName} ({Element})";
}
