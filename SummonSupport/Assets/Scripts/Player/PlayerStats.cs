using System;
using System.Collections.Generic;
using SummonSupportEvents;
using UnityEngine;

public class PlayerStats : LivingBeing
{
    [SerializeField] public int CurrentLevel { private set; get; } = 0;
    [SerializeField] public int CurrentXP { private set; get; } = 0;
    [SerializeField] public Dictionary<string, int> SlottedAbilities { private set; get; } = new Dictionary<string, int>(); //This will store the slot in which an ability is contained. the string is a placeholder until we decide the object type of an ability

    //public new void ChangeAttribute(AttributeType attributeType, int value, ValueType valueType = ValueType.Flat)
    //{
    //    base.ChangeAttribute(attributeType, value, valueType);
    //    List<AttributeType> movementAttributes = new List<AttributeType>(){AttributeType.MovementSpeed, AttributeType.DashBoost, AttributeType.DashCooldown,  AttributeType.DashDuration};
    //    if (movementAttributes.Contains(attributeType))
    //    {
    //        EventDeclarer.SpeedAttributeChanged?.Invoke(attributeType, value);
    //    }
    //}

    //public void ChangeMovementAttribute(AttributeType movementAttribute, float value)
    //{
    //    EventDeclarer.SpeedAttributeChanged?.Invoke(movementAttribute, value);
    //}
}
