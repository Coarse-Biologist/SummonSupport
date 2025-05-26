using System.Collections.Generic;
using Quest;
using UnityEngine;
using UnityEngine.Events;

namespace SummonSupportEvents
{
    public static class EventDeclarer
    {
        public static UnityEvent<LivingBeing, AttributeType> attributeChanged = new();
        public static UnityEvent<LivingBeing, AttributeType> regenerationChanged = new();
        public static UnityEvent<LivingBeing, AttributeType> maxAttributeChanged = new();

        #region Quests
        public static UnityEvent<Quest_SO> QuestStarted = new();
        public static UnityEvent<Quest_SO> QuestCompleted = new();
        public static UnityEvent<RepeatableAccomplishments, int> RepeatableQuestCompleted = new();
        public static UnityEvent<GameObject> EnemyDefeated = new();

        #endregion
        public static UnityEvent<LivingBeing> minionDied = new UnityEvent<LivingBeing>();
        public static UnityEvent<MinionStats> SetActiveMinion = new();
        public static UnityEvent<AttributeType, float> SpeedAttributeChanged = new();
        public static UnityEvent<int, Ability> SlotChanged = new();
        public static UnityEvent<int> AbilityUsed = new(); // int should be the index of the ability
        public static UnityEvent<Vector2> SpawnEnemies = new();

    }
}
