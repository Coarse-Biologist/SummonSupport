using UnityEngine;
using System.Collections;
using System;

public class AIPeacefulState : AIState
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public GameObject detectedTargetObject;
    private bool canSeeTarget;
    private AIStateHandler stateHandler;
    private AIChaseState chaseState;


    void Start()
    {
        stateHandler = gameObject.GetComponent<AIStateHandler>();
        chaseState = gameObject.GetComponent<AIChaseState>();
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
        GameObject target = CheckTargetInRange();
        if (target != null)
        {
            visionBlocked = CheckVisionBlocked(target);
        }
        if (!visionBlocked && target != null)
        {
            //Debug.Log($"I can see the target at {target.transform.position}");
            stateHandler.lastSeenLoc = target.transform.position;
            canSeeTarget = true;
            return true;
        }
        else return false;
    }

    public GameObject CheckTargetInRange()
    {
        //if (Vector3.Angle(transform.forward, directionToTarget) < angle / 2)
        Collider2D[] rangeChecks = Physics2D.OverlapCircleAll(transform.position, stateHandler.DetectionRadius, stateHandler.targetMask);
        if (rangeChecks.Length != 0)
        {
            Debug.DrawRay(transform.position, rangeChecks[0].transform.position, Color.green, 2f);

            GameObject detectedObject = rangeChecks[0].transform.gameObject;
            chaseState.SetTargetEntity(detectedObject);
            return detectedObject;
        }
        else
        {
            return null;
        }

    }

    public bool CheckVisionBlocked(GameObject target, float angleOffset = 0)
    {
        // Correct way to calculate the direction vector
        Vector2 directionToTarget = (target.transform.position - transform.position).normalized;

        // Distance between the two objects
        float distanceToTarget = Vector2.Distance(transform.position, target.transform.position);

        // Debug the raycast for visual feedback
        Debug.DrawLine(transform.position, target.transform.position, Color.red);

        if (angleOffset != 0) directionToTarget = RotatePoint(directionToTarget, transform.position, angleOffset);

        // Perform the raycast with direction and distance
        return Physics2D.Raycast(transform.position, directionToTarget, distanceToTarget, stateHandler.obstructionMask);
    }
    public Vector2 RotatePoint(Vector2 targetLoc, Vector2 origin, float angleDegrees)
    {
        float angleRad = angleDegrees * Mathf.Deg2Rad; // Convert degrees to radians
        float cosTheta = Mathf.Cos(angleRad);
        float sinTheta = Mathf.Sin(angleRad);

        return new Vector2(
            cosTheta * targetLoc.x - sinTheta * targetLoc.y,
            sinTheta * targetLoc.x + cosTheta * targetLoc.y
        ) + origin;
    }


    public override AIState RunCurrentState()
    {
        //Debug.Log("running peaceful state");
        if (canSeeTarget)
        {
            //Debug.Log("Requesting use of chase")
            return chaseState;
        }
        return this;
    }
}

//
//if (gameObject.CompareTag("Minion")) // if attached to minion 
//{
//    if (detectedObject.CompareTag("Enemy")) // if detecting an enemy
//    {
//        stateHandler.lastSeenLoc = detectedObject.transform.position;
//        return detectedObject; // return the enemy
//    }
//}
//if (gameObject.CompareTag("Enemy"))
//{
//    Debug.Log("I am indeed an enemy");
//    if (detectedObject.CompareTag("Player") || detectedObject.CompareTag("Minion"))
//    {
//        Debug.Log($"player or mion detected at {detectedObject.transform.position}");
//        stateHandler.lastSeenLoc = detectedObject.transform.position;
//        return detectedObject;
//    }
//    else return null;
//}
//else
//{
//    Debug.Log($"detected Object not a player");
//
//    GetComponent<AIChaseState>().Chase(stateHandler.lastSeenLoc);
//    return null;
//}
