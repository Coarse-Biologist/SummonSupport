using System.Collections;
using Unity.VisualScripting;
using UnityEngine;


public class AIChaseState : AIState
{
    private AIStateHandler stateHandler;
    private AIPeacefulState peaceState;
    private AIObedienceState obedienceState;
    public GameObject targetEntity { private set; get; }
    private CreatureAbilityHandler abilityHandler;
    private bool targetIsInRange;
    private Rigidbody2D rb;
    private LivingBeing statScript;
    [SerializeField] GameObject rotationObject;
    private Coroutine attackCoroutine;
    private bool runningAttackLoop = false;
    private CreatureSpriteController spriteController;
    private WaitForSeconds attackSpeed = new WaitForSeconds(1);
    private float MovementSpeed;



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

    public void SetTargetEntity(GameObject target)
    {
        targetEntity = target;
    }

    public override AIState RunCurrentState()
    {
        //Debug.Log("Running chase state");
        if (stateHandler.minionStats != null && stateHandler.minionStats.CurrentCommand == MinionCommands.FocusTarget) targetEntity = obedienceState.commandTarget;
        if (targetEntity != null)
        {
            Vector2 targetLoc = targetEntity.transform.position;
            if (peaceState.FieldOfViewCheck() == true)
            {
                //Logging.Info("I See the player!!!");
                Chase(targetLoc);

                LookAtTarget(targetLoc);
                if (!runningAttackLoop)
                    attackCoroutine = StartCoroutine(HandleAttack(targetEntity));
            }
            else
            {
                //Logging.Info("I  dont see the player");
                runningAttackLoop = false;

                StopCoroutine(attackCoroutine);

                Chase(stateHandler.lastSeenLoc);

                LookAtTarget(stateHandler.lastSeenLoc);
            }
            return this;
        }
        else
        {
            StopCoroutine(attackCoroutine);
            return peaceState;
        }
    }

    public bool CheckInRange()
    {
        Rigidbody2D target = targetEntity.GetComponent<Rigidbody2D>();

        Vector3 direction = target.transform.position - transform.position;

        return direction.sqrMagnitude <= 150;

    }
    public void LookAtTarget(Vector2 targetLoc)
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


    public void Chase(Vector2 targetLoc)
    {
        Vector2 currentLoc = new Vector2(transform.position.x, transform.position.y);
        Vector2 direction = targetLoc - currentLoc;
        float distance = direction.sqrMagnitude;
        bool uniqueMovement = true;
        if (distance > 10 || peaceState.CheckVisionBlocked(targetEntity))
        {
            if (!uniqueMovement)
            {
                if (direction.sqrMagnitude > 10 || peaceState.CheckVisionBlocked(targetEntity)) rb.linearVelocity = (targetLoc - currentLoc) * MovementSpeed;
                else rb.linearVelocity = new Vector2(0, 0);
            }
            else StrafeMovement(targetLoc, currentLoc, distance);
        }
    }
    private void StrafeMovement(Vector2 targetLoc, Vector2 currentLoc, float distance)
    {
        float a = -60f * distance;
        Vector2 offset = currentLoc - targetLoc;
        float theta = Mathf.Atan2(offset.y, offset.x);

        float dx = a * Mathf.Cos(theta) - a * theta * Mathf.Sin(theta);
        float dy = a * Mathf.Sin(theta) + a * theta * Mathf.Cos(theta);

        rb.linearVelocity = new Vector2(dx, dy).normalized * MovementSpeed;

    }

    private IEnumerator HandleAttack(GameObject target)
    {
        runningAttackLoop = true;
        while (true)
        {
            yield return attackSpeed;
            Vector2 targetLoc = target.transform.position;
            abilityHandler.UseAbility(target.GetComponent<LivingBeing>());
        }

    }

}




