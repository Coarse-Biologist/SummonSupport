using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;

public class AIStateHandler : MonoBehaviour
{
    [SerializeField] public int FollowRadius = 5; // The distance a summon follows behind a player
    [SerializeField] public int DetectionRadius = 50; // the radius in which summons will recognize and try to chase or attack
    [SerializeField] public int AngleOfSight = 360;
    [SerializeField] public int InterestRadius = 80; // radius beyond which a summon will lose interest in attacking
    [SerializeField] public int AttackSpeed = 1;
    [SerializeField] public int Cowardice = 0; // At what HP percentage a summon may try to retreat
    public LayerMask targetMask { private set; get; }
    public LayerMask obstructionMask { private set; get; }
    public AIState currentState;
    [SerializeField] public AI_CC_State ccState;
    [SerializeField] public AIState peaceState { private set; get; }
    [SerializeField] public AIState chaseState { private set; get; }
    [SerializeField] public AIState obedienceState { private set; get; }
    [SerializeField] public LivingBeing livingBeing { private set; get; }
    [SerializeField] public MinionStats minionStats { private set; get; }
    [SerializeField] public GameObject player { protected set; get; }

    public Vector2 lastSeenLoc;

    private Vector2 startLocation;


    public void Awake()
    {
        startLocation = transform.position;
        obstructionMask = LayerMask.GetMask("Obstruction");
        if (gameObject.CompareTag("Minion")) targetMask = LayerMask.GetMask("Enemy");
        else targetMask = LayerMask.GetMask("Minion", "Player");

        currentState = GetComponentInChildren<AIPeacefulState>();
        obedienceState = GetComponent<AIObedienceState>();
        livingBeing = GetComponent<LivingBeing>();
        minionStats = GetComponent<MinionStats>();
        ccState = GetComponent<AI_CC_State>();
        InvokeRepeating("RunStateMachine", 0f, .1f);
    }



    private void RunStateMachine()
    {
        // Called in Awake by using "Invoke repeating"
        if (ccState != null && ccState.currentCCs.Count != 0) SwitchToNextState(ccState);
        if (minionStats == null || minionStats.CurrentCommand == MinionCommands.None)
        {
            AIState nextState = currentState?.RunCurrentState();
            //Debug.Log($"current state = {currentState}. next state = {nextState}");
            if (nextState != null)
            {
                SwitchToNextState(nextState);
            }
            else Debug.Log("nextState state was null");
        }
        else
        {
            //Logging.Info($"Minion has a command to follow");
            SwitchToNextState(obedienceState);
            AIState nextState = obedienceState.RunCurrentState();
            if (nextState != null)
            {
                SwitchToNextState(nextState);
            }
            else Debug.Log("next state was null");

        }
    }

    private void SwitchToNextState(AIState nextState)
    {
        currentState = nextState;
    }

}


