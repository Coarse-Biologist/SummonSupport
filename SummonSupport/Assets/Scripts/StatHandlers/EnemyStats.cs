using UnityEngine;
using System.Collections.Generic;
using SummonSupportEvents;


public class EnemyStats : LivingBeing
{
    [SerializeField] public Dictionary<string, int> SlottedAbilities { private set; get; } = new Dictionary<string, int>(); //This will store the slot in which an ability is contained. the string is a placeholder until we decide the object type of an ability
    [SerializeField] public List<AlchemyLoot> Loot { private set; get; } = new List<AlchemyLoot>();

    public override void Die()
    {
        EventDeclarer.EnemyDefeated.Invoke(this);
        if (HasStatusEffect(StatusEffectType.ExplodeOnDeath)) ViciousDeathExplosion();
        //if(TryGetComponent(out UnityEngine.AI.NavMeshAgent navAgent))
        //{
        //    navAgent.ResetPath();
        //}
        //if(TryGetComponent(out AIStateHandler stateHandler))
        //{
        //    stateHandler.CancelInvoke("RunStateMachine");
        //}
        //transform.position = new Vector3 (99, 99, 99);
        Debug.Log("Delayed test death being called now");

        Invoke("DelayedTestDeath", .1f);
    }

    public override void HandleUIAttrDisplay(AttributeType attributeType, float newValue)
    {
        switch (attributeType)
        {
            case AttributeType.CurrentHitpoints:
                resourceBarInterface?.SetHealthBarValue(GetAttribute(attributeType));
                break;
            case AttributeType.CurrentPower:
                resourceBarInterface?.SetPowerBarValue(GetAttribute(attributeType));
                break;
            case AttributeType.MaxHitpoints:
                resourceBarInterface?.SetHealthBarMaxValue(GetAttribute(attributeType));
                break;
            case AttributeType.MaxPower:
                resourceBarInterface?.SetPowerBarMaxValue(GetAttribute(attributeType));
                break;
            default:
                break;

        }
    }

    private void DelayedTestDeath()
    {
        Debug.Log("Delayed test death happening now");
        Destroy(gameObject);
    }


}

