using UnityEngine;
using System.Collections.Generic;
using System;
[CreateAssetMenu(menuName = "Dialogues")]

public class Dialogue_SO : ScriptableObject
{
    public List<PlayerToNPC_SO> NPC_ResponseDict;
    public List<NPC_ToPlayer_SO> playerResponseDict;


    public List<string> GetPlayerResponses(string NPC_Words)
    {
        List<string> playerResponses = new List<string>() { "..." };

        foreach (NPC_ToPlayer_SO entry in playerResponseDict)
        {
            if (entry.key == NPC_Words) playerResponses = entry.value;
        }
        return playerResponses;
    }

    public string GetNPCResponseToPlayer(string playerWords)
    {
        string NPC_Words = "...";
        foreach (PlayerToNPC_SO entry in NPC_ResponseDict)
        {
            if (entry.key == playerWords) NPC_Words = entry.value;
        }
        return NPC_Words;
    }
}
