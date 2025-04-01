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
        bool missionAccomplished = ObeyCommand(currentCommand);
        if (missionAccomplished) return peaceState;
        else return this;
    }

    private bool ObeyCommand(MinionCommands command)
    {
        bool missionAccomplished = false;
        //if (command == MinionCommands.GoTo) //GoToLocation();

        return missionAccomplished;
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
