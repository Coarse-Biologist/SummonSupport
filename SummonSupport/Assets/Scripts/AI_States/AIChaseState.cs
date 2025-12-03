using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;


public class AIChaseState : AIState
{
    #region Class variables
    private AIStateHandler stateHandler;
    private AIPeacefulState peaceState;
    private AIObedienceState obedienceState;
    private CreatureAbilityHandler abilityHandler;
    private bool targetIsInRange;
    private Rigidbody rb;
    private LivingBeing livingBeing;
    [SerializeField] GameObject rotationObject;
    private Coroutine attackCoroutine;
    private bool runningAttackLoop = false;
    private WaitForSeconds attackSpeed = new WaitForSeconds(1);
    public float SelectedAbilityAttackRange { private set; get; } = 20f;

    #endregion

    void Start()
    {
        stateHandler = gameObject.GetComponent<AIStateHandler>();
        peaceState = gameObject.GetComponent<AIPeacefulState>();
        obedienceState = gameObject.GetComponent<AIObedienceState>();

        rb = gameObject.GetComponent<Rigidbody>();
        livingBeing = GetComponent<LivingBeing>();

        abilityHandler = GetComponent<CreatureAbilityHandler>();
    }

    public override AIState RunCurrentState()
    {
        if (stateHandler.minionStats != null && stateHandler.minionStats.CurrentCommand == MinionCommands.FocusTarget) stateHandler.SetTarget(obedienceState.commandTarget.GetComponent<LivingBeing>());
        // if minion and has a command to focus a target, set target to be the commanded target 
        if (stateHandler.target != null) // still has a target?
        {
            if (peaceState.FieldOfViewCheck() == true) //is target visible?
            {
                //Debug.Log($"Chasing because field of view check was false. going to last seen location: {stateHandler.target.transform.position}");
                Chase(stateHandler.target.transform.position);
                if (!runningAttackLoop)
                    attackCoroutine = StartCoroutine(HandleAttack(stateHandler.target));
            }
            else
            {
                //Debug.Log($"Chasing because field of view check was false. going to last seen location: {stateHandler.lastSeenLoc}");
                EndAttackRoutine();
                Chase(stateHandler.lastSeenLoc);
            }
            return this;
        }
        else
        {
            EndAttackRoutine();
            return peaceState;
        }
    }

    private void EndAttackRoutine()
    {
        runningAttackLoop = false;
        if (attackCoroutine != null)
            StopCoroutine(attackCoroutine);
    }

    public void Chase(Vector3 targetLoc)
    {

        if (!stateHandler.StuckInAbilityAnimation)
        {
            float distance = (stateHandler.target.transform.position - transform.position).sqrMagnitude;

            if (distance <= SelectedAbilityAttackRange || stateHandler.navAgent.stoppingDistance >= distance * distance) // if distance to target is more than ability range
            {
                IdleAndResetPath();
            }
            else // if distance to target is less than or equal to ability range
            {
                SetDestinationandAnimation(targetLoc);
            }
        }
    }
    private void SetDestinationandAnimation(Vector3 targetLoc)
    {
        stateHandler.navAgent.SetDestination(targetLoc);

        if (stateHandler.anim != null)
        {
            stateHandler.anim.ChangeAnimation("Run", .2f);
        }
    }
    private void IdleAndResetPath()
    {
        stateHandler.navAgent.ResetPath();
        if (stateHandler.anim != null)
        {
            stateHandler.anim.ChangeAnimation("Idle", .2f);
        }
    }

    private IEnumerator HandleAttack(LivingBeing target)
    {
        if (target == null)
        {
            runningAttackLoop = false;
            yield break;
        }
        if (stateHandler.anim != null)
        {
            stateHandler.anim.ChangeAnimation("Idle", .2f);
        }

        runningAttackLoop = true;

        while (runningAttackLoop)
        {
            if (stateHandler.Dead)
            {
                runningAttackLoop = false;
                yield break;
            }

            Debug.Log($"target selected : {target}.");

            Ability ability = stateHandler.abilityHandler.GetAbilityForTarget(target);
            Debug.Log($"Ability selected : {ability}.");
            if (ability != null)
            {
                SetAbilityRange(ability.Range);

                if ((transform.position - target.transform.position).sqrMagnitude <
                    SelectedAbilityAttackRange * SelectedAbilityAttackRange || ability is AuraAbility)
                {
                    abilityHandler.UseAbility(target, ability);
                }
            }

            yield return attackSpeed;
        }
    }

    public void SetAbilityRange(float range)
    {
        SelectedAbilityAttackRange = range;
    }

}




