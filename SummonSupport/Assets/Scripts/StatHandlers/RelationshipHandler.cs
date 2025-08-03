
using System;

public static class RelationshipHandler
{
    static RelationshipType[,] RelationshipTable;

    static RelationshipHandler()
    {
        int numberTags = Enum.GetValues(typeof(CharacterTag)).Length;
        RelationshipTable = new RelationshipType[numberTags, numberTags];

        RelationshipTable[(int)CharacterTag.Player, (int)CharacterTag.Player] = RelationshipType.Friendly;
        RelationshipTable[(int)CharacterTag.Player, (int)CharacterTag.Minion] = RelationshipType.Friendly;
        RelationshipTable[(int)CharacterTag.Player, (int)CharacterTag.Guard] = RelationshipType.Friendly;
        RelationshipTable[(int)CharacterTag.Player, (int)CharacterTag.Enemy] = RelationshipType.Hostile;

        RelationshipTable[(int)CharacterTag.Minion, (int)CharacterTag.Player] = RelationshipType.Friendly;
        RelationshipTable[(int)CharacterTag.Minion, (int)CharacterTag.Minion] = RelationshipType.Friendly;
        RelationshipTable[(int)CharacterTag.Minion, (int)CharacterTag.Guard] = RelationshipType.Friendly;
        RelationshipTable[(int)CharacterTag.Minion, (int)CharacterTag.Enemy] = RelationshipType.Hostile;

        RelationshipTable[(int)CharacterTag.Guard, (int)CharacterTag.Player] = RelationshipType.Friendly;
        RelationshipTable[(int)CharacterTag.Guard, (int)CharacterTag.Minion] = RelationshipType.Friendly;
        RelationshipTable[(int)CharacterTag.Guard, (int)CharacterTag.Guard] = RelationshipType.Friendly;
        RelationshipTable[(int)CharacterTag.Guard, (int)CharacterTag.Enemy] = RelationshipType.Hostile;

        RelationshipTable[(int)CharacterTag.Enemy, (int)CharacterTag.Player] = RelationshipType.Hostile;
        RelationshipTable[(int)CharacterTag.Enemy, (int)CharacterTag.Minion] = RelationshipType.Hostile;
        RelationshipTable[(int)CharacterTag.Enemy, (int)CharacterTag.Guard] = RelationshipType.Hostile;
        RelationshipTable[(int)CharacterTag.Enemy, (int)CharacterTag.Enemy] = RelationshipType.Friendly;
    }
    public static RelationshipType GetRelationshipType(CharacterTag owner, CharacterTag target)
    {
        Logging.Info($"Relationship =  between {target} and {owner} is {RelationshipTable[(int)owner, (int)target]}");
        return RelationshipTable[(int)owner, (int)target];
    }
}