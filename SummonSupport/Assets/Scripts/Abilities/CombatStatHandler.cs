using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;


public static class CombatStatHandler
{
    //public static CombatStatHandler Instance;

    // current limitations: only affects health and power.
    // doesnt use or recognize percentage based damage
    private static WaitForSeconds tickRate = new WaitForSeconds(1f);

    public static void HandleAllEvents(Ability ability, List<OnEventDo> eventList, LivingBeing caster, LivingBeing targetStats)
    {
        foreach (OnEventDo individualEvent in eventList)
        {
            HandleOnEventDo(ability, individualEvent, targetStats, caster);
        }
    }
    public static void HandleOnEventDo(Ability ability, OnEventDo onEvent, LivingBeing targetStats, LivingBeing caster) //TODO: This belongs in its own class!! Other Ability types will definitly use this!
    {
        switch (onEvent)
        {
            case OnEventDo.Nothing:
                break;
            case OnEventDo.Ability:
                break;
            case OnEventDo.Damage:
                if (ability.Attribute != AttributeType.None && ability.Value != 0)
                {
                    //targetStats.ChangeAttribute(ability.Attribute, ability.Value);
                    AdjustDamageValue(ability, targetStats, caster);
                }
                break;
            case OnEventDo.Heal:
                AdjustHealValue(ability.Value, targetStats);
                break;
            case OnEventDo.StatusEffect:
                if (ability.StatusEffects != null)
                {
                    foreach (StatusEffect statusEffect in ability.StatusEffects)
                    {
                        statusEffect.ApplyStatusEffect(targetStats.gameObject, caster);
                    }
                }
                break;
        }
    }

    public static float AdjustDamageValue(Ability ability, LivingBeing target, LivingBeing caster = null)
    {
        float damageValue = 0f;
        if (ability.ElementType != Element.None) damageValue = AdjustBasedOnAffinity(ability.ElementType, ability.Value, caster, target);
        if (ability.PhysicalType != PhysicalType.None) damageValue = AdjustBasedOnArmor(ability.PhysicalType, ability.Value, target);
        AdjustForOverValue(target, AttributeType.Overshield, AttributeType.MaxHitpoints, AttributeType.CurrentHitpoints, damageValue);

        return damageValue;
    }

    public static float AdjustHealValue(float healValue, LivingBeing target, LivingBeing caster = null)
    {
        AdjustForOverValue(target, AttributeType.Overshield, AttributeType.MaxHitpoints, AttributeType.CurrentHitpoints, healValue);

        return healValue;
    }
    public static float AdjustAndApplyTemp(Ability ability, float changeValue, float duration, LivingBeing target, LivingBeing caster = null)
    {
        changeValue = AdjustBasedOnAffinity(ability.ElementType, changeValue, caster, target);
        changeValue = AdjustBasedOnArmor(ability.PhysicalType, changeValue, target);
        ApplyTempValue(target, ability.Attribute, changeValue, duration);

        return changeValue;
    }

    public static float AdjustAndApplyDOT(PhysicalType physical, Element element, float damageValue, float duration, LivingBeing target, LivingBeing caster = null)
    {
        damageValue = AdjustBasedOnAffinity(element, -damageValue, caster, target); // damage value is opposite?
        damageValue = AdjustBasedOnArmor(physical, damageValue, target);
        HandleApplyDOT(target, AttributeType.CurrentHitpoints, damageValue, duration);

        return damageValue;
    }

    private static float AdjustBasedOnAffinity(Element element, float damageValue, LivingBeing caster, LivingBeing target)
    {
        // change value based on Affinity;
        Debug.Log($"element = {element}, damageValue = {damageValue}, target = {target}");

        float relevantAffinity = target.Affinities[element].Get();
        if (relevantAffinity > 0)
        {
            damageValue -= damageValue * relevantAffinity / 100; //new value equals old value, plus old value times relevant affinity converted into percentage
        }

        Logging.Info($"Based on affinity the damagebvalue has been changed to {damageValue}");

        return damageValue;
    }

    private static float AdjustBasedOnArmor(PhysicalType physical, float value, LivingBeing target)
    {
        // change value based Physical Resistance;

        float relevantResistance = target.PhysicalDict[physical].Get();
        if (relevantResistance > 0)
        {
            value -= value * relevantResistance / 100;
            //new value equals old value, plus old value times relevant affinity converted into percentage
        }

        return value;
    }

    public static void AdjustForOverValue(LivingBeing target, AttributeType attributeTypeTempMax, AttributeType attributeTypeMax, AttributeType typeCurrentValue, float changeValue)
    {
        float max = target.GetAttribute(attributeTypeMax);
        float tempMax = target.GetAttribute(attributeTypeTempMax);
        float currentValue = target.GetAttribute(typeCurrentValue);

        //if (newValue > max) // if the newly calculated value (after recieving heal or damage) is greater than the  characters max
        //    return max;

        if (changeValue < 0 && tempMax > 0) // if damage is being calculated and one has overshield
        {
            if (tempMax + changeValue <= 0) // if the damage is more than the shield
            {
                Logging.Info($"{target.name} has had {AttributeType.CurrentHitpoints} changed by {changeValue}. option 1");
                target.SetAttribute(AttributeType.CurrentHitpoints, currentValue + changeValue);

                target.SetAttribute(attributeTypeTempMax, 0); // set overshield to 0
            }
            else
                Logging.Info($"{target.name} has had {AttributeType.CurrentHitpoints} changed by {changeValue}. option  2");

            target.SetAttribute(attributeTypeTempMax, tempMax + changeValue); // otherwise lower overshield value by the damage
        }
        else
        {
            if (changeValue + currentValue > max)
            {
                Logging.Info($"{target.name} has had {AttributeType.CurrentHitpoints} changed by {changeValue}. option 3");
                target.SetAttribute(AttributeType.CurrentHitpoints, max);
            }

            else
            {
                target.SetAttribute(AttributeType.CurrentHitpoints, target.GetAttribute(AttributeType.CurrentHitpoints) + changeValue);
                Logging.Info($"{target.name} has had {AttributeType.CurrentHitpoints} changed by {changeValue}. option 4");

            }
        }
    }

    static void HandleApplyAttribute(LivingBeing target, AttributeType attributeType, float changeValue)
    {
        switch (attributeType)
        {
            case AttributeType.CurrentHitpoints:
                AdjustForOverValue(target, AttributeType.Overshield, AttributeType.MaxHitpoints, AttributeType.CurrentHitpoints, changeValue);
                break;

            case AttributeType.CurrentPower:
                AdjustForOverValue(target, AttributeType.PowerSurge, AttributeType.MaxPower, AttributeType.CurrentPower, changeValue);
                break;

            default:
                ApplyNormalValue(target, attributeType, changeValue);
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
    private static IEnumerator ResetTempAttribute(LivingBeing target, AttributeType attributeType, float changeValue, float duration)
    {

        float elapsed = 0f;

        while (elapsed < duration)
        {
            yield return tickRate;
            elapsed += 1f;
            Logging.Info($"waiting {elapsed}/{duration} seconds for temporary attribute to reset");
        }
        ApplyNormalValue(target, attributeType, -changeValue); // resets by adding the opposite of what was added before (which may have been negative)
    }



    public static IEnumerator ApplyAttributeRepeatedly(LivingBeing target, AttributeType attributeType, float changeValue, float duration)
    {
        AttributeType tempMax = AttributeType.None;
        AttributeType max = AttributeType.None;
        AttributeType current = AttributeType.None;


        float elapsed = 0f;
        if (attributeType == AttributeType.CurrentHitpoints)
        {
            tempMax = AttributeType.Overshield;
            max = AttributeType.MaxHitpoints;
            current = AttributeType.CurrentHitpoints;
        }
        if (attributeType == AttributeType.CurrentPower)
        {
            tempMax = AttributeType.PowerSurge;
            max = AttributeType.MaxPower;
            current = AttributeType.CurrentPower;
        }

        while (elapsed < duration)
        {
            if (tempMax != AttributeType.None)
            {
                ApplyNormalValue(target, attributeType, changeValue);
            }
            else
                AdjustForOverValue(target, tempMax, max, current, changeValue);

            yield return tickRate;
            elapsed += 1f;
            Logging.Info($"applying {elapsed}/{duration} ticks of {changeValue} {attributeType}");

        }
    }




}

