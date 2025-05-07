using UnityEngine;
using System.Collections.Generic;
using System;


namespace Quest
{
    [CreateAssetMenu(menuName = "NPC")]
    public class NPC_SO : ScriptableObject
    {
        [SerializeField] public NPCName npc_Name = NPCName.Villager;
        [SerializeField] public string Greeting = "Oh?";
        [SerializeField] public string Goodbye = "Au revoir...";
        [SerializeField] public Dialogue_SO Dialogue;
        [SerializeField] public QuestName GivesQuest = QuestName.None;
        [SerializeField] public int XP_Reward = 0;
        [SerializeField] public int GoldReward = 0;
        [SerializeField] public int KnowlegdeReward = 0;
        [SerializeField] public Element ElementsLootReward = Element.None;
        [SerializeField] public AlchemyLoot AlchemyLootReward = AlchemyLoot.WretchedOrgans;
        [SerializeField] public AlchemyTool AlchemyToolReward = AlchemyTool.Beaker;


        [SerializeField] public Sprite NPC_Sprite;

        public string GetResult(DialogueResult result)
        {
            Logging.Info(result.ToString());
            switch (result)
            {
                case DialogueResult.None:
                    return "";
                case DialogueResult.Quest:
                    return $"\nYou have started the quest: {GivesQuest}";
                case DialogueResult.Gold:
                    return $"\nYou have recieved {GoldReward} gold.";
                case DialogueResult.XP:
                    return $"\nYou have recieved {XP_Reward} experience.";
                case DialogueResult.Knowledge:
                    return $"\nYou have recieved {KnowlegdeReward} {ElementsLootReward} knowledge.";
                case DialogueResult.Ingredient:
                    return $" \n You have recieved {AlchemyLootReward}";
                default:
                    return "";
            }
        }
        public void UnlockDialogueOption()
        {

        }

    }


    public enum NPCName
    {
        Villager,
        MoronicVillager,
        SnootyVillager,
        BraveVillager,
        InjuredVillager,
        Screegler,
    }
}



