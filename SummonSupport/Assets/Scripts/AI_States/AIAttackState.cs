using UnityEngine;

public class AIAttackState : AIState
{
    private AIStateHandler stateHandler;
    private AIChaseState chaseState;
    private AIPeacefulState peaceState;

    public void Start()
    {
        stateHandler = GetComponent<AIStateHandler>();
        chaseState = GetComponent<AIChaseState>();
        peaceState = GetComponent<AIPeacefulState>();

    }

    public override AIState RunCurrentState()
    {
        chaseState.LookAtTarget(chaseState.targetEntity);
        chaseState.Chase(chaseState.targetEntity);
        Debug.Log("I am using abilities to attack people and stuff");
        return stateHandler.chaseState;
    }
}
