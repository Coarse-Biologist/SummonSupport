using System.Collections;
using UnityEngine;
using System;
using Unity.VisualScripting;

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
    [field: SerializeField] public float Value { get; protected set; } = 1f;
    [field: SerializeField] public ValueType ValueType { get; protected set; } = ValueType.Flat;
    [field: SerializeField] public Element ElementType { get; protected set; } = Element.None;
    private float runtimeDuration;


    public void ApplyStatusEffect(GameObject target, Vector2 sourcePosition)
    {
        if (!target.TryGetComponent(out LivingBeing livingBeing))
            return;

        ApplyStatusEffect(livingBeing, sourcePosition);
    }
    public void ApplyStatusEffect(LivingBeing livingBeing, Vector2 sourcePosition)
    {
        if (RenewStatusEffect(livingBeing))
            return;

        ChooseRightCoroutineToApplyStatusEffect(livingBeing, sourcePosition);
    }
    public void ApplyStatusEffect(LivingBeing livingBeing)
    {
        ApplyEffect(livingBeing, Value, ChangeRegeneration);
        Logging.Info($"{livingBeing.name} is being given {this} status effect with value of {Value}");
    }
    public void RemoveStatusEffect(LivingBeing livingBeing)
    {
        RemoveEffect(livingBeing, -Value, ChangeRegeneration);
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
                CoroutineManager.Instance.StartCustomCoroutine(ApplyForXSeconds(livingBeing, -Value, ChangeRegeneration));
                break;
            case StatusEffectType.AttributeIncreaseOverTime:
                CoroutineManager.Instance.StartCustomCoroutine(ApplyForXSeconds(livingBeing, Value, ChangeRegeneration));
                break;
            case StatusEffectType.AttributeReduction:
                CoroutineManager.Instance.StartCustomCoroutine(ApplyForXSeconds(livingBeing, -Value, ChangeAttribute));
                break;
            case StatusEffectType.AttributeIncrease:
                CoroutineManager.Instance.StartCustomCoroutine(ApplyForXSeconds(livingBeing, Value, ChangeAttribute));
                break;
            case StatusEffectType.KnockInTheAir:
                AI_CC_State ccState = livingBeing.gameObject.GetComponent<AI_CC_State>();
                if (ccState != null) ccState.RecieveCC(StatusEffectType.KnockInTheAir, sourcePosition);
                break;
        }
    }
    private void ApplyEffect(LivingBeing target, float value, Action<LivingBeing, float> action)
    {
        if (!target)
            return;

        StatusEffectInstance instance = new(this);
        target.activeStatusEffects.Add(Name, instance);
        action(target, value);
    }
    private void RemoveEffect(LivingBeing target, float value, Action<LivingBeing, float> action)
    {
        if (!target)
            return;

        target.activeStatusEffects.Remove(Name);
        action(target, value);
    }
    private IEnumerator ApplyForXSeconds(LivingBeing target, float value, Action<LivingBeing, float> action)
    {
        try
        {
            if (target)
            {
                target.activeStatusEffects.Add(Name, new StatusEffectInstance(this));
                action(target, value);
                yield return new WaitForSeconds(Duration);
            }
        }
        finally
        {
            if (target)
            {
                action(target, -value);
                target.activeStatusEffects.Remove(Name);
            }
        }
    }

    private void ChangeAttribute(LivingBeing target, float value)
    {
        Logging.Info($"{target.name} is having attribute changed due to {this} status effect with value of {Value}");

        target.ChangeAttribute(Attribute, value);
    }

    private void ChangeRegeneration(LivingBeing target, float value)
    {
        Logging.Info($"{target.name} is having attribute changed due to {this} status effect with value of {Value}");

        target.ChangeRegeneration(Attribute, value);
    }
}
