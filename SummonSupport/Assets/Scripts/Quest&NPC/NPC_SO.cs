using UnityEngine;
using System.Collections.Generic;
using System;

[CreateAssetMenu(menuName = "NPC")]
public class NPC_SO : ScriptableObject
{
    [SerializeField] public NPCName npc_Name = NPCName.Villager;
    [SerializeField] public string Greeting = "Oh?";
    [SerializeField] public string Goodbye = "Au revoir...";
    [SerializeField] public Dialogue_SO Dialogue;
    [SerializeField] public QuestName GivesQuest = QuestName.None;
    [SerializeField] public int XpReward = 0;
    [SerializeField] public int GoldReward = 0;
    [SerializeField] public int KnowlegdeReward = 0;
    [SerializeField] public Sprite NPC_Sprite;


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

public enum QuestName
{
    None,
    Quest1,
    Quest2,

}

