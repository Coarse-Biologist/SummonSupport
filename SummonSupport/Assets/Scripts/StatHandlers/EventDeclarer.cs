using System.Collections.Generic;
using Quest;
using UnityEngine;
using UnityEngine.Events;

namespace SummonSupportEvents
{
    public static class EventDeclarer
    {
        public static UnityEvent<LivingBeing, AttributeType> attributeChanged = new UnityEvent<LivingBeing, AttributeType>();
        public static UnityEvent<LivingBeing, AttributeType> maxAttributeChanged = new();

        #region Quests
        public static UnityEvent<Quest_SO> QuestStarted = new();
        public static UnityEvent<Quest_SO> QuestCompleted = new();
        public static UnityEvent<RepeatableAccomplishments, int> RepeatableQuestCompleted = new UnityEvent<RepeatableAccomplishments, int>();
        public static UnityEvent<EnemyStats> EnemyDefeated = new();

        #endregion
        public static UnityEvent<GameObject> minionDied = new UnityEvent<GameObject>();
        public static UnityEvent<MinionStats> SetActiveMinion = new();
        public static UnityEvent<MovementAttributes, float> SpeedAttributeChanged = new();
        public static UnityEvent<int, Ability> SlotChanged = new();
        public static UnityEvent<int> AbilityUsed = new(); // int should be the index of the ability
        public static UnityEvent<GameObject> SpawnEnemies = new();

        public static UnityEvent<bool> PlayerDead = new();
        public static UnityEvent<Ability> PlayerLearnedAbility = new();


    }
}
