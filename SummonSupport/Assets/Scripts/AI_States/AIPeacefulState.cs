using UnityEngine;
using System.Collections;

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
            Debug.Log($"I can see the target at {target.transform.position}");
            canSeeTarget = true;
            return true;
        }
        else return false;
    }

    public GameObject CheckTargetInRange()
    {
        //if (Vector3.Angle(transform.forward, directionToTarget) < angle / 2)
        Collider2D[] rangeChecks = Physics2D.OverlapCircleAll(transform.position, stateHandler.DetectionRadius, stateHandler.targetMask); // contains lists of all objects in the area?

        if (rangeChecks.Length != 0)
        {
            GameObject detectedObject = rangeChecks[0].transform.gameObject;
            chaseState.SetTargetEntity(detectedObject);
            return detectedObject;
        }
        else
        {
            return null;
        }

    }

    public bool CheckVisionBlocked(GameObject target)
    {
        // Correct way to calculate the direction vector
        Vector2 directionToTarget = (target.transform.position - transform.position).normalized;

        // Distance between the two objects
        float distanceToTarget = Vector2.Distance(transform.position, target.transform.position);

        // Debug the raycast for visual feedback
        Debug.DrawLine(transform.position, target.transform.position, Color.red);

        // Perform the raycast with direction and distance
        return Physics2D.Raycast(transform.position, directionToTarget, distanceToTarget, stateHandler.obstructionMask);
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
