using System.Collections.Generic;
using SummonSupportEvents;
using UnityEngine;

public static class CommandMinion
{


    public static List<LivingBeing> selectedMinions { private set; get; } = new();

    public static void RemoveSelectedMinion(LivingBeing minionObject)
    {
        if (selectedMinions.Contains(minionObject))
            selectedMinions.Remove(minionObject);
    }
    public static void AddSelectedMinion(LivingBeing minionObject)
    {
        if (!selectedMinions.Contains(minionObject))
            selectedMinions.Add(minionObject);
    }


    public static void HandleCommand(RaycastHit hit)
    {
        Vector3 loc = hit.point;
        if (AlchemyHandler.Instance.activeMinions != null)
        {
            if (hit.collider.TryGetComponent(out EnemyStats targetLivingBeing))
            {
                SetupManager.Instance.DebugLocation(loc, Color.red, 2);

                CommandMinionToAttack(targetLivingBeing);
            }

            else if (hit.collider.TryGetComponent(out I_Interactable interactable))
            {
                SetupManager.Instance.DebugLocation(loc, Color.green, 2);

                SendMinionToInteract(hit.collider.gameObject.transform.position);
            }

            else
            {
                SetupManager.Instance.DebugLocation(loc, Color.black, 2);

                CommandMinionToGoToLoc(loc);
            }
        }

    }


    private static void SendMinionToInteract(Vector3 loc)
    {
        if (AlchemyHandler.Instance.activeMinions.Count > 0)
        {
            MinionStats minion = AlchemyHandler.Instance.activeMinions[0];
            {
                if (minion != null) IssueCommand(minion, MinionCommands.Interact, loc, null);
                else AlchemyHandler.Instance.activeMinions.Remove(minion);
            }

        }
    }
    public static void CommandMinionToGoToLoc(Vector3 loc)
    {
        foreach (MinionStats minion in AlchemyHandler.Instance.activeMinions)
        {
            if (minion == null)
            {
                AlchemyHandler.Instance.RemoveActiveMinion(minion);
                return;
            }
            IssueCommand(minion, MinionCommands.GoTo, loc, null);
            //Logging.Info($"{stats.Name} is going to location {obedienceState.commandLoc}");
        }
    }

    public static void CommandMinionToAttack(LivingBeing enemy)
    {
        enemy.resourceBarInterface.HighlightHealthbar(5f);
        foreach (MinionStats minion in AlchemyHandler.Instance.activeMinions)
        {
            IssueCommand(minion, MinionCommands.FocusTarget, enemy.transform.position, enemy);
            //Logging.Info($"{stats.Name} is seeking {obedienceState.commandTarget} at location {obedienceState.commandLoc}");
        }
    }

    /// <summary>Gives a command to one of the local player's minions. Online, the host runs minion AI, so it goes there.</summary>
    private static void IssueCommand(MinionStats minion, MinionCommands command, Vector3 loc, LivingBeing target)
    {
        if (NetworkSession.IsOnline && !NetworkSession.IsAuthority && minion.TryGetComponent(out NetworkEnemy networkEnemy) && networkEnemy.IsSpawned)
            networkEnemy.SendCommand(command, loc, target);
        else
            ApplyCommand(minion, command, loc, target);
    }

    /// <summary>Points a minion's AI at a location or target. Runs where the minion's AI runs.</summary>
    public static void ApplyCommand(MinionStats minion, MinionCommands command, Vector3 loc, LivingBeing target)
    {
        AIObedienceState obedienceState = minion.GetComponent<AIObedienceState>();
        switch (command)
        {
            case MinionCommands.GoTo:
                obedienceState.SetCommandLoc(loc);
                minion.SetCommand(MinionCommands.GoTo);
                break;
            case MinionCommands.FocusTarget:
                obedienceState.SetCommandTarget(target);
                minion.SetCommand(MinionCommands.FocusTarget);
                break;
            case MinionCommands.Interact:
                minion.GetComponent<MinionInteractionHandler>().SetCommandToInteract(true);
                obedienceState.SetCommandLoc(loc);
                minion.SetCommand(MinionCommands.GoTo);
                break;
        }
    }



}
