using UnityEngine;
using System.Collections.Generic;
using System;
[CreateAssetMenu(menuName = "Dialogues")]

public class Dialogue_SO : ScriptableObject
{

    [SerializeField] public List<npc_key<string, List<string>>> npc_keys;
    [SerializeField] public List<player_key<string, string>> player_keys;




    public List<string> GetPlayerResponses(string NPC_Words)
    {
        List<string> playerResponses = new List<string>() { "..." };

        foreach (npc_key<string, List<string>> entry in npc_keys)
        {
            if (entry.Key == NPC_Words) playerResponses = entry.Value;
        }
        return playerResponses;
    }

    public string GetNPCResponseToPlayer(string playerWords)
    {
        string NPC_Words = "...";
        foreach (player_key<string, string> entry in player_keys)
        {
            if (entry.Key == playerWords) NPC_Words = entry.Value;
        }
        return NPC_Words;
    }

    public DialogueResult GetResult(string playerChoice)
    {
        Logging.Info(playerChoice + "get result func");

        foreach (player_key<string, string> dict in player_keys)
        {
            if (playerChoice == dict.Key) return dict.result;
            else Logging.Info($"{playerChoice} is not equal to {dict.Key}");
        }
        return DialogueResult.None;
    }


}

public enum DialogueResult
{
    None,
    Quest,
    Gold,
    XP,
    Knowledge,
    Item,
    Ingredient,
    Tool,
    Trader,
}
