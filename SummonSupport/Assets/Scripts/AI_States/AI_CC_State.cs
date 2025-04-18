using UnityEngine;
using System.Collections.Generic;
using System;
using Unity.Mathematics;
using UnityEngine.InputSystem;


public class AI_CC_State : AIState
{
    private AIState peaceState;
    public bool sufferingCC = false;
    public Dictionary<string, Vector2> currentCCs = new Dictionary<string, Vector2>();
    private Rigidbody2D rb;

    private float duration = 2f;
    private float timeElapsed = 0f;

    public void Awake()
    {
        peaceState = GetComponent<AIPeacefulState>();
        rb = GetComponent<Rigidbody2D>();
    }
    public override AIState RunCurrentState()
    {
        if (currentCCs.Keys.Count != 0)
        {
            KnockInTheAir(); // will later have a switch for the different CCs
            return this;
        }
        else
        {
            rb.bodyType = RigidbodyType2D.Dynamic;
            return peaceState;
        }
    }

    public void RecieveCC(string CC, Vector2 cause)
    {
        currentCCs.Add(CC, cause);
    }

    private void KnockInTheAir()
    {
        Logging.Info("Knock in the air func is being called");
        sufferingCC = true;
        Vector2 causeLoc = currentCCs["KnockUp"];
        if (timeElapsed <= duration)
        {
            rb.linearDamping = 40;
            rb.AddForce(((Vector2)transform.position - causeLoc).normalized, ForceMode2D.Impulse);
            if (timeElapsed <= duration * .5)
                rb.AddForce(new Vector2(0, 2f), ForceMode2D.Impulse);
            if (timeElapsed > duration * .5 && timeElapsed <= .8 * duration) rb.AddForce(new Vector2(0, -2), ForceMode2D.Impulse);
            else rb.linearDamping = 50;
            timeElapsed += Time.deltaTime;
        }
        else
        {
            timeElapsed = 0f;
            currentCCs.Clear();
            rb.linearDamping = 10;
        }
    }


}
