using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEditor.Rendering;
using System.Runtime.InteropServices;
using System.Linq;



public class AI_CC_State : AIState
{
    private AIState peaceState;
    private AIState chaseState;

    private AIStateHandler stateHandler;
    public bool sufferingCC = false;
    public Dictionary<StatusEffectType, Vector2> currentCCs = new Dictionary<StatusEffectType, Vector2>();
    private Rigidbody2D rb;

    private float duration = 2f;
    private float timeElapsed = 0f;

    private bool beingKnockedInAir = false;
    public bool isMad { private set; get; } = false;

    public void Awake()
    {
        peaceState = GetComponent<AIPeacefulState>();
        stateHandler = GetComponent<AIStateHandler>();
        chaseState = GetComponent<AIChaseState>();
        rb = GetComponent<Rigidbody2D>();
    }
    public override AIState RunCurrentState()
    {
        if (currentCCs.Count == 0)
        {
            return peaceState;
        }
        if (!beingKnockedInAir && currentCCs.Keys.Contains(StatusEffectType.KnockInTheAir))
        {
            StartCoroutine(KnockRoutine());
            return this;
        }
        if (!isMad && currentCCs.Keys.Contains(StatusEffectType.Madness))
        {
            BecomeMad(10f);
            return peaceState;
        }

        return peaceState;

    }

    public void RecieveCC(StatusEffects CC, LivingBeing caster)
    {
        currentCCs.TryAdd(CC.EffectType, caster.transform.position);
    }

    private bool KnockInTheAir()
    {
        beingKnockedInAir = true;
        if (currentCCs.TryGetValue(StatusEffectType.KnockInTheAir, out Vector2 sourcePosition) && timeElapsed <= duration)
        {
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
            rb.bodyType = RigidbodyType2D.Dynamic;
            return false;
        }
    }

    private void BecomeMad(float duration)
    {
        isMad = true;
        currentCCs.Remove(StatusEffectType.Madness);
        stateHandler.SetTargetMask(true);
        Invoke("RestoreSanity", duration);
    }
    private void RestoreSanity()
    {
        stateHandler.SetTargetMask(false);
        isMad = false;
        currentCCs.Remove(StatusEffectType.Madness);


    }
    private IEnumerator KnockRoutine()
    {
        bool repeat = true;
        WaitForSeconds wait = new WaitForSeconds(0.1f);
        while (repeat)
        {
            yield return wait;
            repeat = KnockInTheAir();
            timeElapsed += .1f;
        }
    }


}
