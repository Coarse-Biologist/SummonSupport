using System.Collections.Generic;
using Quest;
using UnityEngine;
using UnityEngine.Events;

namespace SummonSupportEvents
{
    public static class EventDeclarer
    {
        public static UnityEvent<LivingBeing, AttributeType> attributeChanged = new UnityEvent<LivingBeing, AttributeType>();
        public static UnityEvent maxAttributeChanged = new UnityEvent();
        public static UnityEvent<RepeatableAccomplishments> RepeatableQuestCompleted = new UnityEvent<RepeatableAccomplishments>();
        public static UnityEvent EnemyDefeated = new UnityEvent();
        public static UnityEvent<LivingBeing> minionDied = new UnityEvent<LivingBeing>();
        public static UnityEvent<AttributeType, float> SpeedAttributeChanged = new();
        public static UnityEvent<int, Ability> SlotChanged = new();
        public static UnityEvent<int> AbilityUsed = new(); // int should be the index of the ability

    }
}
