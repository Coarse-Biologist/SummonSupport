using UnityEngine;
using System.Collections;

public class AIPeacefulState : AIState
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public GameObject detectedTargetObject;

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
        //Debug.Log("Checking field of view");

        Collider2D[] rangeChecks = Physics2D.OverlapAreaAll(transform.position,
    new Vector2(transform.position.x + 50, transform.position.y + 50),
    stateHandler.targetMask);


        if (rangeChecks.Length != 0)
        {
            //Debug.Log("More than 0 objects were dans ma regarde");
            Transform target = rangeChecks[0].transform;
            Vector3 directionToTarget = (target.position - transform.position).normalized;

            if (Vector3.Angle(transform.forward, directionToTarget) < stateHandler.AngleOfSight / 2)
            {
                //Debug.Log("something is in my line of sight");
                float distanceToTarget = Vector3.Distance(transform.position, target.position);

                if (!Physics2D.Raycast(transform.position, directionToTarget, distanceToTarget, stateHandler.obstructionMask))
                {
                    //Debug.Log("nothing is blocking my line of sight");

                    if (target.CompareTag("Player"))
                    {
                        //Debug.Log($"tag =  {target.tag} detected");
                        canSeeTarget = true;
                        detectedTargetObject = target.gameObject;
                    }

                }
                else
                {
                    //Debug.Log("something is blocking my line of sight");
                    canSeeTarget = false;
                }

            }
            else
            {
                canSeeTarget = false;
                //Debug.Log(" the thing is out of my line of sight");
            }

        }
        else if (canSeeTarget)
        {
            //Debug.Log("no more than 0 objects were dans ma regarde");

            canSeeTarget = false;
        }

    }

    public override AIState RunCurrentState()
    {
        Debug.Log("running peaceful state");

        if (canSeeTarget)
        {
            Debug.Log("Requesting use of chase");

            return gameObject.GetComponent<AIChaseState>();
        }
        return this;
    }
}
