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
    private Rigidbody2D rb;
    private LivingBeing statScript;
    [SerializeField] GameObject rotationObject;
    private Coroutine attackCoroutine;
    private bool runningAttackLoop = false;
    private CreatureSpriteController spriteController;
    private WaitForSeconds attackSpeed = new WaitForSeconds(1);
    public float MovementSpeed { private set; get; }



    void Start()
    {
        spriteController = GetComponentInChildren<CreatureSpriteController>();
        stateHandler = gameObject.GetComponent<AIStateHandler>();
        peaceState = gameObject.GetComponent<AIPeacefulState>();
        obedienceState = gameObject.GetComponent<AIObedienceState>();

        rb = gameObject.GetComponent<Rigidbody2D>();
        statScript = GetComponent<LivingBeing>();
        abilityHandler = GetComponent<CreatureAbilityHandler>();
        MovementSpeed = statScript.GetAttribute(AttributeType.MovementSpeed);
    }



    public override AIState RunCurrentState()
    {
        if (stateHandler.StuckInAbilityAnimation) return this;
        if (stateHandler.minionStats != null && stateHandler.minionStats.CurrentCommand == MinionCommands.FocusTarget) stateHandler.SetTarget(obedienceState.commandTarget.GetComponent<LivingBeing>());
        if (stateHandler.target != null)
        {
            Vector2 targetLoc = stateHandler.target.transform.position;
            if (peaceState.FieldOfViewCheck() == true)
            {
                Logging.Info($"Chase state checks field of view and finds 'I See the {stateHandler.target}!'");
                Chase(targetLoc);

                LookAtTarget(targetLoc);
                if (!runningAttackLoop)
                    attackCoroutine = StartCoroutine(HandleAttack(stateHandler.target));
            }

            else
            {

                runningAttackLoop = false;
                if (attackCoroutine != null)
                {
                    StopCoroutine(attackCoroutine);
                }
                Chase(stateHandler.lastSeenLoc, true);
                LookAtTarget(stateHandler.lastSeenLoc);
            }
            return this;
        }
        else
        {
            //Debug.Log($"trying to stop attack coroutine because target is null {stateHandler.target}");
            if (attackCoroutine != null)
            {
                runningAttackLoop = false;
                StopCoroutine(attackCoroutine);
            }
            return peaceState;
        }
    }

    public bool CheckInRange()
    {
        Rigidbody2D target = stateHandler.target.GetComponent<Rigidbody2D>();

        Vector3 direction = target.transform.position - transform.position;

        return direction.sqrMagnitude <= 150;

    }
    public void LookAtTarget(Vector2 targetLoc)
    {
        if (!stateHandler.StuckInAbilityAnimation)
        {
            if (rotationObject != null)
            {
                Vector2 direction = (targetLoc - (Vector2)transform.position).normalized;
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                rotationObject.transform.rotation = Quaternion.Euler(0, 0, angle);
                if (spriteController != null)
                    spriteController.SetSpriteDirection(angle);
            }
        }
    }


    public void Chase(Vector2 targetLoc, bool cantSeeTarget = false)
    {
        if (!stateHandler.StuckInAbilityAnimation)
        {
            Debug.Log("Chase func called.");
            Vector2 currentLoc = new Vector2(transform.position.x, transform.position.y);
            Vector2 direction = targetLoc - currentLoc;
            float distance = direction.sqrMagnitude;
            bool uniqueMovement = true;
            if (distance > 10 || peaceState.CheckVisionBlocked(stateHandler.target))
            {
                if (!uniqueMovement || cantSeeTarget)
                {
                    Debug.Log("Chase func called. Either creature cant see target or hasnt the ability to walk strangely");

                    if (direction.sqrMagnitude > 10 || peaceState.CheckVisionBlocked(stateHandler.target))
                    {
                        rb.linearVelocity = (targetLoc - currentLoc) * MovementSpeed;

                        Debug.Log($"target loc = {targetLoc}. target =  {stateHandler.target}");
                    }
                    else
                    {
                        rb.linearVelocity = new Vector2(0, 0);
                        Debug.Log($"vision of target {stateHandler.target} was blocked ({peaceState.CheckVisionBlocked(stateHandler.target)}) or distance was less than 10 {direction.sqrMagnitude}");
                    }
                }
                else StrafeMovement(targetLoc, currentLoc, distance);
            }
        }
    }
    private void StrafeMovement(Vector2 targetLoc, Vector2 currentLoc, float distance)
    {
        Debug.Log("Strafe movent called");

        float a = -60f * distance;
        Vector2 offset = currentLoc - targetLoc;
        float theta = Mathf.Atan2(offset.y, offset.x);

        float dx = a * Mathf.Cos(theta) - a * theta * Mathf.Sin(theta);
        float dy = a * Mathf.Sin(theta) + a * theta * Mathf.Cos(theta);

        rb.linearVelocity = new Vector2(dx, dy).normalized * MovementSpeed * 3;

    }

    private IEnumerator HandleAttack(LivingBeing target)
    {

        runningAttackLoop = true;
        while (true)
        {
            if (target != null)
            {
                //Debug.Log($"target = {target.Name}");

                yield return attackSpeed;
                Vector2 targetLoc = target.transform.position;
                abilityHandler.UseAbility(target);
            }
        }
    }

}




