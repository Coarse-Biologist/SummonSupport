using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class AIObedienceState : AIState
{
    private AIStateHandler stateHandler;
    private AIChaseState chaseState;
    private AIPeacefulState peaceState;
    private Rigidbody rb;
    public MinionCommands currentCommand;
    public Vector2 commandLoc { private set; get; }
    public LivingBeing commandTarget { private set; get; }
    private MinionStats minionStats;
    //can be command + game object, command + location

    public void Awake()
    {
        stateHandler = GetComponent<AIStateHandler>();
        chaseState = GetComponent<AIChaseState>();
        peaceState = GetComponent<AIPeacefulState>();
        minionStats = GetComponent<MinionStats>();

        rb = GetComponent<Rigidbody>();
    }

    public override AIState RunCurrentState()
    {
        if (stateHandler.minionStats != null) { Debug.Log($"{stateHandler.minionStats.Name} is obeying"); }

        currentCommand = minionStats.CurrentCommand;
        States state = ObeyCommand(currentCommand);

        if (state == States.Obedience) return this;
        if (state == States.Peace) return peaceState;
        if (state == States.Chase) return chaseState;

        else return this;
    }

    private States ObeyCommand(MinionCommands command)
    {
        States state = States.Obedience;
        if (command == MinionCommands.GoTo)
        {
            state = GoToLocation();
            return state;
        }

        if (command == MinionCommands.FocusTarget && commandTarget != null)
        {
            stateHandler.SetTarget(commandTarget);
            state = GoToLocation();
        }
        else
        {
            currentCommand = MinionCommands.None;
            minionStats.SetCommand(MinionCommands.None);

            return States.Peace;
        }
        return state;


    }
    public void SetCommandLoc(Vector2 loc)
    {
        commandLoc = loc;
    }
    public void SetCommandTarget(LivingBeing target)
    {
        commandTarget = target;
    }

    private States GoToLocation()
    {
        Vector2 currentLoc = new Vector2(transform.position.x, transform.position.y);
        if (currentCommand == MinionCommands.FocusTarget) commandLoc = commandTarget.transform.position;
        Vector2 direction = commandLoc - currentLoc;
        if (direction.sqrMagnitude > 4)
        {
            rb.linearVelocity = direction * stateHandler.movementScript.MovementSpeed * 3;
            return States.Obedience;
        }
        else
        {
            minionStats.SetCommand(MinionCommands.None);
            if (currentCommand == MinionCommands.GoTo) stateHandler.SetTarget(null); // is this good?
            return States.Peace; // Is this good?...
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

