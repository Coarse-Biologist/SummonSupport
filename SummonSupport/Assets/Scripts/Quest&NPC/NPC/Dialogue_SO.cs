using UnityEngine;
using System.Collections.Generic;
using System;
using Quest;
[CreateAssetMenu(menuName = "Dialogues")]

public class Dialogue_SO : ScriptableObject
{

    [SerializeField] public List<npc_key> npc_keys;
    [SerializeField] public List<player_key> player_keys;
    [SerializeField] public List<npc_key> unlockableDialogueNPC;
    [SerializeField] public List<player_key> unlockableDialoguePlayer;
    [SerializeField] public Quest_SO questToUnlockDialogue;



    public List<string> GetPlayerResponses(string NPC_Words, bool dialogueUnlocked = false)
    {
        List<string> playerResponses = new List<string>() { "..." };

        foreach (npc_key entry in npc_keys)
        {
            if (entry.Key == NPC_Words) playerResponses = entry.Value;
        }
        if (dialogueUnlocked)
        {
            foreach (npc_key entry in unlockableDialogueNPC)
            {
                if (entry.Key == NPC_Words)
                    foreach (string response in entry.Value)
                        playerResponses.Add(response);
            }
        }
        return playerResponses;
    }

    public Tuple<string, AudioClip> GetNPCResponseToPlayer(string playerWords, bool dialogueUnlocked = false)
    {
        string NPC_Words = "...";
        AudioClip audioClip = null;
        foreach (player_key entry in player_keys)
        {
            if (entry.Key == playerWords)
            {
                NPC_Words = entry.Value;
                audioClip = entry.NPC_VoiceLine;
            }
        }
        if (dialogueUnlocked)
        {
            Logging.Info($"while getting NPC response to player, dialogueUnlocked was true.");
            foreach (player_key entry in unlockableDialoguePlayer)
            {
                if (entry.Key == playerWords)
                {
                    Logging.Info($"{entry.Key} matches {playerWords}");
                    Logging.Info($"NPC words being set to {entry.Value}");
                    NPC_Words = entry.Value;
                    audioClip = entry.NPC_VoiceLine;
                }
            }
        }
        else Logging.Info($"while getting NPC response to player, dialogueUnlocked was FALSE.");

        return new Tuple<string, AudioClip>(NPC_Words, audioClip);
    }


    public DialogueResult GetResult(string playerChoice)
    {
        Logging.Info(playerChoice + "get result func");

        foreach (player_key dict in player_keys)
        {
            if (playerChoice == dict.Key)
                return dict.result;

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
