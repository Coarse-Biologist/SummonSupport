using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AIStateHandler : MonoBehaviour
{
    [SerializeField] public int FollowRadius = 15; // The distance a summon follows behind a player
    [SerializeField] public int DetectionRadius = 50; // the radius in which summons will recognize and try to chase or attack
    [SerializeField] public int AngleOfSight = 360;
    [SerializeField] public int InterestRadius = 80; // radius beyond which a summon will lose interest in attacking
    [SerializeField] public int AttackSpeed = 1;
    [SerializeField] public int Cowardice = 0; // At what HP percentage a summon may try to retreat
    public LayerMask targetMask { private set; get; }
    public LayerMask obstructionMask { private set; get; }
    private AIState currentState;
    [SerializeField] public AIState peaceState { private set; get; }
    [SerializeField] public AIState chaseState { private set; get; }
    [SerializeField] public AIState attackState { private set; get; }
    [SerializeField] public LivingBeing livingBeing { private set; get; }

    public Vector2 lastSeenLoc;

    private Vector2 startLocation;

    public void Awake()
    {
        startLocation = transform.position;
        obstructionMask = LayerMask.GetMask("Obstruction");
        if (gameObject.CompareTag("Minion")) targetMask = LayerMask.GetMask("Enemy");
        else targetMask = LayerMask.GetMask("Summon", "Player");
        currentState = GetComponentInChildren<AIPeacefulState>();
        livingBeing = GetComponent<LivingBeing>();
        Debug.Log($"here is the living being script: {livingBeing}");
    }

    void Update()
    {
        RunStateMachine();
    }

    private void RunStateMachine()
    {
        AIState nextState = currentState?.RunCurrentState();
        //Debug.Log($"current state = {currentState}. next state = {nextState}");
        if (nextState != null)
        {
            SwitchToNextState(nextState);
        }
    }

    private void SwitchToNextState(AIState nextState)
    {
        currentState = nextState;
    }
}


