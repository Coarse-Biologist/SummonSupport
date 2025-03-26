using UnityEngine;

public class AIAttackState : AIState
{
    private AIStateHandler stateHandler;
    private AIChaseState chaseState;
    private AIPeacefulState peaceState;
    public Rigidbody2D rb;

    public void Start()
    {
        stateHandler = GetComponent<AIStateHandler>();
        chaseState = GetComponent<AIChaseState>();
        peaceState = GetComponent<AIPeacefulState>();
        rb = GetComponent<Rigidbody2D>();

    }

    public override AIState RunCurrentState()
    {
        if (peaceState.FieldOfViewCheck() == false) // no target in sight
        {
            Vector2 targetShadow = stateHandler.lastSeenLoc;
            chaseState.LookAtTarget(targetShadow);
            chaseState.Chase(targetShadow);
            return chaseState;
        }
        else
        {
            chaseState.LookAtTarget(stateHandler.lastSeenLoc);
            chaseState.Chase(stateHandler.lastSeenLoc);
            Debug.Log("I am using abilities to attack people and stuff");
            return this;//stateHandler.chaseState;
        }
    }

}
