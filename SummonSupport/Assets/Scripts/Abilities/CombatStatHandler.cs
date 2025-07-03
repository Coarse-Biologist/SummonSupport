using UnityEngine;
using System;
using System.Collections;


public static class CombatStatHandler
{

    // current limitations: only affects health and power.
    // doesnt use or recognize percentage based damage
    private static WaitForSeconds tickRate = new WaitForSeconds(1f);



    public static float AdjustDamageValue(Element element, float damageValue, LivingBeing target, LivingBeing caster = null)
    {
        damageValue = AdjustBasedOnAffinity(element, -damageValue, caster, target);
        damageValue = AdjustBasedOnArmor(damageValue, target);
        HandleApplyAttribute(target, AttributeType.CurrentHitpoints, damageValue);

        return damageValue;
    }

    public static float AdjustHealValue(float healValue, LivingBeing target, LivingBeing caster = null)
    {
        //value = AdjustBasedOnAffinity(effectPackage, caster, target);
        //value = AdjustBasedOnArmor(value, target);
        HandleApplyAttribute(target, AttributeType.CurrentHitpoints, healValue);

        return healValue;
    }

    public static float AdjustAndApplyDOT(Element element, float damageValue, float duration, LivingBeing target, LivingBeing caster = null)
    {
        damageValue = AdjustBasedOnAffinity(element, -damageValue, caster, target);
        damageValue = AdjustBasedOnArmor(damageValue, target);
        HandleApplyDOT(target, AttributeType.CurrentHitpoints, damageValue, duration);

        return damageValue;
    }

    private static float AdjustBasedOnAffinity(Element element, float damageValue, LivingBeing caster, LivingBeing target)
    {
        // change value based on Affinity;

        float relevantAffinity = target.Affinities[element].Get();
        if (relevantAffinity > 0)
        {
            damageValue += damageValue * relevantAffinity / 100; //new value equals old value, plus old value times relevant affinity converted into percentage
        }

        Logging.Info($"Based on affinity the damagebvalue has been changed to {damageValue}");

        return damageValue;
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
        float currentHP = target.GetAttribute(AttributeType.CurrentHitpoints);
        //if (newValue > max) // if the newly calculated value (after recieving heal or damage) is greater than the  characters max
        //    return max;

        if (newValue < 0 && tempMax > 0) // if damage is being calculated and one has overshield
        {
            if (tempMax + newValue <= 0) // if the damage is more than the shield
            {
                Logging.Info($"{target.name} has had {AttributeType.CurrentHitpoints} changed by {newValue}. option 1");
                target.SetAttribute(AttributeType.CurrentHitpoints, currentHP + newValue);

                target.SetAttribute(attributeTypeTempMax, 0); // set overshield to 0
            }
            else
                Logging.Info($"{target.name} has had {AttributeType.CurrentHitpoints} changed by {newValue}. option  2");

            target.SetAttribute(attributeTypeTempMax, tempMax + newValue); // otherwise lower overshield value by the damage
        }
        else
        {
            if (newValue + currentHP > target.MaxHP)
            {
                Logging.Info($"{target.name} has had {AttributeType.CurrentHitpoints} changed by {newValue}. option 3");
                target.SetAttribute(AttributeType.CurrentHitpoints, target.MaxHP);
            }

            else
            {
                target.SetAttribute(AttributeType.CurrentHitpoints, target.GetAttribute(AttributeType.CurrentHitpoints) + newValue);
                Logging.Info($"{target.name} has had {AttributeType.CurrentHitpoints} changed by {newValue}. option 4");

            }
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

    private static void HandleApplyDOT(LivingBeing target, AttributeType attributeType, float newValue, float duration)
    {
        target.StartCoroutine(ApplyAttributeRepeatedly(target, attributeType, newValue, duration));

    }

    private static void ApplyNormalValue(LivingBeing target, AttributeType attributeType, float newValue)
    {
        Logging.Info($"{target.name} has had {attributeType} changed by {newValue}");
        target.SetAttribute(attributeType, target.GetAttribute(attributeType) + newValue);
    }

    public static void ApplyTempValue(LivingBeing target, AttributeType attributeType, float newValue, float duration)
    {
        Logging.Info($"{target.name} has had {attributeType} changed by {newValue}");

        target.SetAttribute(attributeType, target.GetAttribute(attributeType) + newValue);
        target.StartCoroutine(ResetTempAttribute(target, attributeType, newValue, duration));

    }
    private static IEnumerator ResetTempAttribute(LivingBeing target, AttributeType attributeType, float newValue, float duration)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            yield return tickRate;
            elapsed += 1f;
            Logging.Info($"waiting {elapsed}/{duration} seconds for temporary attribute to reset");
        }
        ApplyNormalValue(target, attributeType, -newValue); // resets by adding the opposite of what was added before (which may have been negative)
    }



    public static IEnumerator ApplyAttributeRepeatedly(LivingBeing target, AttributeType attributeType, float newValue, float duration)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            HandleApplyAttribute(target, attributeType, newValue);
            yield return tickRate;
            elapsed += 1f;
            Logging.Info($"applying {elapsed}/{duration} ticks of {newValue} {attributeType}");

        }
    }




}

