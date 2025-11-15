using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Build.Pipeline;
using UnityEngine.AI;


public class AIPeacefulState : AIState
{
    [field: SerializeField] WaitForSeconds FOVCheckFrequency = new WaitForSeconds(0.5f);
    private bool canSeeTarget;
    private AIStateHandler stateHandler;
    private AIChaseState chaseState;
    private GameObject player;
    private Rigidbody rb;
    #region Support ability use handling
    private bool runningSupportLoop = false;
    private WaitForSeconds supportSpeed = new WaitForSeconds(5);
    private Coroutine supportCoroutine;
    private LivingBeing peaceStateTarget;
    #endregion



    void Start()
    {
        player = PlayerStats.Instance.gameObject;
        stateHandler = gameObject.GetComponent<AIStateHandler>();
        chaseState = gameObject.GetComponent<AIChaseState>();
        rb = GetComponent<Rigidbody>();
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
        peaceStateTarget = CheckTargetInRange();
        if (peaceStateTarget != null && !CheckVisionBlocked(peaceStateTarget))
        {
            stateHandler.lastSeenLoc = peaceStateTarget.transform.position;
            canSeeTarget = true;
            return true;
        }
        else
        {
            canSeeTarget = false;
            return false;
        }
    }

    public LivingBeing CheckTargetInRange()
    {
        Collider[] rangeChecks = Physics.OverlapSphere(transform.position, stateHandler.DetectionRadius, stateHandler.targetMask);
        LivingBeing target = null;
        CharacterTag preferedTargetType = GetTargetPreference();
        if (rangeChecks.Length != 0)
        {
            for (int i = 0; i < rangeChecks.Length; i++)
            {
                GameObject detectedObject = rangeChecks[i].transform.gameObject;
                if (detectedObject.TryGetComponent(out LivingBeing targetLivingBeing))
                {
                    if (targetLivingBeing.CharacterTag == preferedTargetType || rangeChecks.Length == 1)
                    {
                        if (targetLivingBeing.GetAttribute(AttributeType.CurrentHitpoints) > 0)
                        {
                            if (stateHandler.target != targetLivingBeing)
                            {
                                stateHandler.SetTarget(targetLivingBeing);
                            }
                            target = targetLivingBeing;
                        }
                    }
                }
            }
        }

        return target;

    }
    //Returns either player, or more likely a minion if this script is attached to an enemy. if attached to a minion it will return either player or minion
    private CharacterTag GetTargetPreference()
    {
        if (stateHandler.charType == CharacterTag.Enemy)
            return Random.Range(0, 100) > 90 ? CharacterTag.Player : CharacterTag.Minion;
        else
            return CharacterTag.Enemy;
    }



    public bool CheckVisionBlocked(LivingBeing target, float angleOffset = 0)
    {
        bool blocked = NavMesh.Raycast(stateHandler.navAgent.transform.position, target.transform.position, out NavMeshHit hit, NavMesh.AllAreas);

        return blocked;

    }



    public override AIState RunCurrentState()
    {
        if (canSeeTarget)
        {
            if (runningSupportLoop)
            {
                runningSupportLoop = false;

                StopCoroutine(supportCoroutine);
            }

            return chaseState;
        }
        else if (stateHandler.charType == CharacterTag.Minion)
        {

            Vector3 direction = player.transform.position - transform.position;
            stateHandler.SetTarget(stateHandler.playerStats);
            if (direction.sqrMagnitude > stateHandler.navAgent.stoppingDistance)
            {
                GoToPlayer();

                if (!runningSupportLoop)
                    supportCoroutine = StartCoroutine(HandleSupportloop());
            }
            else
            {
                stateHandler.navAgent.ResetPath();
            }
            return this;
        }
        else return this;

    }
    private IEnumerator HandleSupportloop()
    {
        LivingBeing targetStats;
        runningSupportLoop = true;
        while (true)
        {
            targetStats = SelectFriendlyTarget();
            //chaseState.LookAtTarget(targetStats.transform.position);
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
        LivingBeing selectedFriend = friendlies[Random.Range(0, friendlies.Count)];
        return selectedFriend;
    }

    public void GoToPlayer()
    {
        stateHandler.navAgent.SetDestination(player.transform.position);
    }
}
