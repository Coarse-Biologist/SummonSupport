using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using Unity.VisualScripting.Antlr3.Runtime.Misc;

public class AIStateHandler : MonoBehaviour
{
    [SerializeField] public int FollowRadius = 5; // The distance a summon follows behind a player
    [SerializeField] public int DetectionRadius = 50; // the radius in which summons will recognize and try to chase or attack
    [SerializeField] public int AngleOfSight = 360;
    [SerializeField] public int InterestRadius = 80; // radius beyond which a summon will lose interest in attacking
    [SerializeField] public int AttackSpeed = 1;
    [SerializeField] public int Cowardice = 0; // At what HP percentage a summon may try to retreat
    public LayerMask targetMask { private set; get; }

    public LayerMask enemyMask { private set; get; }
    public LayerMask friendlyMask { private set; get; }
    public LayerMask belligerantMask { private set; get; }
    public LayerMask obstructionMask { private set; get; }
    public AIState currentState;
    [SerializeField] public AI_CC_State ccState;
    [SerializeField] public AIState peaceState { private set; get; }
    [SerializeField] public AIState chaseState { private set; get; }
    [SerializeField] public AIState obedienceState { private set; get; }
    [SerializeField] public LivingBeing livingBeing { private set; get; }
    [SerializeField] public MinionStats minionStats { private set; get; }
    public CreatureAbilityHandler abilityHandler { private set; get; }
    public GameObject player { protected set; get; }
    public LivingBeing playerStats { protected set; get; }
    public LivingBeing target { protected set; get; }

    public Vector2 lastSeenLoc;

    private Vector2 startLocation;


    public void Awake()
    {
        startLocation = transform.position;
        obstructionMask = LayerMask.GetMask("Obstruction");


        currentState = GetComponentInChildren<AIPeacefulState>();
        obedienceState = GetComponent<AIObedienceState>();
        livingBeing = GetComponent<LivingBeing>();
        minionStats = GetComponent<MinionStats>();
        ccState = GetComponent<AI_CC_State>();
        InvokeRepeating("RunStateMachine", 0f, .1f);
        abilityHandler = GetComponent<CreatureAbilityHandler>();
        SetMasks();
        SetTargetMask();

    }
    void Start()
    {
        player = PlayerStats.Instance.gameObject;
        playerStats = PlayerStats.Instance;
    }
    public void SetTarget(LivingBeing theTarget)
    {
        //Debug.Log($"target of {this} is now {target}");
        target = theTarget;
    }
    private void SetMasks()
    {
        belligerantMask = LayerMask.GetMask("Enemy", "Player", "Minion", "Guard");      // used for madness
        enemyMask = LayerMask.GetMask("Enemy");                                         // used by allies
        friendlyMask = LayerMask.GetMask("Player", "Minion");                           // used by enemies

    }

    public void SetTargetMask(StatusEffectType statusEffect = StatusEffectType.None)
    {
        bool charmed = statusEffect == StatusEffectType.Charmed;
        bool mad = statusEffect == StatusEffectType.Madness;
        if (mad)
        {
            if (targetMask == belligerantMask) Debug.Log("mask is belligerant mask");
            targetMask = belligerantMask;
            return;
        }
        CharacterTag tag = livingBeing.CharacterTag;
        if (tag == CharacterTag.Minion && !charmed || tag == CharacterTag.Enemy && charmed || tag == CharacterTag.Guard && !charmed) targetMask = enemyMask;
        else targetMask = friendlyMask;
        if (targetMask == friendlyMask) Debug.Log("mask is friendly mask");
        if (targetMask == enemyMask) Debug.Log("mask is enemy  mask");

    }

    private void RunStateMachine()
    {
        // Called in Awake by using "Invoke repeating"
        if (ccState != null && ccState.CCToCaster.Count != 0) SwitchToNextState(ccState);
        if (minionStats == null || minionStats.CurrentCommand == MinionCommands.None)
        {
            AIState nextState = currentState?.RunCurrentState();
            if (nextState != null)
            {
                SwitchToNextState(nextState);
            }
            ///else Debug.Log("nextState state was null");
        }
        else
        {
            SwitchToNextState(obedienceState);
            AIState nextState = obedienceState.RunCurrentState();
            if (nextState != null)
            {
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


