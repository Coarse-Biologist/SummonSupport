using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AIStateHandler : MonoBehaviour
{
    [SerializeField] public int FollowRadius = 15; // The distance a summon follows behind a player
    [SerializeField] public int DetectionRadius = 50; // the radius in which summons will recognize and try to chase or attack
    [SerializeField] public int AngleOfSight = 50;
    [SerializeField] public int InterestRadius = 80; // radius beyond which a summon will lose interest in attacking
    [SerializeField] public int AttackSpeed = 1;
    [SerializeField] public int Cowardice = 0; // At what HP percentage a summon may try to retreat
    public LayerMask targetMask { private set; get; } = LayerMask.GetMask("Enemy");
    public LayerMask obstructionMask { private set; get; } = LayerMask.GetMask("Obstruction");
    private AIState currentState;
    [SerializeField] public AIState peaceState { private set; get; }
    [SerializeField] public AIState chaseState { private set; get; }
    [SerializeField] public AIState attackState { private set; get; }

    public void Start()
    {
        if (this.gameObject.CompareTag("Minion")) targetMask = LayerMask.GetMask("Enemy");
        else targetMask = LayerMask.GetMask("Summon");
        currentState = GetComponentInChildren<AIPeacefulState>();
    }

    void Update()
    {
        RunStateMachine();
    }

    private void RunStateMachine()
    {
        AIState nextState = currentState?.RunCurrentState();

        if (nextState != null)
        {
            //switch to nextAIState
            SwitchToNextState(nextState);
        }
    }

    private void SwitchToNextState(AIState nextState)
    {
        currentState = nextState;
    }
}


