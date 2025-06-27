using UnityEngine;
using System;
using Unity.VisualScripting;

public static class DamageHandler
{

    // current limitations: only affects health and power.
    // doesnt use or recognize percentage based damage



    private static float AdjustValue(Ability ability, LivingBeing target, LivingBeing caster = null)
    {
        float value = ability.Value;

        if (value < 0) // if value is negative, and therefore a damage value
        {
            value = AdjustBasedOnAffinity(ability, caster, target);
            value = AdjustBasedOnArmor(value, target);
            HandleApplyAttribute(target, ability.Attribute, value);
        }
        return value;
    }


    private static float AdjustBasedOnAffinity(Ability ability, LivingBeing caster, LivingBeing target)
    {
        // change value based on Affinity;
        float value = ability.Value;
        Element element = ability.ElementType;
        float relevantAffinity = target.Affinities[ability.ElementType].Get();
        if (relevantAffinity > 0)
        {
            value += value * relevantAffinity / 100; //new value equals old value, plus old value times relevant affinity converted into percentage
        }

        return value;
    }

    private static float AdjustBasedOnArmor(float value, LivingBeing target)
    {
        // change value based Physical Resistance;

        PhysicalType physical = PhysicalType.Bludgeoning;
        float relevantResistance = target.PhysicalDict[physical].Get();
        if (relevantResistance > 0)
        {
            value += value * relevantResistance / 100;
            //new value equals old value, plus old value times relevant affinity converted into percentage
        }

        return value;
    }

    public static void AdjustForOverValue(LivingBeing target, AttributeType attributeTypeTempMax, float newValue, float currentValue, AttributeType attributeTypeMax = AttributeType.Overshield)
    {
        float max = target.GetAttribute(attributeTypeMax);
        float tempMax = target.GetAttribute(attributeTypeTempMax);
        //if (newValue > max) // if the newly calculated value (after recieving heal or damage) is greater than the  characters max
        //    return max;

        if (newValue < 0 && tempMax > 0) // if damage is being calculated and one has overshield
        {
            if (tempMax + newValue <= 0) // if the damage is more than the shield
            {
                target.SetAttribute(attributeTypeTempMax, 0); // set overshield to 0
            }
            else
                target.SetAttribute(attributeTypeTempMax, tempMax + newValue); // otherwise lower overshield value by the damage
        }
    }

    static void HandleApplyAttribute(LivingBeing target, AttributeType attributeType, float newValue)
    {
        switch (attributeType)
        {
            case AttributeType.CurrentHitpoints:
                AdjustForOverValue(target, AttributeType.MaxHitpoints, newValue, target.GetAttribute(AttributeType.CurrentHitpoints), AttributeType.Overshield);
                break;

            case AttributeType.CurrentPower:
                AdjustForOverValue(target, AttributeType.MaxPower, newValue, target.GetAttribute(AttributeType.CurrentPower), AttributeType.Overshield);
                break;

            default:
                ApplyNormalValue(target, attributeType, newValue);
                break;
        }
    }

    private static void ApplyNormalValue(LivingBeing target, AttributeType attributeType, float newValue)
    {
        target.SetAttribute(attributeType, target.GetAttribute(attributeType) + newValue);
    }





}

