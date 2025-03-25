using Unity.VisualScripting;
using UnityEngine;
using System.Collections;


public class AIChaseState : AIState
{
    private AIStateHandler stateHandler;
    private AIPeacefulState peaceState;
    public GameObject targetEntity { private set; get; }
    private bool targetIsInRange;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        stateHandler = gameObject.GetComponent<AIStateHandler>();
        peaceState = gameObject.GetComponent<AIPeacefulState>();
    }

    public override AIState RunCurrentState()
    {
        Debug.Log("Running chase state");

        targetEntity = peaceState.detectedTargetObject;

        transform.position = Vector3.Lerp(transform.position, targetEntity.transform.position, 2);

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

        if (direction.sqrMagnitude <= 50)//AbilityHandler.GetLongestRangeAbility(gameObject))
        {
            targetIsInRange = true;
        }
        else targetIsInRange = false;

        return targetIsInRange;

    }
}
