using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Build.Pipeline;
using UnityEngine.AI;
using Unity.VisualScripting;


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
            foreach (Collider collider in rangeChecks)
            {
                GameObject detectedObject = collider.gameObject;
                if (detectedObject.TryGetComponent(out LivingBeing targetLivingBeing))
                    if (targetLivingBeing.CharacterTag == preferedTargetType || rangeChecks.Length == 1)
                        if (targetLivingBeing.GetAttribute(AttributeType.CurrentHitpoints) > 0)
                        {
                            stateHandler.SetTarget(targetLivingBeing);
                            target = targetLivingBeing;
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
            stateHandler.SetTarget(stateHandler.playerStats);
            GoToPlayer();
            if (!runningSupportLoop)
                supportCoroutine = StartCoroutine(HandleSupportloop());

            return this;
        }
        else return this;

    }
    private IEnumerator HandleSupportloop()
    {
        LivingBeing targetStats;
        Ability ability;
        runningSupportLoop = true;
        while (true)
        {
            targetStats = SelectFriendlyTarget();
            stateHandler.SetTarget(targetStats);
            ability = stateHandler.abilityHandler.GetAbilityForTarget(targetStats, targetStats == stateHandler.minionStats);
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
        List<LivingBeing> friendlies = new() { stateHandler.playerStats, stateHandler.livingBeing };
        foreach (GameObject minion in CommandMinion.activeMinions)
        {
            friendlies.Add(minion.GetComponent<LivingBeing>());
        }

        LivingBeing selectedFriend = friendlies[Random.Range(0, friendlies.Count)];
        return selectedFriend;
    }

    public void GoToPlayer()
    {
        Debug.Log("Going to player function called");
        float distance = (player.transform.position - transform.position).sqrMagnitude;
        if (distance >= stateHandler.navAgent.stoppingDistance || distance >= chaseState.SelectedAbilityAttackRange)
        {
            Debug.Log($"Going to player becuase: Distance = {distance}. stopping distance = {stateHandler.navAgent.stoppingDistance}. chase state selected ability = {chaseState.SelectedAbilityAttackRange}");

            stateHandler.navAgent.SetDestination(player.transform.position);
            if (stateHandler.anim != null)
            {
                stateHandler.anim.ChangeAnimation("Run", .2f);
            }
        }
        else
        {
            Debug.Log($"NOToing to player becuase: Distance = {distance}. stopping distance = {stateHandler.navAgent.stoppingDistance}. chase state selected ability = {chaseState.SelectedAbilityAttackRange}");

            stateHandler.navAgent.ResetPath();
            if (stateHandler.anim != null)
            {
                stateHandler.anim.ChangeAnimation("Idle", .2f);
            }
        }
    }
}
