using Quest;
using UnityEngine;
using UnityEngine.Events;

namespace SummonSupportEvents
{
    public static class EventDeclarer
    {
        public static UnityEvent<LivingBeing, AttributeType> attributeChanged = new UnityEvent<LivingBeing, AttributeType>();
        public static UnityEvent<RepeatableAccomplishments> RepeatableQuestCompleted = new UnityEvent<RepeatableAccomplishments>();
        public static UnityEvent EnemyDefeated = new UnityEvent();
        public static UnityEvent<LivingBeing> minionDied = new UnityEvent<LivingBeing>();
        public static UnityEvent<AttributeType, float> SpeedAttributeChanged = new();


        //static EventDeclarer()
        //{
        //
        //}
        //public static void Initialize()
        //{
        //    Logging.Info("Event declarer is initialized");
        //}
    }
}
