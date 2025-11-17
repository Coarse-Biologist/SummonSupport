using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine.AI;

public class AIStateHandler : MonoBehaviour
{
    [field: SerializeField] public int FollowRadius = 5; // The distance a summon follows behind a player
    [field: SerializeField] public int DetectionRadius = 50; // the radius in which summons will recognize and try to chase or attack
    [field: SerializeField] public int AngleOfSight = 360;
    [field: SerializeField] public int InterestRadius = 80; // radius beyond which a summon will lose interest in attacking
    [field: SerializeField] public int Cowardice = 0; // At what HP percentage a summon may try to retreat
    [field: SerializeField] public bool Dead = false; // At what HP percentage a summon may try to retreat


    public LayerMask targetMask { private set; get; }
    public bool StuckInAbilityAnimation { private set; get; } = false;

    public LayerMask enemyMask { private set; get; }
    public LayerMask friendlyMask { private set; get; }
    public LayerMask belligerantMask { private set; get; }
    public LayerMask obstructionMask { private set; get; }
    public NavMeshAgent navAgent { private set; get; }
    public AIState currentState { get; private set; }
    [SerializeField] public AI_CC_State ccState;
    [SerializeField] public AIState peaceState { private set; get; }
    [SerializeField] public AIState chaseState { private set; get; }
    [SerializeField] public AIState obedienceState { private set; get; }
    [SerializeField] public LivingBeing livingBeing { private set; get; }
    public MovementScript movementScript { get; private set; }
    [SerializeField] public MinionStats minionStats { private set; get; }
    public CreatureAbilityHandler abilityHandler { private set; get; }
    public GameObject player { protected set; get; }
    public LivingBeing playerStats { protected set; get; }
    public LivingBeing target { protected set; get; }
    public CharacterTag charType { protected set; get; }


    public Vector3 lastSeenLoc;

    private Vector3 startLocation;

    public void SetCurrentState(AIState state)
    {
        currentState = state;
    }
    public void SetStuckInAbilityAnimation(bool stuck)
    {
        StuckInAbilityAnimation = stuck;
    }


    public void Awake()
    {
        startLocation = transform.position;
        obstructionMask = LayerMask.GetMask("Obstruction");
        navAgent = GetComponent<NavMeshAgent>();

        currentState = GetComponentInChildren<AIPeacefulState>();
        obedienceState = GetComponent<AIObedienceState>();
        livingBeing = GetComponent<LivingBeing>();
        movementScript = GetComponent<MovementScript>();
        minionStats = GetComponent<MinionStats>();
        ccState = GetComponent<AI_CC_State>();
        InvokeRepeating("RunStateMachine", 0f, 1f);
        abilityHandler = GetComponent<CreatureAbilityHandler>();
        SetMasks();
        SetTargetMask();
        SetCharType(livingBeing.CharacterTag);

    }
    void Start()
    {
        player = PlayerStats.Instance.gameObject;
        playerStats = PlayerStats.Instance;
    }
    public void SetTarget(LivingBeing theTarget)
    {
        target = theTarget;

    }
    private void SetMasks()
    {
        belligerantMask = LayerMask.GetMask("Enemy", "Player", "Minion", "Guard");      // used for madness
        enemyMask = LayerMask.GetMask("Enemy");                                         // used by allies
        friendlyMask = LayerMask.GetMask("Player", "Minion");                           // used by enemies

    }
    private void SetCharType(CharacterTag tag)
    {
        charType = tag;
    }


    public void SetTargetMask(StatusEffectType statusEffect = StatusEffectType.None)
    {
        bool charmed = statusEffect == StatusEffectType.Charmed;
        bool mad = statusEffect == StatusEffectType.Madness;
        if (mad)
        {
            if (targetMask == belligerantMask) //Debug.Log("mask is belligerant mask");
                targetMask = belligerantMask;
            return;
        }
        CharacterTag tag = livingBeing.CharacterTag;
        if (tag == CharacterTag.Minion && !charmed || tag == CharacterTag.Enemy && charmed || tag == CharacterTag.Guard && !charmed) targetMask = enemyMask;
        else targetMask = friendlyMask;
        //if (targetMask == friendlyMask) //Debug.Log("mask is friendly mask");
        //if (targetMask == enemyMask) //Debug.Log("mask is enemy  mask");

    }

    public void SetDead(bool dead)
    {
        Dead = dead;
    }

    private void RunStateMachine()
    {
        //Debug.Log($"current state = {currentState}");
        if (Dead) return;
        // Called in Awake by using "Invoke repeating"
        if (ccState != null && ccState.CCToCaster.Count != 0) SwitchToNextState(ccState);
        if (minionStats == null || minionStats.CurrentCommand == MinionCommands.None)
        {
            AIState nextState = currentState?.RunCurrentState();
            if (nextState != null)
            {
                SwitchToNextState(nextState);
            }
            else Debug.Log("nextState state was null");
        }
        else
        {
            if (livingBeing.CharacterTag == CharacterTag.Minion)
            {
                AIState nextState = obedienceState.RunCurrentState();
                SwitchToNextState(nextState);

            }
            else
            {
                AIState nextState = peaceState.RunCurrentState();
                SwitchToNextState(nextState);
            }
            //else Debug.Log("next state was null");

        }
    }

    private void SwitchToNextState(AIState nextState)
    {
        currentState = nextState;
    }

}


