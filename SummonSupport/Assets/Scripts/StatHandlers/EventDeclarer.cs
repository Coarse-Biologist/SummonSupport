using Quest;
using UnityEngine.Events;

namespace SummonSupportEvents
{
    public static class EventDeclarer
    {
        public static UnityEvent<LivingBeing, AttributeType>    attributeChanged            = new();
        public static UnityEvent                                maxAttributeChanged         = new();
        public static UnityEvent<RepeatableAccomplishments>     RepeatableQuestCompleted    = new();
        public static UnityEvent                                EnemyDefeated               = new();
        public static UnityEvent<LivingBeing>                   minionDied                  = new();
        public static UnityEvent<AttributeType, float>          SpeedAttributeChanged       = new();
        public static UnityEvent<int, Ability>                  SlotChanged                 = new();
        public static UnityEvent<int>                           AbilityUsed                 = new(); // int should be the index of the ability
    }
}
