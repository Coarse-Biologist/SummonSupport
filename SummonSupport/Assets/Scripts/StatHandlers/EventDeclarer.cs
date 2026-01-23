using System.Collections.Generic;
using Quest;
using UnityEngine;
using UnityEngine.Events;

namespace SummonSupportEvents
{
    public static class EventDeclarer
    {
        public static UnityEvent<LivingBeing, AttributeType> maxAttributeChanged = new();

        #region Quests
        public static UnityEvent<Quest_SO> QuestStarted = new();
        public static UnityEvent<Quest_SO> QuestCompleted = new();
        public static UnityEvent<RepeatableAccomplishments, int> RepeatableQuestCompleted = new UnityEvent<RepeatableAccomplishments, int>();
        public static UnityEvent<EnemyStats> EnemyDefeated = new();

        #endregion
        public static UnityEvent<GameObject> minionDied = new();
        public static UnityEvent<GameObject> minionRecycled = new();
        public static UnityEvent<LivingBeing> newMinionAdded = new();

        public static UnityEvent<MinionStats> SetActiveMinion = new();
        public static UnityEvent<MovementAttributes, float> SpeedAttributeChanged = new();
        public static UnityEvent<int, Ability> SlotChanged = new();
        public static UnityEvent<int, Ability> SetSlot = new();

        public static UnityEvent<int> AbilityUsed = new(); // int should be the index of the ability
        public static UnityEvent<DialogueAndAudio_SO> PlayerDialogue = new();
        public static UnityEvent<Ability> PlayAbilityCastSound = new();
        public static UnityEvent<Ability> PlayAbilityImpactSound = new();

        public static UnityEvent<SpawnLocationInfo> SpawnEnemies = new();

        public static UnityEvent<bool> PlayerDead = new();
        public static UnityEvent<List<string>> PlayerLevelUp = new(); // should I be passing this?
        public static UnityEvent PlayerUsingUI = new();
        public static UnityEvent PlayerFootstep = new();


        public static UnityEvent<Ability> PlayerLearnedAbility = new();
        public static UnityEvent TogglePauseGame = new();
        public static UnityEvent<float> ShakeCamera = new();

        #region status effects
        public static UnityEvent<LivingBeing> ViciousDeath = new();
        public static UnityEvent<LivingBeing> FrozenSolid = new();
        public static UnityEvent<LivingBeing> GraspingVines = new();
        public static UnityEvent<LivingBeing> SpreadVirus = new();
        public static UnityEvent<LivingBeing> IonizedAttack = new();
        public static UnityEvent<Rigidbody> PlantAttack = new();
        public static UnityEvent<LivingBeing> Overheating = new();






        #endregion
    }
}
