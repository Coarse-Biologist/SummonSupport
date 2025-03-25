using UnityEngine;
using System.Collections;

public class AIPeacefulState : AIState
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private bool canSeeTarget;
    private AIStateHandler stateHandler;


    void Start()
    {
        stateHandler = gameObject.GetComponent<AIStateHandler>();
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

    private void FieldOfViewCheck()
    {
        Collider[] rangeChecks = Physics.OverlapSphere(transform.position, stateHandler.DetectionRadius, stateHandler.targetMask);


        if (rangeChecks.Length != 0)
        {

            Transform target = rangeChecks[0].transform;
            Vector3 directionToTarget = (target.position - transform.position).normalized;

            if (Vector3.Angle(transform.forward, directionToTarget) < stateHandler.AngleOfSight / 2)
            {
                float distanceToTarget = Vector3.Distance(transform.position, target.position);

                if (!Physics.Raycast(transform.position, directionToTarget, distanceToTarget, stateHandler.obstructionMask))
                {

                    if (target.CompareTag("Player"))
                    {
                        //Debug.Log($"tag =  {target.tag}, layer = {targetMask}");
                        canSeeTarget = true;
                        detectedTargetObject = target.gameObject;
                    }

                }
                else
                {
                    //Debug.Log($"tag =  {target.tag}, layer = {targetMask}");
                    canSeeTarget = false;
                }

            }
            else
                canSeeTarget = false;
        }
        else if (canSeeTarget)
            canSeeTarget = false;
    }

    public override AIState RunCurrentState()
    {
        if (canSeeTarget)
        {
            return stateHandler.chaseState;
        }
        return this;
    }
}
