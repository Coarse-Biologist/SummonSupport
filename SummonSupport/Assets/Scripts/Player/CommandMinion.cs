using System.Collections.Generic;
using SummonSupportEvents;
using UnityEngine;

public static class CommandMinion
{

    public static List<GameObject> SelectedMinions { private set; get; } = new List<GameObject>();
    public static List<GameObject> activeMinions { private set; get; } = new();


    public static void RemoveActiveMinions(GameObject minionObject)
    {
        if (activeMinions.Contains(minionObject))
            activeMinions.Remove(minionObject);
    }
    public static void AddActiveMinions(GameObject minionObject)
    {
        if (!activeMinions.Contains(minionObject))
            activeMinions.Add(minionObject);
    }

    public static void SetActiveMinions(List<GameObject> activeMinionsList)
    {
        activeMinions = activeMinionsList;
    }

    public static void HandleCommand(RaycastHit hit)
    {
        Vector3 loc = hit.point;
        if (activeMinions != null)
        {
            if (hit.collider.TryGetComponent(out EnemyStats targetLivingBeing))
            {
                CommandMinionToAttack(targetLivingBeing);
            }

            else if (hit.collider.TryGetComponent(out I_Interactable interactable))
            {
                SendMinionToInteract(hit.collider.gameObject.transform.position);
            }

            else
            {
                CommandMinionToGoToLoc(loc);
            }
        }

    }


    public static void SetSelectedMinion(GameObject minion)
    {
        if (minion != null)
        {
            if (!SelectedMinions.Contains(minion))
                SelectedMinions.Add(minion);
            else SelectedMinions.Remove(minion);
        }
    }

    private static void SendMinionToInteract(Vector3 loc)
    {
        foreach (GameObject minion in activeMinions)
        {
            if (minion != null)
            {

                MinionStats stats = minion.GetComponent<MinionStats>();
                AIObedienceState obedienceState = minion.GetComponent<AIObedienceState>();
                minion.GetComponent<MinionInteractionHandler>().SetCommandToInteract(true); // null minion
                obedienceState.SetCommandLoc(loc);
                stats.SetCommand(MinionCommands.GoTo);
            }
            else activeMinions.Remove(minion);
        }

    }
    public static void CommandMinionToGoToLoc(Vector3 loc)
    {
        foreach (GameObject minion in activeMinions)
        {
            if (minion == null)
            {
                activeMinions.Remove(minion);
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
        foreach (GameObject minion in activeMinions)
        {
            MinionStats stats = minion.GetComponent<MinionStats>();
            AIObedienceState obedienceState = minion.GetComponent<AIObedienceState>();
            obedienceState.SetCommandTarget(enemy);
            stats.SetCommand(MinionCommands.FocusTarget);
            //Logging.Info($"{stats.Name} is seeking {obedienceState.commandTarget} at location {obedienceState.commandLoc}");
        }
    }



}
