using UnityEngine;
using System.Collections;
using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;

public class AIPeacefulState : AIState
{
    public GameObject detectedTargetObject;
    [field: SerializeField] WaitForSeconds FOVCheckFrequency = new WaitForSeconds(0.5f);
    private bool canSeeTarget;
    private AIStateHandler stateHandler;
    private AIChaseState chaseState;
    private GameObject player;
    private bool closeToPlayer = false;
    private Rigidbody2D rb;
    #region Support ability use handling
    private bool runningSupportLoop = false;
    private WaitForSeconds supportSpeed = new WaitForSeconds(1);
    private Coroutine supportCoroutine;
    #endregion



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

        while (true)
        {
            yield return FOVCheckFrequency;
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
            stateHandler.lastSeenLoc = target.transform.position;
            canSeeTarget = true;
            return true;
        }
        else
        {
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
        Vector2 directionToTarget = (target.transform.position - transform.position).normalized;

        float distanceToTarget = Vector2.Distance(transform.position, target.transform.position);

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
            if (runningSupportLoop)
            {
                runningSupportLoop = false;

                StopCoroutine(supportCoroutine);
            }

            return chaseState;
        }
        else if (player)
        {
            Vector2 currentLoc = new Vector2(transform.position.x, transform.position.y);
            Vector2 playerPos = player.transform.position;
            Vector2 direction = playerPos - currentLoc;
            stateHandler.SetTarget(stateHandler.playerStats);
            if (!closeToPlayer && CompareTag("Minion") && (direction.sqrMagnitude > stateHandler.FollowRadius))
            {
                GoToPlayer();
                // function which will decide whether minion should try to use some buff or heal on player or fellow minions
                if (!runningSupportLoop)
                    supportCoroutine = StartCoroutine(HandleSupportloop());
            }
            return this;
        }
        else
        {
            return this;
        }
    }
    private IEnumerator HandleSupportloop()
    {
        LivingBeing targetStats;
        runningSupportLoop = true;
        while (true)
        {
            targetStats = SelectFriendlyTarget();
            chaseState.LookAtTarget(targetStats.transform.position);
            stateHandler.SetTarget(targetStats);
            stateHandler.abilityHandler.UseAbility(targetStats);

            yield return supportSpeed;
        }

    }
    private LivingBeing SelectFriendlyTarget()
    {
        List<LivingBeing> friendlies = new();
        if (player.TryGetComponent<LivingBeing>(out var playerStats))
        {
            friendlies.Add(playerStats);
            friendlies.Add(gameObject.GetComponent<LivingBeing>());
            foreach (GameObject minion in CommandMinion.activeMinions)
            {
                friendlies.Add(minion.GetComponent<LivingBeing>());
            }
        }
        LivingBeing selectedFriend = friendlies[UnityEngine.Random.Range(0, friendlies.Count)];
        Debug.Log($"Selecting friend: {selectedFriend}");
        return selectedFriend;
    }

    public void GoToPlayer()
    {
        Vector2 currentLoc = new Vector2(transform.position.x, transform.position.y);
        Vector2 playerPos = player.transform.position;
        Vector2 direction = playerPos - currentLoc;

        rb.linearVelocity = direction * stateHandler.livingBeing.Speed;

    }
}
