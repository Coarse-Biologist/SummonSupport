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
    private MinionCommands currentCommand;
    private Vector2 commandLoc;
    private GameObject commandTarget;
    //can be command + game object, command + location

    public void Start()
    {
        stateHandler = GetComponent<AIStateHandler>();
        chaseState = GetComponent<AIChaseState>();
        peaceState = GetComponent<AIPeacefulState>();
        rb = GetComponent<Rigidbody2D>();
    }

    public override AIState RunCurrentState()
    {
        currentCommand = GetComponent<MinionStats>().CurrentCommand;
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
        }

        if (command == MinionCommands.FocusTarget && commandTarget != null)
        {
            chaseState.SetTargetEntity(commandTarget);
            state = States.Chase;
            commandTarget = null;
        }

        return state;
    }

    private States GoToLocation()
    {
        Vector2 currentLoc = new Vector2(transform.position.x, transform.position.y);
        Vector2 direction = commandLoc - currentLoc;
        if (direction.sqrMagnitude > 10)
        {
            rb.linearVelocity = (commandLoc - currentLoc) * stateHandler.livingBeing.Speed;
            if (direction.sqrMagnitude < 5) ;
            return States.Obedience;
        }
        else
        {
            commandLoc = new Vector2(0, 0);
            return States.Peace;
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
