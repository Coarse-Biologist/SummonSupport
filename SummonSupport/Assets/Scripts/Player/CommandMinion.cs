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
            Collider2D[] enemyHits = Physics2D.OverlapCircleAll(loc, 1, LayerMask.GetMask("Enemy"));
            if (enemyHits.Length > 0)
            {
                GameObject enemy = enemyHits[0].gameObject;

                CommandMinionToAttack(enemy);
            }

            Collider2D[] interactHits = Physics2D.OverlapCircleAll(loc, 1);
            Logging.Info($"{interactHits.Length} colliders in click area.");

            if (interactHits.Length > 0)
            {
                foreach (Collider2D collider in interactHits)
                {
                    I_Interactable interactable = collider.gameObject.GetComponent<I_Interactable>();
                    if (interactable != null)
                        SendMinionToInteract(loc);

                }
            }
            else
            {
                CommandMinionToGoToLoc(loc);
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

    private static void SendMinionToInteract(Vector2 loc)
    {
        selectedMinion.GetComponent<MinionInteractionHandler>().SetCommandToInteract(true);
        obedienceState.SetCommandLoc(loc);
        stats.SetCommand(MinionCommands.GoTo);
        Logging.Info($"{stats.Name} is going to location {obedienceState.commandLoc} to interact");

    }
    public static void CommandMinionToGoToLoc(Vector2 loc)
    {
        obedienceState.SetCommandLoc(loc);
        stats.SetCommand(MinionCommands.GoTo);
        Logging.Info($"{stats.Name} is going to location {obedienceState.commandLoc}");
        stats.ChangeAttribute(AttributeType.CurrentHitpoints, -10);
    }

    public static void CommandMinionToAttack(GameObject enemy)
    {
        obedienceState.SetCommandTarget(enemy);
        stats.SetCommand(MinionCommands.FocusTarget);
        Logging.Info($"{stats.Name} is seeking {obedienceState.commandTarget} at location {obedienceState.commandLoc}");
    }



}
