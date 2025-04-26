using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;
using Unity.Mathematics;
using UnityEngine.InputSystem;


public class AI_CC_State : AIState
{
    private AIState peaceState;
    public bool sufferingCC = false;
    public Dictionary<StatusEffectType, Vector2> currentCCs = new Dictionary<StatusEffectType, Vector2>();
    private Rigidbody2D rb;

    private float duration = 2f;
    private float timeElapsed = 0f;

    private bool beingKnockedInAir = false;

    public void Awake()
    {
        peaceState = GetComponent<AIPeacefulState>();
        rb = GetComponent<Rigidbody2D>();
    }
    public override AIState RunCurrentState()
    {
        if (currentCCs.Keys.Count != 0)
        {
            if (!beingKnockedInAir && currentCCs.TryGetValue(StatusEffectType.KnockInTheAir, out Vector2 source)) StartCoroutine(KnockRoutine()); // will later have a switch for the different CCs
            return this;
        }
        else
        {
            rb.bodyType = RigidbodyType2D.Dynamic;
            return peaceState;
        }
    }

    public void RecieveCC(StatusEffectType CC, Vector2 sourcePosition)
    {
        currentCCs.TryAdd(CC, sourcePosition);
    }

    private bool KnockInTheAir()
    {
        Logging.Info("Knock in the air func is being called");
        beingKnockedInAir = true;
        if(currentCCs.TryGetValue(StatusEffectType.KnockInTheAir, out Vector2 sourcePosition) && timeElapsed <= duration)
            {
                Logging.Info($"Time elapsed = {timeElapsed}. duration = {duration}");
                rb.linearDamping = 40;
                if (timeElapsed <= duration * .8)
                    rb.AddForce(((Vector2)transform.position - sourcePosition).normalized, ForceMode2D.Impulse);
                if (timeElapsed <= duration * .5)
                    rb.AddForce(new Vector2(0, 2f), ForceMode2D.Impulse);
                if (timeElapsed > duration * .5 && timeElapsed <= .8 * duration) rb.AddForce(new Vector2(0, -2), ForceMode2D.Impulse);
                return true;
            }
       else
        {
            beingKnockedInAir = false;
            timeElapsed = 0f;
            currentCCs.Remove(StatusEffectType.KnockInTheAir);
            rb.linearDamping = 10;
            return false;
        }
    }
    private IEnumerator KnockRoutine()
    {
        WaitForSeconds wait = new WaitForSeconds(0.1f);
        while (true)
        {
            yield return wait;
            KnockInTheAir();
            timeElapsed += .1f;
        }
    }


}
