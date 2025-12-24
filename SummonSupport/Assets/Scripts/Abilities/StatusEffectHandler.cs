using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;

public class StatusEffectHandler : MonoBehaviour
{
    private LivingBeing livingBeing;
    private AIStateHandler stateHandler;
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
        }
    }
    public void HandleStatusEffect(StatusEffects status, bool add)
    {
        if (stateHandler == null) return;
        if (add)
        {
            switch (status.EffectType)
            {
                case StatusEffectType.Chilled:
                    {
                        stateHandler.navAgent.speed -= 5;
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
            }
        }
        if (!add)
        {
            switch (status.EffectType)
            {
                case StatusEffectType.Chilled:
                    {
                        stateHandler.navAgent.speed += 5;
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
            SufferedStatusEffects[status.EffectType] += 1;
            HandleStatusEffect(status, true);
            RemoveStatusEffect(status, 20f);
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

        AlterStatusEffectList(status, false); // ill also need to undo whatever damage was done (0___0)
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


}
