using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEditor.Rendering;
using System.Runtime.InteropServices;
using System.Linq;
using System;
using UnityEngine.Splines.Interpolators;



public class AI_CC_State : AIState
{
    private AIState peaceState;
    private AIChaseState chaseState;

    private AIStateHandler stateHandler;
    public bool sufferingCC = false;
    public Dictionary<StatusEffectType, LivingBeing> CCToCaster = new();
    public Dictionary<StatusEffectType, StatusEffects> typeToCC = new();


    private Rigidbody rb;
    private float waitTime = .1f;
    WaitForSeconds wait;// = new WaitForSeconds(0.1f);
    private float knockDuration = 2f;
    private float timeElapsed = 0f;
    private bool beingKnockedInAir = false;
    public bool isCharmed { private set; get; } = false;
    public bool isMad { private set; get; } = false;
    public bool beingPulled { private set; get; } = false;

    public Vector3 PullEpicenter { private set; get; } = new Vector3(-1, -1, -1);



    public void Awake()
    {
        wait = new WaitForSeconds(waitTime);
        peaceState = GetComponent<AIPeacefulState>();
        stateHandler = GetComponent<AIStateHandler>();
        chaseState = GetComponent<AIChaseState>();
        rb = GetComponent<Rigidbody>();
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
        if (!beingPulled && typeToCC.Keys.Contains(StatusEffectType.Pulled))
        {
            StartCoroutine(PullRoutine());
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
        if (typeToCC.Keys.Contains(StatusEffectType.AttackAnimation))
        {
            return this;
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
    public void SetPullEpicenter(Vector3 loc)
    {
        PullEpicenter = loc;
    }

    private bool KnockInTheAir()
    {
        Debug.Log("Doing knock in the air");
        beingKnockedInAir = true;
        if (CCToCaster.TryGetValue(StatusEffectType.KnockInTheAir, out LivingBeing caster) && timeElapsed <= knockDuration)
        {
            rb.AddForce((transform.position - caster.transform.position).normalized * 20, ForceMode.Impulse);

            return true;
        }
        else
        {
            beingKnockedInAir = false;
            timeElapsed = 0f;
            RemoveCC(StatusEffectType.KnockInTheAir);
            return false;
        }
    }
    private IEnumerator PullRoutine()
    {
        float pullDuration = typeToCC[StatusEffectType.Pulled].Duration;
        beingPulled = true;
        if (PullEpicenter == Vector3.zero) PullEpicenter = new Vector3(transform.position.x, transform.position.y - 2);
        bool stillPulling = true;
        while (stillPulling)
        {
            yield return wait;
            pullDuration -= waitTime;
            transform.position = Vector3.MoveTowards(transform.position, PullEpicenter, .1f);
            if (pullDuration <= 0) //((Vector3)transform.position - PullEpicenter).sqrMagnitude < .3f )
            {
                stillPulling = false;
                RemoveCC(StatusEffectType.Pulled);
                PullEpicenter = Vector3.down;
                beingPulled = false;
            }
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
