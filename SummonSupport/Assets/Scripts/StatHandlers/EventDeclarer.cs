using Quest;
using UnityEngine;
using UnityEngine.Events;

namespace SummonSupportEvents
{
    public static class EventDeclarer
    {
        public static UnityEvent<GameObject> hpChanged                                      = new UnityEvent<GameObject>();
        public static UnityEvent<RepeatableAccomplishments> RepeatableQuestCompleted        = new UnityEvent<RepeatableAccomplishments>();
        public static UnityEvent EnemyDefeated                                              = new UnityEvent();
        public static UnityEvent<GameObject> minionDied                                     = new UnityEvent<GameObject >();
        public static UnityEvent<AttributeType, float> SpeedAttributeChanged                = new ();


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
