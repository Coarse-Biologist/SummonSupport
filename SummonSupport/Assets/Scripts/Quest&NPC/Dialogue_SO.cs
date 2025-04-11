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
            if (entry.player_says == playerWords) NPC_Words = entry.npc_says;
        }
        return NPC_Words;
    }
}
