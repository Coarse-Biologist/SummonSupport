using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class AIObedienceState : AIState
{
    private AIStateHandler stateHandler;
    private AIChaseState chaseState;
    private AIPeacefulState peaceState;
    private Rigidbody2D rb;
    public MinionCommands currentCommand;
    public Vector2 commandLoc { private set; get; }
    public GameObject commandTarget { private set; get; }
    private MinionStats minionStats;
    //can be command + game object, command + location

    public void Awake()
    {
        stateHandler = GetComponent<AIStateHandler>();
        chaseState = GetComponent<AIChaseState>();
        peaceState = GetComponent<AIPeacefulState>();
        minionStats = GetComponent<MinionStats>();

        rb = GetComponent<Rigidbody2D>();
    }

    public override AIState RunCurrentState()
    {
        //Logging.Info($"Current state = obedience. command = {currentCommand}. state  = {state}");
        currentCommand = minionStats.CurrentCommand;
        States state = ObeyCommand(currentCommand);
        //Logging.Info($"Current state = {state}. command = {currentCommand}");

        if (state == States.Obedience) return this;
        if (state == States.Peace) return peaceState;
        if (state == States.Chase) return chaseState;

        else return this;
    }

    private States ObeyCommand(MinionCommands command)
    {
        //Logging.Info($"Trying to obey command ({command})");
        States state = States.Obedience;
        if (command == MinionCommands.GoTo)
        {
            state = GoToLocation();
            //Logging.Info($"REturning {state} after following the command {command}");
            return state;
        }

        if (command == MinionCommands.FocusTarget && commandTarget != null)
        {
            //Logging.Info($"Command is to focus a target and command target is not null");
            chaseState.SetTargetEntity(commandTarget);
            state = GoToLocation();
            //minionStats.SetCommand(MinionCommands.None);
            //commandTarget = null;
        }
        else
        {
            //Logging.Info($"Command {command} and command target {commandTarget}");
            currentCommand = MinionCommands.None;
            minionStats.SetCommand(MinionCommands.None);

            return States.Peace;
        }
        //Logging.Info($"Returning state : {state}");
        return state;


    }
    public void SetCommandLoc(Vector2 loc)
    {
        commandLoc = loc;
    }
    public void SetCommandTarget(GameObject target)
    {
        commandTarget = target;
    }

    private States GoToLocation()
    {
        //Logging.Info($"Going to location {commandLoc}!!!!!");
        Vector2 currentLoc = new Vector2(transform.position.x, transform.position.y);
        if (currentCommand == MinionCommands.FocusTarget) commandLoc = commandTarget.transform.position;
        Vector2 direction = commandLoc - currentLoc;
        if (direction.sqrMagnitude > 4)
        {
            //Logging.Info($"Squaremagnitude of distance to target is still pretty far away i'll keep moving");
            rb.linearVelocity = direction * stateHandler.livingBeing.Speed;
            return States.Obedience;
        }
        else
        {
            //Logging.Info($"Square magnitude of distance to target is Close enough! ");
            minionStats.SetCommand(MinionCommands.None);
            if (currentCommand == MinionCommands.GoTo) chaseState.SetTargetEntity(null); // is this good?
            //commandLoc = new Vector2(0, 0);
            return States.Chase; // Is this good?...
        }

    }
    private enum States
    {
        None,
        Obedience,
        Peace,
        Chase,
    }

}



//if (peaceState.FieldOfViewCheck() == false) // no target in sight
//{
//    Vector2 targetShadow = stateHandler.lastSeenLoc;
//    chaseState.LookAtTarget(targetShadow);
//    chaseState.Chase(targetShadow);
//    return chaseState;
//}
//else
//{
//    chaseState.LookAtTarget(stateHandler.lastSeenLoc);
//    chaseState.Chase(stateHandler.lastSeenLoc);
//    //Debug.Log("I am using abilities to attack people and stuff");
//return this;//stateHandler.chaseState;
//}
