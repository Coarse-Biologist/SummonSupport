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
        //targetEntity = peaceState.detectedTargetObject;

        rb = gameObject.GetComponent<Rigidbody2D>();
        statScript = stateHandler.livingBeing;
        //Debug.Log($"stat script = {statScript}");
    }

    public void SetTargetEntity(GameObject target)
    {
        targetEntity = target;
    }

    public override AIState RunCurrentState()
    {
        //Debug.Log("Running chase state");
        Vector2 targetLoc = targetEntity.transform.position;
        if (peaceState.FieldOfViewCheck() == true)
        {
            Chase(targetLoc);

            LookAtTarget(targetLoc);
        }
        else
        {
            Chase(stateHandler.lastSeenLoc);

            LookAtTarget(stateHandler.lastSeenLoc);
            //return peaceState;
        }
        return this;

        //bool targetIsInRange = CheckInRange(target);
        //
        //if (targetIsInRange)
        //{
        //    return this; //gameObject.GetComponent<AIAttackState>();
        //}
        //else return this;
    }

    public bool CheckInRange()
    {
        Rigidbody2D target = targetEntity.GetComponent<Rigidbody2D>();

        Vector3 direction = target.transform.position - transform.position;

        return direction.sqrMagnitude <= 50;//AbilityHandler.GetLongestRangeAbility

    }
    public void LookAtTarget(Vector2 targetLoc)
    {
        transform.up = targetLoc;
        Debug.DrawRay(transform.position, targetLoc.normalized * targetLoc.magnitude, Color.red);
    }


    public void Chase(Vector2 targetLoc)
    {
        Vector2 currentLoc = new Vector2(transform.position.x, transform.position.y);
        Vector2 direction = targetLoc - currentLoc;
        if (direction.sqrMagnitude > 10 || peaceState.CheckVisionBlocked(targetEntity)) rb.linearVelocity = Time.fixedDeltaTime * (targetLoc - currentLoc) * statScript.Speed;
        else rb.linearVelocity = new Vector2(0, 0);

    }


}





//public void Chase(Vector2 targetLoc)
//    {
//        Vector2 currentLoc = new Vector2(transform.position.x, transform.position.y);
//        Vector2 direction = targetLoc - currentLoc;
//        if (direction.sqrMagnitude > 10 || peaceState.CheckVisionBlocked(targetEntity))
//        {
//            if (peaceState.CheckVisionBlocked(targetEntity, 10))
//                targetLoc = peaceState.RotatePoint(targetLoc, transform.position, -1);
//            if (peaceState.CheckVisionBlocked(targetEntity, -10))
//                targetLoc = peaceState.RotatePoint(targetLoc, transform.position, 1);
//
//            rb.linearVelocity = Time.fixedDeltaTime * (targetLoc - currentLoc) * statScript.Speed;
//        }
//        else rb.linearVelocity = new Vector2(0, 0);
//
//    }
//|| Vector2.Distance(transform.position, stateHandler.lastSeenLoc) < 0.5f