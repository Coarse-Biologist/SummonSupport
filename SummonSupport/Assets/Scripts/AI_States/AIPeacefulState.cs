using UnityEngine;
using System.Collections;

public class AIPeacefulState : AIState
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public GameObject detectedTargetObject;
    private bool canSeeTarget;
    private AIStateHandler stateHandler;


    void Start()
    {
        stateHandler = gameObject.GetComponent<AIStateHandler>();
        StartCoroutine(FOVRoutine());
    }

    private IEnumerator FOVRoutine()
    {
        WaitForSeconds wait = new WaitForSeconds(0.2f);
        while (true)
        {
            yield return wait;
            FieldOfViewCheck();
        }
    }

    public bool FieldOfViewCheck()
    {
        bool visionBlocked = true;
        GameObject target = CheckAnyThingInRange();
        if (target != null)
        {
            visionBlocked = CheckVisionBlocked(target);
        }
        Debug.Log($"visionBlocked blocked = {visionBlocked}, target = {target!}");
        if (!visionBlocked && target != null)
        {
            canSeeTarget = true;
            return true;
        }
        else return false;
    }

    public GameObject CheckAnyThingInRange()
    {
        Collider2D[] rangeChecks = Physics2D.OverlapAreaAll(transform.position,
            new Vector2(transform.position.x + stateHandler.DetectionRadius, transform.position.y + stateHandler.DetectionRadius),
            stateHandler.targetMask); // contains lists of all objects in the area?
        if (rangeChecks.Length != 0)
        {
            GameObject detectedObject = rangeChecks[0].transform.gameObject;

            if (gameObject.CompareTag("Minion")) // if attached to minion 
            {
                if (detectedObject.CompareTag("Enemy")) // if detecting an enemy
                {
                    return detectedObject; // return the enemy
                }
            }
            if (gameObject.CompareTag("Enemy"))
            {
                Debug.Log("I am indeed an enemy");
                if (detectedObject.CompareTag("Player") || detectedObject.CompareTag("Minion"))
                {
                    return detectedObject;
                }
                else return null;
            }
            else return null;
        }
        else return null;
    }

    public bool CheckVisionBlocked(GameObject target)
    {
        Vector2 directionToTarget = target.transform.position;
        float distanceToTarget = Vector2.Distance(transform.position, directionToTarget);
        Debug.DrawLine(transform.position, directionToTarget);

        return Physics2D.Raycast(transform.position, directionToTarget, distanceToTarget, stateHandler.obstructionMask);
    }


    public override AIState RunCurrentState()
    {
        //Debug.Log("running peaceful state");
        if (canSeeTarget)
        {
            //Debug.Log("Requesting use of chase")
            return gameObject.GetComponent<AIChaseState>();
        }
        return this;
    }
}
