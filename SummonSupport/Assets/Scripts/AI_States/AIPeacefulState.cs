using UnityEngine;
using System.Collections;
using System;

public class AIPeacefulState : AIState
{
    public GameObject detectedTargetObject;
    private bool canSeeTarget;
    private AIStateHandler stateHandler;
    private AIChaseState chaseState;
    private GameObject player;
    private bool closeToPlayer = false;
    private Rigidbody2D rb;


    void Start()
    {
        player = PlayerStats.Instance.gameObject;
        stateHandler = gameObject.GetComponent<AIStateHandler>();
        chaseState = gameObject.GetComponent<AIChaseState>();
        rb = GetComponent<Rigidbody2D>();
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
            Logging.Info($"target seen : {target}");
            stateHandler.lastSeenLoc = target.transform.position;
            canSeeTarget = true;
            return true;
        }
        else
        {
            Logging.Info($"No target seen");
            canSeeTarget = false;
            return false;
        }
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
            Debug.Log("Requesting chase state");
            return chaseState;
        }
        else if (player)
        {
            Vector2 currentLoc = new Vector2(transform.position.x, transform.position.y);
            Vector2 playerPos = player.transform.position;
            Vector2 direction = playerPos - currentLoc;
            if (!closeToPlayer && CompareTag("Minion") && (direction.sqrMagnitude > stateHandler.FollowRadius))
            {
                GoToPlayer();
            }
            return this;
        }
        else
        {
            return this;
        }
    }

    public void GoToPlayer()
    {
        Vector2 currentLoc = new Vector2(transform.position.x, transform.position.y);
        Vector2 playerPos = player.transform.position;
        Vector2 direction = playerPos - currentLoc;

        rb.linearVelocity = direction * stateHandler.livingBeing.Speed;

    }
}
