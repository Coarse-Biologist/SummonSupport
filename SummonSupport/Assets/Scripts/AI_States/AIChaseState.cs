using System.Collections;
using Unity.VisualScripting;
using UnityEngine;


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
    private CreatureSpriteController spriteController;
    private WaitForSeconds attackSpeed = new WaitForSeconds(1);
    public float SelectedAbilityAttackRange { private set; get; } = 20f;



    void Start()
    {
        spriteController = GetComponentInChildren<CreatureSpriteController>();
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
                Debug.Log($"trying to stop attack coroutine because target is null {stateHandler.target}");

                EndAttackRoutine();
                Chase(stateHandler.lastSeenLoc, true);
            }
            return this;
        }
        else
        {
            Debug.Log($"trying to stop attack coroutine because target is null {stateHandler.target}");
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
    //public void LookAtTarget(Vector3 targetLoc)
    //{
    //    if (rotationObject != null)
    //    {
    //        Vector3 direction = (targetLoc - transform.position).normalized;
    //        float angle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
    //        rotationObject.transform.rotation = Quaternion.Euler(0, angle, 0);
    //        if (spriteController != null)
    //            spriteController.SetSpriteDirection(angle);
    //    }
    //}

    public void Chase(Vector3 targetLoc, bool cantSeeTarget = false)
    {
        if (!stateHandler.StuckInAbilityAnimation)
        {
            Vector3 direction = targetLoc - transform.position;
            bool uniqueMovement = false;

            if (!uniqueMovement || cantSeeTarget)
            {
                if (direction.sqrMagnitude > SelectedAbilityAttackRange)
                {
                    stateHandler.navAgent.SetDestination(targetLoc);// direction* stateHandler.movementScript.MovementSpeed;
                }
                else
                {
                    stateHandler.navAgent.ResetPath();
                }
            }
            else StrafeMovement(targetLoc, direction.sqrMagnitude);

        }
    }
    private void StrafeMovement(Vector3 targetLoc, float distance)
    {
        Debug.Log("Strafe movement called");

        float a = -60f * distance;
        Vector3 offset = transform.position - targetLoc;
        float theta = Mathf.Atan2(offset.y, offset.x);

        float dx = a * Mathf.Cos(theta) - a * theta * Mathf.Sin(theta);
        float dy = a * Mathf.Sin(theta) + a * theta * Mathf.Cos(theta);

        rb.linearVelocity = new Vector2(dx, dy).normalized * stateHandler.movementScript.GetMovementAttribute(MovementAttributes.MovementSpeed) * 3;

    }

    private IEnumerator HandleAttack(LivingBeing target)
    {
        runningAttackLoop = true;
        while (true)
        {
            if (stateHandler.Dead)
            {
                EndAttackRoutine();
            }
            if (target != null)
            {
                Ability ability = GetComponent<CreatureAbilityHandler>().GetAbilityForTarget(target);
                if (ability != null)
                {
                    SetAbilityRange(ability.Range);
                    if ((transform.position - target.transform.position).magnitude < SelectedAbilityAttackRange)
                    {
                        abilityHandler.UseAbility(target, ability);
                    }
                }
                //else Debug.Log("The ability was null during the Handl attack function of the ai chase state");

                yield return attackSpeed;

            }
        }
    }

    public void SetAbilityRange(float range)
    {
        SelectedAbilityAttackRange = range;
    }

}




