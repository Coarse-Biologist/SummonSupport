using System.Collections.Generic;
using UnityEngine;

public static class CommandMinion
{

    public static List<GameObject> SelectedMinions { private set; get; } = new List<GameObject>();
    public static List<GameObject> activeMinions { private set; get; } = new();

    public static void SetActiveMinions(List<GameObject> activeMinionsList)
    {
        activeMinions = activeMinionsList;
    }

    public static void HandleCommand(Vector2 loc)
    {

        if (SelectedMinions != null)
        {
            Collider2D[] enemyHits = Physics2D.OverlapCircleAll(loc, 1, LayerMask.GetMask("Enemy"));
            Logging.Info($"{enemyHits.Length} colliders in click area.");

            if (enemyHits.Length > 0)
            {
                GameObject enemy = enemyHits[0].gameObject;
                if (enemy.TryGetComponent<LivingBeing>(out LivingBeing targetLivingbeing))

                    CommandMinionToAttack(targetLivingbeing);
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
            if (!SelectedMinions.Contains(minion))
                SelectedMinions.Add(minion);
            else SelectedMinions.Remove(minion);

        }
        else Logging.Error("Oh, you want to control that nobody? Select an actual minion!");
    }

    private static void SendMinionToInteract(Vector2 loc)
    {
        foreach (GameObject minion in SelectedMinions)
        {
            MinionStats stats = minion.GetComponent<MinionStats>();
            AIObedienceState obedienceState = minion.GetComponent<AIObedienceState>();
            minion.GetComponent<MinionInteractionHandler>().SetCommandToInteract(true);
            obedienceState.SetCommandLoc(loc);
            stats.SetCommand(MinionCommands.GoTo);
            Logging.Info($"{stats.Name} is going to location {obedienceState.commandLoc} to interact");
        }

    }
    public static void CommandMinionToGoToLoc(Vector2 loc)
    {
        foreach (GameObject minion in SelectedMinions)
        {
            MinionStats stats = minion.GetComponent<MinionStats>();
            AIObedienceState obedienceState = minion.GetComponent<AIObedienceState>();
            obedienceState.SetCommandLoc(loc);
            stats.SetCommand(MinionCommands.GoTo);
            Logging.Info($"{stats.Name} is going to location {obedienceState.commandLoc}");
            //stats.ChangeAttribute(AttributeType.CurrentHitpoints, -10);
        }
    }

    public static void CommandMinionToAttack(LivingBeing enemy)
    {
        foreach (GameObject minion in SelectedMinions)
        {
            MinionStats stats = minion.GetComponent<MinionStats>();
            AIObedienceState obedienceState = minion.GetComponent<AIObedienceState>();
            obedienceState.SetCommandTarget(enemy);
            stats.SetCommand(MinionCommands.FocusTarget);
            Logging.Info($"{stats.Name} is seeking {obedienceState.commandTarget} at location {obedienceState.commandLoc}");
        }
    }



}
