using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Build.Pipeline;


public class AIPeacefulState : AIState
{
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
        LivingBeing target = CheckTargetInRange();

        if (target != null && !CheckVisionBlocked(target))
        {
            stateHandler.lastSeenLoc = target.transform.position;
            canSeeTarget = true;
            //Debug.Log($"Field of view Check setting canSeetarget to {canSeeTarget} ({target})");

            return true;
        }
        else
        {
            //Debug.Log($"Field of view Check: result of vision block check: cant see target ({target})");
            canSeeTarget = false;
            return false;
        }
    }

    public LivingBeing CheckTargetInRange()
    {
        //if (Vector3.Angle(transform.forward, directionToTarget) < angle / 2)
        Collider2D[] rangeChecks = Physics2D.OverlapCircleAll(transform.position, stateHandler.DetectionRadius, stateHandler.targetMask);
        //Debug.Log($"checking target in range: Looking for target of target mask {stateHandler.targetMask.value}");
        LivingBeing target = null;
        CharacterTag preferedTargetType = GetTargetPreference();
        if (rangeChecks.Length != 0)
        {
            for (int i = 0; i < rangeChecks.Length; i++)
            {

                GameObject detectedObject = rangeChecks[i].transform.gameObject;
                if (detectedObject == gameObject) continue;
                if (detectedObject.TryGetComponent<LivingBeing>(out LivingBeing targetLivingBeing))
                {
                    if (targetLivingBeing.CharacterTag == preferedTargetType || rangeChecks.Length == 1)
                    {
                        if (targetLivingBeing.GetAttribute(AttributeType.CurrentHitpoints) > 0)
                        {
                            stateHandler.SetTarget(targetLivingBeing);
                            target = targetLivingBeing;
                        }
                    }
                }
            }
            if (stateHandler.minionStats != null && target != null) Debug.Log($"Check target in range func returning {target.Name}");
        }
        return target;

    }

    private CharacterTag GetTargetPreference()
    {
        return Random.Range(0, 100) > 90 ? CharacterTag.Player : CharacterTag.Minion;
    }
    // if pref minion, look for minion, if prefers player, look player. if finds return. else returnnother.



    public bool CheckVisionBlocked(LivingBeing target, float angleOffset = 0)
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
        //("Run current state: peaceful state says 'running peaceful state'");
        if (stateHandler.minionStats != null) { Debug.Log($"{stateHandler.minionStats} is maybe peacefully looking around"); }

        if (canSeeTarget)
        {
            //Debug.Log("Run current state: peaceful state says 'Requesting chase state'");
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
            chaseState.LookAtTarget(player.transform.position);
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
            Ability ability = GetComponent<CreatureAbilityHandler>().GetAbilityForTarget(targetStats, targetStats == stateHandler.minionStats);
            if (ability != null)
            {
                chaseState.SetAbilityRange(ability.Range);
                if ((transform.position - targetStats.transform.position).magnitude < chaseState.SelectedAbilityAttackRange)
                {
                    stateHandler.abilityHandler.UseAbility(targetStats, ability);
                }
            }
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
        return selectedFriend;
    }

    public void GoToPlayer()
    {
        Vector2 currentLoc = new Vector2(transform.position.x, transform.position.y);
        Vector2 playerPos = player.transform.position;
        Vector2 direction = playerPos - currentLoc;

        rb.linearVelocity = direction * stateHandler.movementScript.GetMovementAttribute(MovementAttributes.MovementSpeed);

    }
}
