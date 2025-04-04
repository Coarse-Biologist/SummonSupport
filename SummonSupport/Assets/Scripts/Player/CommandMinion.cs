using System;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public static class CommandMinion
{

    public static GameObject selectedMinion { private set; get; } = null;
    public static MinionStats stats { private set; get; } = null;
    public static AIObedienceState obedienceState { private set; get; } = null;
    public static void HandleCommand(Vector2 loc)
    {
        if (selectedMinion != null)
        {
            Collider2D[] hits = Physics2D.OverlapCircleAll(loc, 1, LayerMask.GetMask("Enemy"));
            if (hits.Length > 0)
            {
                Debug.DrawLine(new Vector3(0, 0, 0), loc, Color.red, 2);
                obedienceState.SetCommandTarget(hits[0].gameObject);
                stats.SetCommand(MinionCommands.FocusTarget);
                Logging.Info($"{stats.Name} is seeking {obedienceState.commandTarget} at location {obedienceState.commandLoc}");
            }
            else
            {
                obedienceState.SetCommandLoc(loc);
                Debug.DrawLine(new Vector3(0, 0, 0), loc, Color.red, 2);
                stats.SetCommand(MinionCommands.GoTo);
                Logging.Info($"{stats.Name} is going to location {obedienceState.commandLoc}");
            }
        }

        else Logging.Error("You talking to yourself, Bud? no minion is selected!");
    }

    public static void SetSelectedMinion(GameObject minion)
    {
        if (minion != null)
        {
            selectedMinion = minion;
            stats = selectedMinion.GetComponent<MinionStats>();
            obedienceState = selectedMinion.GetComponent<AIObedienceState>();
        }


        else Logging.Error("Oh, you want to control that nobody? Select an actual minion!");
    }



}
