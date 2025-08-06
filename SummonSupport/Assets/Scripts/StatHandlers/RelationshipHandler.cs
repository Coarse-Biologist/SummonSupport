
using System;
using System.Collections.Generic;

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
        //Logging.Info($"Relationship =  between {target} and {owner} is {RelationshipTable[(int)owner, (int)target]}");
        return RelationshipTable[(int)owner, (int)target];
    }
}

public static class CrewsRelationshipHandler
{
    public static List<CharacterTag> Allies = new List<CharacterTag> { CharacterTag.Player, CharacterTag.Minion, CharacterTag.Guard };

    public static RelationshipType GetRelationshiptype(LivingBeing creature1, LivingBeing creature2)
    {
        bool isAlly1 = Allies.Contains(creature1.CharacterTag);
        bool isAlly2 = Allies.Contains(creature2.CharacterTag);
        if (creature1.TryGetComponent<AI_CC_State>(out AI_CC_State ccState) && ccState.isMad) return RelationshipType.Hostile;
        return (isAlly1 == isAlly2) ? RelationshipType.Friendly : RelationshipType.Hostile;
    }
}