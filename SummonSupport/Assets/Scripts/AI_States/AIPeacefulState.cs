using UnityEngine;
using System.Collections;
using System.Collections.Generic;
//using UnityEditor.Build.Pipeline;
using UnityEngine.AI;
//using Unity.VisualScripting;


public class AIPeacefulState : AIState
{
    [field: SerializeField] WaitForSeconds FOVCheckFrequency = new WaitForSeconds(0.5f);
    private bool canSeeTarget;
    private AIStateHandler stateHandler;
    private AIChaseState chaseState;
    private GameObject player => stateHandler != null ? stateHandler.player : null;
    private Rigidbody rb;
    #region Support ability use handling
    private bool runningSupportLoop = false;
    private WaitForSeconds supportSpeed = new WaitForSeconds(5);
    private Coroutine supportCoroutine;
    private LivingBeing peaceStateTarget;
    [field: SerializeField] private float playerFollowRadius = 2f;
    #endregion
    #region Hostile target selection
    [SerializeField] private float minionTargetWeight = 1.5f; // how many times as likely a minion is to be picked as a player
    [SerializeField] private float proximityFalloff = 8f; // a target this many metres away is half as likely as one right next to us
    [SerializeField] private float retargetInterval = 5f; // seconds a creature sticks with its target before reconsidering
    [SerializeField] private float currentTargetStickiness = 3f; // extra weight the current target gets when reconsidering
    private LivingBeing hostileTarget;
    private float nextRetargetTime;
    private readonly List<LivingBeing> candidates = new();
    private readonly List<(LivingBeing being, float weight)> candidateWeights = new();
    private AIObedienceState obedienceState;
    #endregion



    void Start()
    {
        if (!NetworkSession.IsAuthority)
        {
            enabled = false;
            return;
        }
        stateHandler = gameObject.GetComponent<AIStateHandler>();
        chaseState = gameObject.GetComponent<AIChaseState>();
        obedienceState = gameObject.GetComponent<AIObedienceState>();
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

    /// <summary>
    /// Picks the hostile this creature fights and makes it the state handler's target.
    /// The choice is weighted-random: closer creatures are more likely and minions are
    /// minionTargetWeight times as likely as players. A chosen target is kept for about
    /// retargetInterval seconds, and gets currentTargetStickiness extra weight when the
    /// choice is reconsidered, so small differences in distance don't cause flip-flopping.
    /// </summary>
    public LivingBeing CheckTargetInRange()
    {
        // A minion told to focus a target fights that one.
        if (stateHandler.minionStats != null && stateHandler.minionStats.CurrentCommand == MinionCommands.FocusTarget)
        {
            LivingBeing commanded = obedienceState != null ? obedienceState.commandTarget : null;
            if (IsAlive(commanded))
            {
                hostileTarget = commanded;
                stateHandler.SetTarget(commanded);
                return commanded;
            }
        }

        bool currentValid = IsValidHostile(hostileTarget, stateHandler.InterestRadius);
        if (!currentValid) hostileTarget = null;
        else if (Time.time < nextRetargetTime)
        {
            stateHandler.SetTarget(hostileTarget);
            return hostileTarget;
        }

        LivingBeing chosen = ChooseWeightedTarget(hostileTarget);
        if (chosen == null) chosen = hostileTarget; // nothing visible: keep chasing the one we had
        if (chosen != null)
        {
            nextRetargetTime = Time.time + retargetInterval * Random.Range(0.75f, 1.25f); // spread re-checks out between creatures
            hostileTarget = chosen;
            stateHandler.SetTarget(chosen);
        }
        return chosen;
    }

    private LivingBeing ChooseWeightedTarget(LivingBeing current)
    {
        candidates.Clear();
        Collider[] rangeChecks = Physics.OverlapSphere(transform.position, stateHandler.DetectionRadius, stateHandler.targetMask);
        foreach (Collider collider in rangeChecks)
        {
            // A creature can have several colliders (e.g. the resurrect trigger), so each counts once.
            if (collider.TryGetComponent(out LivingBeing being) && !candidates.Contains(being)
                && IsValidHostile(being, stateHandler.DetectionRadius) && !CheckVisionBlocked(being))
                candidates.Add(being);
        }
        if (candidates.Count == 0) return null;

        float totalWeight = 0f;
        candidateWeights.Clear();
        foreach (LivingBeing being in candidates)
        {
            float distance = Vector3.Distance(transform.position, being.transform.position);
            float weight = proximityFalloff / (proximityFalloff + distance); // 1 when touching, half at proximityFalloff metres
            if (being.CharacterTag == CharacterTag.Minion) weight *= minionTargetWeight;
            if (being == current) weight *= currentTargetStickiness;
            candidateWeights.Add((being, weight));
            totalWeight += weight;
        }

        float roll = Random.value * totalWeight;
        foreach ((LivingBeing being, float weight) in candidateWeights)
        {
            roll -= weight;
            if (roll <= 0f) return being;
        }
        return candidateWeights[candidateWeights.Count - 1].being;
    }

    private bool IsValidHostile(LivingBeing being, float range)
    {
        if (!IsAlive(being) || being == stateHandler.livingBeing) return false;
        if ((stateHandler.targetMask.value & (1 << being.gameObject.layer)) == 0) return false; // e.g. charm or madness ended
        return (being.transform.position - transform.position).sqrMagnitude <= range * range;
    }

    private static bool IsAlive(LivingBeing being)
    {
        return being != null && being.GetAttribute(AttributeType.CurrentHitpoints) > 0;
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
            if (runningSupportLoop && supportCoroutine != null) // new change where crews brain has possibly lost the ability to write or understand c#
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
        List<LivingBeing> friendlies = new() { stateHandler.playerStats };
        // A minion supports its own leader and that leader's other minions.
        List<LivingBeing> minions = stateHandler.livingBeing is MinionStats
            ? NetworkEnemy.FellowMinionsOf(stateHandler.livingBeing)
            : new List<LivingBeing>(AlchemyHandler.Instance.activeMinions);
        foreach (LivingBeing minion in minions)
        {
            friendlies.Add(minion);
        }

        LivingBeing selectedFriend = friendlies[Random.Range(0, friendlies.Count)];
        return selectedFriend;
    }

    public void GoToPlayer()
    {
        //Debug.Log("Going to player function called");
        if (stateHandler.StuckInAbilityAnimation || player == null) return;
        float distance = (player.transform.position - transform.position).sqrMagnitude;
        if (distance >= stateHandler.navAgent.stoppingDistance + playerFollowRadius || distance >= chaseState.SelectedAbilityAttackRange)
        {
            //Debug.Log($"Going to player becuase: Distance = {distance}. stopping distance = {stateHandler.navAgent.stoppingDistance}. chase state selected ability = {chaseState.SelectedAbilityAttackRange}");

            stateHandler.navAgent.SetDestination(player.transform.position);

            stateHandler.anim.ChangeAnimation("Run", .2f);

        }
        else
        {
            //Debug.Log($"NOToing to player becuase: Distance = {distance}. stopping distance = {stateHandler.navAgent.stoppingDistance}. chase state selected ability = {chaseState.SelectedAbilityAttackRange}");

            stateHandler.navAgent.ResetPath();

            stateHandler.anim.ChangeAnimation("Idle", .2f);

        }
    }
}
