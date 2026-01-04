using System;
using System.Collections;
using System.Collections.Generic;
using SummonSupportEvents;
using UnityEngine;
using UnityEngine.Animations;

public class StatusEffectHandler : MonoBehaviour
{
    private LivingBeing livingBeing;
    private AIStateHandler stateHandler;
    public Rigidbody rigidBody;
    private float ai_Speed = 5;
    public Dictionary<StatusEffectType, int> SufferedStatusEffects { get; private set; } = new();

    void Awake()
    {
        InitializeStatusEffectDict();
    }
    void Start()
    {
        if (gameObject.TryGetComponent(out LivingBeing liv))
        {
            livingBeing = liv;
        }
        if (gameObject.TryGetComponent(out AIStateHandler ai))
        {
            stateHandler = ai;
            ai_Speed = stateHandler.navAgent.speed;
        }
        if (gameObject.TryGetComponent(out Rigidbody rb))
        {
            rigidBody = rb;
        }
    }
    public void HandleStatusEffect(StatusEffects status, bool add)
    {
        Debug.Log($"Adding {status.EffectType} = {true}");

        if (stateHandler == null) return;
        if (add)
        {
            switch (status.EffectType)
            {
                case StatusEffectType.Chilled:
                    {
                        stateHandler.navAgent.speed -= .2f * ai_Speed;
                        stateHandler.anim.anim.speed -= .2f;
                        if (GetStatusEffectValue(StatusEffectType.Chilled) > 4)
                        {
                            stateHandler.navAgent.speed = 0;
                            stateHandler.anim.anim.speed = 0;
                            EventDeclarer.FrozenSolid?.Invoke(livingBeing);
                        }
                        break;
                    }
                case StatusEffectType.Overheated:
                    {
                        EventDeclarer.Overheating?.Invoke(livingBeing);
                        break;
                    }
                case StatusEffectType.Charmed:
                    {
                        stateHandler.SetTargetMask(StatusEffectType.Charmed);
                        break;
                    }
                case StatusEffectType.KnockBack:
                    {
                        KnockInTheAir();
                        break;
                    }
                case StatusEffectType.Madness:
                    {
                        stateHandler.SetTargetMask(StatusEffectType.Madness);
                        break;
                    }
                case StatusEffectType.Pulled:
                    {
                        if (GetStatusEffectValue(StatusEffectType.Pulled) > 0)
                        {
                            EventDeclarer.GraspingVines?.Invoke(livingBeing);
                        }
                        break;
                    }
                case StatusEffectType.Infected:
                    {
                        if (GetStatusEffectValue(StatusEffectType.Infected) > 4)
                        {
                            Debug.Log($"spreading virus from {livingBeing}");

                            EventDeclarer.SpreadVirus?.Invoke(livingBeing);
                        }
                        break;
                    }
            }
        }
        if (!add)
        {
            switch (status.EffectType)
            {
                case StatusEffectType.Chilled:
                    {
                        Debug.Log($"Increasing speed by {.2f * ai_Speed}");
                        stateHandler.anim.anim.speed += .2f;
                        stateHandler.navAgent.speed += .2f * ai_Speed;
                        break;
                    }
                case StatusEffectType.Charmed:
                    {
                        stateHandler.SetTargetMask(StatusEffectType.Charmed);
                        break;
                    }
                case StatusEffectType.Madness:
                    {
                        stateHandler.SetTargetMask(StatusEffectType.Madness);
                        break;
                    }
                case StatusEffectType.Poisoned:
                    stateHandler.SetStateMachineSpeed(GetStatusEffectValue(StatusEffectType.Poisoned));
                    break;


            }
        }
    }
    public void AlterStatusEffectList(StatusEffects status, bool Add) // modifies the list of abilities by which one is affected
    {
        if (Add)
        {
            if (livingBeing is EnemyStats enemyStats)
            {
                enemyStats.AddStatusEffectSymbol(status, 1);
            }
            SufferedStatusEffects[status.EffectType] += 1;
            HandleStatusEffect(status, true);
            StartCoroutine(RemoveStatusEffect(status, 2 * status.Duration));
        }
        if (!Add)
        {
            SufferedStatusEffects[status.EffectType] -= 1;
            HandleStatusEffect(status, false);
        }

    }
    private IEnumerator RemoveStatusEffect(StatusEffects status, float duration)
    {
        yield return new WaitForSeconds(duration);

        AlterStatusEffectList(status, false);
    }
    public bool HasStatusEffect(StatusEffectType status)
    {
        return SufferedStatusEffects[status] > 0;
    }

    public int GetStatusEffectValue(StatusEffectType status)
    {
        int statusValue = SufferedStatusEffects[status];

        return statusValue;// SufferedStatusEffects[status];
    }

    void InitializeStatusEffectDict()
    {
        SufferedStatusEffects = new Dictionary<StatusEffectType, int>();

        foreach (StatusEffectType effect in Enum.GetValues(typeof(StatusEffectType)))
        {
            SufferedStatusEffects[effect] = 0;
        }
    }
    private void KnockInTheAir()
    {
        Debug.Log($"knocking in the air");

        rigidBody.AddForce(-transform.forward * GetStatusEffectValue(StatusEffectType.KnockBack) * 20, ForceMode.Impulse);
    }


}
