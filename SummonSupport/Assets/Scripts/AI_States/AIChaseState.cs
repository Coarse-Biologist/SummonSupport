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



    void Start()
    {
        stateHandler = gameObject.GetComponent<AIStateHandler>();
        peaceState = gameObject.GetComponent<AIPeacefulState>();
        obedienceState = gameObject.GetComponent<AIObedienceState>();

        rb = gameObject.GetComponent<Rigidbody2D>();
        statScript = PlayerStats.Instance;
        abilityHandler = GetComponent<CreatureAbilityHandler>();
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
                Logging.Info("I See the player!!!");
                Chase(targetLoc);

                LookAtTarget(targetLoc);
                if (!runningAttackLoop)
                    attackCoroutine = StartCoroutine(HandleAttack(targetLoc));
            }
            else
            {
                Logging.Info("I  dont see the player");
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

        return direction.sqrMagnitude <= 50;

    }
    public void LookAtTarget(Vector2 targetLoc)
    {
        if (rotationObject != null)
        {
            Vector2 direction = (targetLoc - (Vector2)transform.position).normalized;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            rotationObject.transform.rotation = Quaternion.Euler(0, 0, angle);
            Debug.DrawRay(rotationObject.transform.position, targetLoc.normalized * targetLoc.magnitude, Color.red);
        }
    }


    public void Chase(Vector2 targetLoc)
    {
        Vector2 currentLoc = new Vector2(transform.position.x, transform.position.y);
        Vector2 direction = targetLoc - currentLoc;
        if (direction.sqrMagnitude > 10 || peaceState.CheckVisionBlocked(targetEntity)) rb.linearVelocity = (targetLoc - currentLoc) * statScript.Speed;
        else rb.linearVelocity = new Vector2(0, 0);
    }

    private IEnumerator HandleAttack(Vector2 targetLoc)
    {
        runningAttackLoop = true;
        while (true)
        {
            yield return new WaitForSeconds(1f);
            abilityHandler.UseAbility(targetLoc);
        }

    }

}




