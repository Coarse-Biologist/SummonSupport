using Unity.VisualScripting;
using UnityEngine;
using System.Collections;


public class AIChaseState : AIState
{
    private AIStateHandler stateHandler;
    private GameObject targetEntity;
    private bool targetIsInRange;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        stateHandler = gameObject.GetComponent<AIStateHandler>();
        targetEntity = stateHandler.peaceState.detectedTargetObject;
    }

    public override AIState RunCurrentState()
    {
        targetEntity = stateHandler.peaceState.detectedTargetObject;
        //NavMeshAgent agent = GetComponentInParent<NavMeshAgent>();

        //agent.SetDestination(targetEntity.transform.position);
        transform.LookAt(targetEntity.transform);

        Rigidbody target = targetEntity.GetComponent<Rigidbody>();

        bool targetIsInRange = CheckInRange(target);

        if (targetIsInRange)
        {
            return this;
        }
        return this;
    }

    public bool CheckInRange(Rigidbody target)
    {
        Vector3 direction = targetEntity.transform.position - transform.position;

        if (direction.sqrMagnitude <= 10)//AbilityHandler.GetLongestRangeAbility(gameObject))
        {
            targetIsInRange = true;
        }
        else targetIsInRange = false;

        return targetIsInRange;

    }
}
