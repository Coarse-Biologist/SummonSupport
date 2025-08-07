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
    public Dictionary<StatusEffectType, LivingBeing> CCToCaster = new();
    public Dictionary<StatusEffectType, StatusEffects> typeToCC = new();

    private Rigidbody2D rb;
    WaitForSeconds wait = new WaitForSeconds(0.1f);
    private float knockDuration = 2f;
    private float timeElapsed = 0f;
    private bool beingKnockedInAir = false;
    public bool isCharmed { private set; get; } = false;
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
        if (CCToCaster.Count == 0)
        {
            return peaceState;
        }
        if (!beingKnockedInAir && typeToCC.Keys.Contains(StatusEffectType.KnockInTheAir))
        {
            StartCoroutine(KnockRoutine());
            return this;
        }
        if (!isCharmed && typeToCC.Keys.Contains(StatusEffectType.Charmed))
        {
            BecomeCharmed(typeToCC[StatusEffectType.Charmed].Duration);
            return peaceState;
        }
        if (!isMad && typeToCC.Keys.Contains(StatusEffectType.Madness))
        {
            BecomeMad(typeToCC[StatusEffectType.Madness].Duration);
            return peaceState;
        }

        return peaceState;

    }

    public void RecieveCC(StatusEffects CC, LivingBeing caster)
    {
        CCToCaster.TryAdd(CC.EffectType, caster);
        typeToCC.TryAdd(CC.EffectType, CC);
    }
    public void RemoveCC(StatusEffectType CC)
    {
        CCToCaster.Remove(CC);
        typeToCC.Remove(CC);
    }

    private bool KnockInTheAir()
    {
        beingKnockedInAir = true;
        if (CCToCaster.TryGetValue(StatusEffectType.KnockInTheAir, out LivingBeing caster) && timeElapsed <= knockDuration)
        {
            rb.linearDamping = 40;
            if (timeElapsed <= knockDuration * .8)
                rb.AddForce(((Vector2)transform.position - (Vector2)caster.transform.position).normalized, ForceMode2D.Impulse);
            if (timeElapsed <= knockDuration * .5)
                rb.AddForce(new Vector2(0, 2f), ForceMode2D.Impulse);
            if (timeElapsed > knockDuration * .5 && timeElapsed <= .8 * knockDuration) rb.AddForce(new Vector2(0, -2), ForceMode2D.Impulse);
            return true;
        }
        else
        {
            beingKnockedInAir = false;
            timeElapsed = 0f;
            RemoveCC(StatusEffectType.KnockInTheAir);
            rb.linearDamping = 10;
            rb.bodyType = RigidbodyType2D.Dynamic;
            return false;
        }
    }

    private void BecomeCharmed(float duration)
    {
        isCharmed = true;
        RemoveCC(StatusEffectType.Charmed);
        stateHandler.SetTargetMask(StatusEffectType.Charmed);
        Invoke("EndEffectCharmed", duration);
    }
    private void EndEffectCharmed()
    {
        stateHandler.SetTargetMask(StatusEffectType.None);
        isCharmed = false;
    }
    private void BecomeMad(float duration)
    {
        isMad = true;
        RemoveCC(StatusEffectType.Madness);
        stateHandler.SetTargetMask(StatusEffectType.Madness);
        Invoke("EndEffectMad", duration);
    }
    private void EndEffectMad()
    {
        stateHandler.SetTargetMask();
        isMad = false;
        //RemoveCC(StatusEffectType.Madness);
    }
    private IEnumerator KnockRoutine()
    {
        bool repeat = true;
        while (repeat)
        {
            yield return wait;
            repeat = KnockInTheAir();
            timeElapsed += .1f;
        }
    }


}
