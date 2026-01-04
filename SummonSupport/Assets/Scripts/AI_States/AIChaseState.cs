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
                //Debug.Log($"Chasing target because field of view check was true. going to target: {stateHandler.target.Name}");
                Chase(stateHandler.target.transform.position);
                if (!runningAttackLoop)
                    attackCoroutine = StartCoroutine(HandleAttack());
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
        //Debug.Log($"Chasing {stateHandler.target.Name}");

        if (!stateHandler.StuckInAbilityAnimation)
        {
            float distance = (stateHandler.target.transform.position - transform.position).sqrMagnitude;
            //Debug.Log($"Chasing {stateHandler.target.Name} and the distance is: {distance}");
            if (stateHandler.navAgent.speed == 0) return;

            else if (distance <= SelectedAbilityAttackRange || stateHandler.navAgent.stoppingDistance >= distance * distance) // if distance to target is more than ability range
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
        if (livingBeing.SE_Handler.HasStatusEffect(StatusEffectType.Blinded))
        {
            targetLoc = new Vector3(targetLoc.x + Random.Range(-10, 10), targetLoc.y, targetLoc.z + Random.Range(-10, 10));
        }
        stateHandler.navAgent.SetDestination(targetLoc);

        if (stateHandler.anim != null)
        {
            //Debug.Log("Going to run!");
            stateHandler.anim.ChangeAnimation("Run", .2f);
        }
    }
    private void IdleAndResetPath()
    {
        stateHandler.navAgent.ResetPath();
        if (stateHandler.anim != null)
        {
            //Debug.Log("Going to idle!");

            stateHandler.anim.ChangeAnimation("Idle", .2f);
        }
    }

    private IEnumerator HandleAttack()
    {
        if (stateHandler.target == null)
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

            //Debug.Log($"Target is {stateHandler.target.Name} during chse state attack loop");

            Ability ability = stateHandler.abilityHandler.GetAbilityForTarget(stateHandler.target);
            if (ability != null)
            {
                SetAbilityRange(ability.Range);

                if ((transform.position - stateHandler.target.transform.position).sqrMagnitude <
                    SelectedAbilityAttackRange * SelectedAbilityAttackRange || ability is AuraAbility)
                {
                    if (stateHandler.anim != null)
                    {
                        if (ability is ProjectileAbility) stateHandler.anim.ChangeAnimation("Projectile");
                        else stateHandler.anim.ChangeAnimation("Melee");
                    }

                    abilityHandler.UseAbility(stateHandler.target, ability);
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




