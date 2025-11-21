using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;


public class AIChaseState : AIState
{
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
        if (stateHandler.target != null)
        {
            Vector3 targetLoc = stateHandler.target.transform.position;
            if (peaceState.FieldOfViewCheck() == true)
            {
                Chase(targetLoc);

                if (!runningAttackLoop)
                    attackCoroutine = StartCoroutine(HandleAttack(stateHandler.target));
            }

            else
            {

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

    public bool CheckInRange()
    {
        Rigidbody target = stateHandler.target.GetComponent<Rigidbody>();

        Vector3 direction = target.transform.position - transform.position;

        return direction.sqrMagnitude <= 150;

    }

    public void Chase(Vector3 targetLoc)
    {
        if (!stateHandler.StuckInAbilityAnimation)
        {
            Vector3 direction = targetLoc - transform.position;

            if (direction.sqrMagnitude > SelectedAbilityAttackRange)
            {
                //if (livingBeing.locSphere != null)
                //{
                //    Instantiate(livingBeing.locSphere, stateHandler.target.transform.position, Quaternion.identity);
                //}
                stateHandler.navAgent.SetDestination(stateHandler.target.transform.position);
                if (stateHandler.anim != null)
                {
                    stateHandler.anim.ChangeAnimation("Run", .2f);
                }
            }
            else
            {
                stateHandler.navAgent.ResetPath();
                if (stateHandler.anim != null)
                {
                    stateHandler.anim.ChangeAnimation("Idle", .2f);
                }
            }
        }
    }

    private IEnumerator HandleAttack(LivingBeing target)
    {
        runningAttackLoop = true;

        // Exit immediately if target is null
        if (target == null)
        {
            runningAttackLoop = false;
            yield break;
        }

        while (runningAttackLoop)
        {
            if (stateHandler.Dead)
            {
                runningAttackLoop = false;
                yield break;
            }

            Ability ability = stateHandler.abilityHandler.GetAbilityForTarget(target);

            if (ability != null)
            {
                SetAbilityRange(ability.Range);

                if ((transform.position - target.transform.position).sqrMagnitude <
                    SelectedAbilityAttackRange * SelectedAbilityAttackRange)
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




