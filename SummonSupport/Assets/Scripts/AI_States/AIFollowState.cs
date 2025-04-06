using UnityEngine;
using System.Collections;


public class AIFollowState : AIState
{

    private Transform player;
    private AIPeacefulState peacefulState;
    private AIStateHandler stateHandler;
    private Rigidbody2D rb;
    private Vector2 playerLoc;
    private bool closeToPlayer = false;

    // Update is called once per frame
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        stateHandler = GetComponent<AIStateHandler>();
        playerLoc = player.position;
        CheckCloseToPlayer();
    }
    private IEnumerator CheckCloseToPlayer()
    {
        if (!closeToPlayer)
        {
            WaitForSeconds wait = new WaitForSeconds(0.5f);
            while (true)
            {
                yield return wait;
                GoToPlayer();
            }
        }
    }
    public override AIState RunCurrentState()
    {
        if (closeToPlayer)
            return peacefulState;
        else return this;
    }

    public void GoToPlayer()
    {
        Vector2 currentLoc = new Vector2(transform.position.x, transform.position.y);
        Vector2 direction = playerLoc - currentLoc;
        if (direction.sqrMagnitude > 4)
        {
            closeToPlayer = false;
            Logging.Info($"Squaremagnitude of distance to target is still pretty far away i'll keep moving");
            rb.linearVelocity = direction * stateHandler.livingBeing.Speed;
        }
        else closeToPlayer = true;
    }
}
