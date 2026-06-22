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
        foreach (LivingBeing minion in AlchemyHandler.Instance.activeMinions)
        {
            if (minion != null)
            {

                MinionStats stats = minion.GetComponent<MinionStats>();
                AIObedienceState obedienceState = minion.GetComponent<AIObedienceState>();
                minion.GetComponent<MinionInteractionHandler>().SetCommandToInteract(true); // null minion
                obedienceState.SetCommandLoc(loc);
                stats.SetCommand(MinionCommands.GoTo);
            }
            else AlchemyHandler.Instance.activeMinions.Remove(minion);
        }

    }
    public static void CommandMinionToGoToLoc(Vector3 loc)
    {
        foreach (LivingBeing minion in AlchemyHandler.Instance.activeMinions)
        {
            if (minion == null)
            {
                AlchemyHandler.Instance.activeMinions.Remove(minion);
                return;
            }
            MinionStats stats = minion.GetComponent<MinionStats>();
            AIObedienceState obedienceState = minion.GetComponent<AIObedienceState>();
            obedienceState.SetCommandLoc(loc);
            stats.SetCommand(MinionCommands.GoTo);
            //Logging.Info($"{stats.Name} is going to location {obedienceState.commandLoc}");
            //stats.ChangeAttribute(AttributeType.CurrentHitpoints, -10);
        }
    }

    public static void CommandMinionToAttack(LivingBeing enemy)
    {
        enemy.resourceBarInterface.HighlightHealthbar(5f);
        foreach (LivingBeing minion in AlchemyHandler.Instance.activeMinions)
        {
            MinionStats stats = minion.GetComponent<MinionStats>();
            AIObedienceState obedienceState = minion.GetComponent<AIObedienceState>();
            obedienceState.SetCommandTarget(enemy);
            stats.SetCommand(MinionCommands.FocusTarget);
            //Logging.Info($"{stats.Name} is seeking {obedienceState.commandTarget} at location {obedienceState.commandLoc}");
        }
    }



}
