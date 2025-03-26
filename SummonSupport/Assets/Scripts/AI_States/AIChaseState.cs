using Unity.VisualScripting;
using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem.LowLevel;


public class AIChaseState : AIState
{
    private AIStateHandler stateHandler;
    private AIPeacefulState peaceState;
    public GameObject targetEntity { private set; get; }
    private bool targetIsInRange;
    private Rigidbody2D rb;
    private LivingBeing statScript;


    void Start()
    {
        stateHandler = gameObject.GetComponent<AIStateHandler>();
        peaceState = gameObject.GetComponent<AIPeacefulState>();
        targetEntity = peaceState.detectedTargetObject;

        rb = gameObject.GetComponent<Rigidbody2D>();
        statScript = stateHandler.livingBeing;
    }

    public override AIState RunCurrentState()
    {

        Debug.Log("Running chase state");
        Rigidbody2D target = targetEntity.GetComponent<Rigidbody2D>();

        Chase(target.gameObject);


        LookAtTarget(target.gameObject);

        bool targetIsInRange = CheckInRange(target);

        if (targetIsInRange)
        {
            return gameObject.GetComponent<AIAttackState>();
        }
        else return this;
    }

    public bool CheckInRange(Rigidbody2D target)
    {
        Vector3 direction = target.transform.position - transform.position;

        if (direction.sqrMagnitude <= 50)//AbilityHandler.GetLongestRangeAbility(gameObject))
        {
            targetIsInRange = true;
        }
        else targetIsInRange = false;

        return targetIsInRange;

    }
    public void LookAtTarget(GameObject target)
    {
        Vector2 direction = target.transform.position - transform.position;
        transform.up = direction;
        Debug.DrawRay(transform.position, direction.normalized * direction.magnitude, Color.red);
    }

    public void Chase(GameObject target)
    {

        Vector2 targetLoc = target.transform.position;
        Vector2 currentLoc = new Vector2(transform.position.x, transform.position.x);

        rb.linearVelocity = (targetLoc - currentLoc) * 5 * Time.fixedDeltaTime;
    }
}
