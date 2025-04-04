using Alchemy;
using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "Status Effect")]
public class StatusEffect : ScriptableObject
{   
    [field: Header("Status Effect")] 
    [field: SerializeField] public string                   EffectName              { get; protected set; } = "Undefined";
    [field: SerializeField] public StatusEffectType         Type                    { get; protected set; } = StatusEffectType.NoEffect;
    [field: SerializeField] public RessourceType            Ressource               { get; protected set; } = RessourceType.None;
    [field: SerializeField] public AttributeType            Attribute               { get; protected set; } = AttributeType.None;
    [field: SerializeField] public float                    Duration                { get; protected set; } = 5f;
    [field: SerializeField] public float                    TickRateSeconds         { get; protected set; } = .1f;
    [field: SerializeField] public int                      Value                   { get; protected set; } = 1;
    [field: SerializeField] public ValueType                ValueType               { get; protected set; } = ValueType.Flat;
    [field: SerializeField] public Elements                 ElementType             { get; protected set; } = Elements.None;

    public void ApplyStatusEffect(GameObject target)
    {
        LivingBeing livingBeing = target.GetComponent<LivingBeing>();
        if (livingBeing == null)
            return;        

        switch (Type)
        {
        case StatusEffectType.DrainRessourceOverTime:
            CoroutineManager.Instance.StartCustomCoroutine(DrainRessourceOverTime(livingBeing));
            break;
        case StatusEffectType.AttributeReduction:
            livingBeing.ChangeAttribute(Attribute, -Value, ValueType);
            break;
        case StatusEffectType.AttributeIncrease:
            livingBeing.ChangeAttribute(Attribute, Value, ValueType);
            break;
        }
    }

    private IEnumerator DrainRessourceOverTime(LivingBeing target)
    {
        float timePassed = 0f;
        
        while (timePassed < Duration)
        {
            Logging.Info("Attribute: " + Attribute + "\nValue: " + Value);
            target.ChangeAttribute(Attribute, -Value);
            yield return new WaitForSeconds(TickRateSeconds);
            timePassed += TickRateSeconds;
        }
    }
}
