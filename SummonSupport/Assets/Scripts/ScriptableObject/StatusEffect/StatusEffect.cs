using System.Collections;
using UnityEngine;
using System;

public class StatusEffectInstance
{
    public StatusEffect effect;
    public int ticksDone;

    public StatusEffectInstance(StatusEffect effect)
    {
        this.effect = effect;
        ticksDone = 0;
    }
    public void Renew()
    {
        ticksDone = 0;
    }
}

[CreateAssetMenu(menuName = "Status Effects/Status Effect")]
public class StatusEffect : ScriptableObject
{
    [field: Header("Status Effect")]
    [field: SerializeField] public string Name { get; protected set; } = "Undefined";
    [field: SerializeField] public Sprite Icon { get; protected set; }
    [field: SerializeField] public StatusEffectType Type { get; protected set; } = StatusEffectType.NoEffect;
    [field: SerializeField] public AttributeType Attribute { get; protected set; } = AttributeType.None;
    [field: SerializeField] public float Duration { get; protected set; } = 5f;
    [field: SerializeField] public float TickRateSeconds { get; protected set; } = .1f;
    [field: SerializeField] public int Value { get; protected set; } = 1;
    [field: SerializeField] public ValueType ValueType { get; protected set; } = ValueType.Flat;
    [field: SerializeField] public Element ElementType { get; protected set; } = Element.None;
    private float runtimeDuration;


    public void ApplyStatusEffect(GameObject target, Vector2 sourcePosition)
    {
        if (!target.TryGetComponent(out LivingBeing livingBeing))
            return;
        if (RenewStatusEffect(livingBeing))
            return;

        ChooseRightCoroutineToApplyStatusEffect(livingBeing, sourcePosition);
    }
    bool RenewStatusEffect(LivingBeing livingBeing)
    {
        if (livingBeing.activeStatusEffects.TryGetValue(Name, out StatusEffectInstance existingEffect))
        {
            existingEffect.Renew();
            return true;
        }
        else
            return false;
    }
    void ChooseRightCoroutineToApplyStatusEffect(LivingBeing livingBeing, Vector2 sourcePosition)
    {
        switch (Type)
        {
            case StatusEffectType.AttributeReductionOverTime:
                CoroutineManager.Instance.StartCustomCoroutine(RepeatStatusEffect(livingBeing, -Value, AttributeChange));
                break;
            case StatusEffectType.AttributeIncreaseOverTime:
                CoroutineManager.Instance.StartCustomCoroutine(RepeatStatusEffect(livingBeing, Value, AttributeChange));
                break;
            case StatusEffectType.AttributeReduction:
                CoroutineManager.Instance.StartCustomCoroutine(HandleOnce(livingBeing, -Value, AttributeChange));
                break;
            case StatusEffectType.AttributeIncrease:
                CoroutineManager.Instance.StartCustomCoroutine(HandleOnce(livingBeing, Value, AttributeChange));
                break;
            case StatusEffectType.KnockInTheAir:
                AI_CC_State ccState = livingBeing.gameObject.GetComponent<AI_CC_State>();
                if (ccState != null) ccState.RecieveCC(StatusEffectType.KnockInTheAir, sourcePosition);
                break;
        }
    }

    private IEnumerator HandleOnce(LivingBeing target, int value, Action<LivingBeing, int> action)
    {
        try
        {
            if (IsExisting(target))
            {
                target.activeStatusEffects.Add(Name, new StatusEffectInstance(this));
                action(target, value);
                yield return new WaitForSeconds(Duration);
            }
        }
        finally
        {
            if (IsExisting(target))
            {
                action(target, -value);
                target.activeStatusEffects.Remove(Name);
            }
        }
    }
    private IEnumerator RepeatStatusEffect(LivingBeing target, int value, Action<LivingBeing, int> action)
    {
        StatusEffectInstance instance = new(this);
        int totalTicks = Mathf.FloorToInt(Duration / TickRateSeconds);
        target.activeStatusEffects.Add(Name, instance);
        try
        {
            while (instance.ticksDone < totalTicks)
            {
                if (!IsExisting(target))
                    yield break; // In case the target died while this was still running. If target died => GameObject does not longer exist.

                action(target, value);
                instance.ticksDone++;
                yield return new WaitForSeconds(TickRateSeconds);
            }
        }
        finally
        {
            if (IsExisting(target)) // Same thing here, make sure target is still alive if we want to access it.
                target.activeStatusEffects.Remove(Name);
        }
    }

    private void AttributeChange(LivingBeing target, int value)
    {
        target.ChangeAttribute(Attribute, value);
    }

    private bool IsExisting(LivingBeing livingBeing)
    {
        return livingBeing != null && livingBeing.gameObject != null && livingBeing.gameObject.activeInHierarchy == true;
    }
}
