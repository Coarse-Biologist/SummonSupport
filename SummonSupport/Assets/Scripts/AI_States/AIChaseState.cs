using Unity.VisualScripting;
using UnityEngine;


public class AIChaseState : AIState
{
    private AIStateHandler stateHandler;
    private AIPeacefulState peaceState;
    private AIObedienceState obedienceState;
    public GameObject targetEntity { private set; get; }
    private bool targetIsInRange;
    private Rigidbody2D rb;
    private LivingBeing statScript;



    void Start()
    {
        stateHandler = gameObject.GetComponent<AIStateHandler>();
        peaceState = gameObject.GetComponent<AIPeacefulState>();
        obedienceState = gameObject.GetComponent<AIObedienceState>();

        rb = gameObject.GetComponent<Rigidbody2D>();
        statScript = stateHandler.livingBeing;
    }

    public void SetTargetEntity(GameObject target)
    {
        targetEntity = target;
    }

    public override AIState RunCurrentState()
    {
        //Debug.Log("Running chase state");
        if (stateHandler.minionStats.CurrentCommand == MinionCommands.FocusTarget) targetEntity = obedienceState.commandTarget;
        if (targetEntity != null)
        {
            Vector2 targetLoc = targetEntity.transform.position;
            if (peaceState.FieldOfViewCheck() == true)
            {
                Chase(targetLoc);

                LookAtTarget(targetLoc);
            }
            else
            {
                Chase(stateHandler.lastSeenLoc);

                LookAtTarget(stateHandler.lastSeenLoc);
            }
            return this;
        }
        else return peaceState;
    }

    public bool CheckInRange()
    {
        Rigidbody2D target = targetEntity.GetComponent<Rigidbody2D>();

        Vector3 direction = target.transform.position - transform.position;

        return direction.sqrMagnitude <= 50;

    }
    public void LookAtTarget(Vector2 targetLoc)
    {
        transform.up = targetLoc;
        Debug.DrawRay(transform.position, targetLoc.normalized * targetLoc.magnitude, Color.red);
    }


    public void Chase(Vector2 targetLoc)
    {

        Vector2 currentLoc = new Vector2(transform.position.x, transform.position.y);
        Vector2 direction = targetLoc - currentLoc;
        if (direction.sqrMagnitude > 10 || peaceState.CheckVisionBlocked(targetEntity)) rb.linearVelocity = (targetLoc - currentLoc) * statScript.Speed;
        else rb.linearVelocity = new Vector2(0, 0);
    }
}




