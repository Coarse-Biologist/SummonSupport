    using Alchemy;
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
        [field: SerializeField] public string                   EffectName              { get; protected set; } = "Undefined";
        [field: SerializeField] public Sprite                   Icon                    { get; protected set; }
        [field: SerializeField] public StatusEffectType         Type                    { get; protected set; } = StatusEffectType.NoEffect;
        [field: SerializeField] public AttributeType            Attribute               { get; protected set; } = AttributeType.None;
        [field: SerializeField] public float                    Duration                { get; protected set; } = 5f;
        [field: SerializeField] public float                    TickRateSeconds         { get; protected set; } = .1f;
        [field: SerializeField] public int                      Value                   { get; protected set; } = 1;
        [field: SerializeField] public ValueType                ValueType               { get; protected set; } = ValueType.Flat;
        [field: SerializeField] public Element                  ElementType             { get; protected set; } = Element.None;
        private float runtimeDuration;
        

        public void ApplyStatusEffect(GameObject target)
        {
            if (!target.TryGetComponent(out LivingBeing livingBeing))
                return;
            if (RenewStatusEffect(livingBeing))
                return;

            ChooseRightCoroutineToApplyStatusEffect(livingBeing);
        }
        bool RenewStatusEffect(LivingBeing livingBeing)
        {
            if (livingBeing.activeStatusEffects.TryGetValue(EffectName, out StatusEffectInstance existingEffect))
            {
                existingEffect.Renew();
                return true;
            }
            else
                return false;
        }
        void ChooseRightCoroutineToApplyStatusEffect(LivingBeing livingBeing)
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
            }
        }

        private IEnumerator HandleOnce(LivingBeing target, int value, Action<LivingBeing, int> action)
        {
            target.activeStatusEffects.Add(EffectName, new StatusEffectInstance(this));
            try
            { 
                action(target, value);
                yield return new WaitForSeconds(Duration);
            }
            finally
            {
                target.activeStatusEffects.Remove(EffectName);
            }
        }
        private IEnumerator RepeatStatusEffect(LivingBeing target, int value, Action<LivingBeing, int> action)
        {
            StatusEffectInstance instance = new(this);
            target.activeStatusEffects.Add(EffectName, instance);
            int totalTicks = Mathf.FloorToInt(Duration / TickRateSeconds);
            try
            {
                while (instance.ticksDone < totalTicks)
                {
                    if (target == null)
                        yield break; // In case the target died while this was still running. If target died => GameObject does not longer exist.
                
                    action(target, value);
                    instance.ticksDone++;
                    yield return new WaitForSeconds(TickRateSeconds);
                }
            }
            finally
            {
                if (target != null) // Same thing here, make sure target is still alive if we want to access it.
                    target.activeStatusEffects.Remove(EffectName);
            }
        }

        private void AttributeChange(LivingBeing target, int value)
        {
            Logging.Info("Attribute: " + Attribute + "\nValue: " + Value);
            target.ChangeAttribute(Attribute, value);
        }

    }
