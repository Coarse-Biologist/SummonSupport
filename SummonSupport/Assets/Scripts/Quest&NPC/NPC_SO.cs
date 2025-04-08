using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "NPC")]
public class NPC_SO : ScriptableObject
{
    [SerializeField] public NPCName npc_Name = NPCName.Villager;
    [SerializeField] public Dictionary<string, string> PlayerToNPC = new Dictionary<string, string>();
    [SerializeField] public Dictionary<string, List<string>> NPCToPlayer= new Dictionary<string, List<string>>();
    [SerializeField] public QuestName GivesQuest = QuestName.None;
    [SerializeField] public int XpReward = 0;
    [SerializeField] public int GoldReward = 0;
    [SerializeField] public int KnowlegdeReward = 0;
    [SerializeField] public Sprite NPC_Sprite;


    public List<string> GetPlayerDialogueOptions(string NPCWords)
    {
        if(NPCToPlayer.TryGetValue(NPCWords, out List<string> playerResponseList)) return playerResponseList;
        else
        {Logging.Error($"The Player has nothing to say to to {NPCWords}"); //throw new Exception("Player has nothing to say".);
        return new List<string>{"...", "...."};
        }
    }
    public string GetNPCResponseToPlayer(string playerWords)
    {
        if(PlayerToNPC.TryGetValue(playerWords, out string NPCWords)) return NPCWords;
        else
        {
            Logging.Error($"The NPC has nothing to say to to {playerWords}"); //throw new Exception("Player has nothing to say".);
        return "...";
        }
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

public enum QuestName
{
    None,
    Quest1,
    Quest2,

}

