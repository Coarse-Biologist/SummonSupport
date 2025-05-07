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
        public static UnityEvent<GameObject> EnemyDefeated = new();

        #endregion
        public static UnityEvent<LivingBeing> minionDied = new UnityEvent<LivingBeing>();
        public static UnityEvent<AttributeType, float> SpeedAttributeChanged = new();
        public static UnityEvent<int, Ability> SlotChanged = new();
        public static UnityEvent<int> AbilityUsed = new(); // int should be the index of the ability

    }
}
