using Quest;
using UnityEngine;
using UnityEngine.Events;

namespace SummonSupportEvents
{
    public static class EventDeclarer
    {
        public static UnityEvent<GameObject> hpChanged = new UnityEvent<GameObject>();
        public static UnityEvent<RepeatableAccomplishments> RepeatableQuestCompleted = new UnityEvent<RepeatableAccomplishments>();
        public static UnityEvent EnemyDefeated = new UnityEvent();
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
