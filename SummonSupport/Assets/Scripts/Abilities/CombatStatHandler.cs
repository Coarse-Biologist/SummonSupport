using UnityEngine;
using System.Collections;
using System.Linq;

public static class CombatStatHandler
{
    //public static CombatStatHandler Instance;

    // doesnt use or recognize percentage based damage
    private static float tickRateFloat = 1f;
    private static WaitForSeconds tickRate = new WaitForSeconds(tickRateFloat);



    public static void HandleEffectPackages(Ability ability, LivingBeing caster, LivingBeing target, bool forSelf = false)
    {
        Debug.Log($"{ability} effects are being processed. Caster = {caster}, target = {target}");
        LivingBeing casterStats = caster.GetComponent<LivingBeing>();
        LivingBeing targetStats = target.GetComponent<LivingBeing>();
        LivingBeing theTarget = casterStats;
        if (!forSelf) theTarget = targetStats;
        Debug.Log($"the target of {ability} shall be {theTarget}");

        foreach (EffectPackage package in ability.TargetTypeAndEffects)
        {
            if (forSelf && package.TargetType == TargetType.Self || !forSelf && package.TargetType == TargetType.Target)
            {
                if (package.Heal.Value > 0) AdjustHealValue(package.Heal.Value, theTarget, casterStats);
                if (package.HealOverTime.Value > 0) HandleApplyDOT(target, AttributeType.CurrentHitpoints, package.HealOverTime.Value, package.HealOverTime.Duration);

                if (package.Damage.Count > 0)
                {
                    foreach (Damage_AT damage in package.Damage)
                    {
                        AdjustDamageValue(damage, theTarget, casterStats);
                    }
                }
                if (package.DamageOverTime.Count > 0)
                {
                    foreach (DamageoT_AT damage in package.DamageOverTime)
                    {
                        AdjustAndApplyDOT(damage, theTarget, casterStats);
                    }
                }
                if (package.AttributeUp.Count > 0)
                {
                    foreach (TempAttrIncrease_AT tempChange in package.AttributeUp)
                    {
                        AdjustAndApplyTempChange(tempChange, theTarget, casterStats);
                    }
                }
                if (package.AttributeDown.Count > 0)
                {
                    foreach (TempAttrDecrease_AT tempChange in package.AttributeDown)
                    {
                        AdjustAndApplyTempChange(tempChange, theTarget, casterStats);
                    }
                }
            }

        }
    }

    #region Adjust and apply damage, heal, temo attributes and damage over times
    public static float AdjustDamageValue(Damage_AT damage_AT, LivingBeing target, LivingBeing caster = null)
    {
        float damageValue = 0f;
        if (damage_AT.Element != Element.None) damageValue = AdjustBasedOnAffinity(damage_AT.Element, damage_AT.Value, caster, target);
        if (damage_AT.Physical != PhysicalType.None) damageValue = AdjustBasedOnArmor(damage_AT.Physical, damage_AT.Value, target);
        AdjustForOverValue(target, AttributeType.Overshield, AttributeType.MaxHitpoints, AttributeType.CurrentHitpoints, -damageValue);

        return damageValue;
    }
    public static float AdjustHealValue(float healValue, LivingBeing target, LivingBeing caster = null)
    {
        AdjustForOverValue(target, AttributeType.Overshield, AttributeType.MaxHitpoints, AttributeType.CurrentHitpoints, healValue);

        return healValue;
    }
    public static float AdjustAndApplyTempChange(TempAttrChange tempAttr, LivingBeing target, LivingBeing caster = null)
    {
        float changeValue = tempAttr.Value;
        float duration = tempAttr.Duration;
        Element element = tempAttr.Element;
        PhysicalType physical = tempAttr.Physical;
        if (tempAttr is TempAttrDecrease_AT) changeValue = -changeValue; // set to negative if it decreases
        if (tempAttr.AttributeType == AttributeType.CurrentHitpoints || tempAttr.AttributeType == AttributeType.MaxHitpoints)
        {
            if (element != Element.None) changeValue = AdjustBasedOnAffinity(element, changeValue, caster, target);
            if (physical != PhysicalType.None) changeValue = AdjustBasedOnArmor(physical, changeValue, target);
        }
        ApplyTempValue(target, tempAttr.AttributeType, changeValue, duration);
        //target.StartCoroutine(ResetTempAttribute(target, tempAttr.AttributeType, -changeValue, duration)); //reset by using opposite

        return changeValue;
    }
    public static float AdjustAndApplyDOT(DamageoT_AT damageOT, LivingBeing target, LivingBeing caster = null)
    {
        float damageValue = -damageOT.Value;
        float duration = damageOT.Duration;
        Element element = damageOT.Element;
        PhysicalType physical = damageOT.Physical;
        if (damageOT.Element != Element.None) damageValue = AdjustBasedOnAffinity(element, damageValue, caster, target); // damage value is opposite?
        if (damageOT.Physical != PhysicalType.None) damageValue = AdjustBasedOnArmor(physical, damageValue, target);
        HandleApplyDOT(target, AttributeType.CurrentHitpoints, damageValue, duration);

        return damageValue;
    }
    #endregion

    #region modify resistable damages
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
            if (changeValue + currentValue >= max)
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

    #endregion

    private static void HandleApplyDOT(LivingBeing target, AttributeType attributeType, float newValue, float duration)
    {
        Debug.Log($"Changing {target}s regen by {newValue}");
        target.ChangeRegeneration(attributeType, newValue);
        target.StartCoroutine(ResetRegeneration(target, attributeType, newValue, duration));
    }
    private static IEnumerator ResetRegeneration(LivingBeing target, AttributeType attributeType, float newValue, float duration)
    {
        WaitForSeconds OverTimeReset = new WaitForSeconds(duration);
        float elapsed = 0f;
        while (elapsed < duration)
        {
            yield return OverTimeReset;
        }
        target.ChangeRegeneration(attributeType, -newValue);


    }

    #region apply already adjusted values, temporrily, repeatedly.
    private static void ApplyValue(LivingBeing target, AttributeType attributeType, float newValue)
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
            elapsed += tickRateFloat;
            Logging.Info($"waiting {elapsed}/{duration} seconds for temporary attribute to reset");
        }
        ApplyValue(target, attributeType, -changeValue); // resets by adding the opposite of what was added before (which may have been negative)
    }


    #endregion

}


//static void HandleApplyAttribute(LivingBeing target, AttributeType attributeType, float changeValue)
//    switch (attributeType)
//    {
//        case AttributeType.CurrentHitpoints:
//            AdjustForOverValue(target, AttributeType.Overshield, AttributeType.MaxHitpoints, AttributeType.CurrentHitpoints, changeValue);
//            break;
//
//        case AttributeType.CurrentPower:
//            AdjustForOverValue(target, AttributeType.PowerSurge, AttributeType.MaxPower, AttributeType.CurrentPower, changeValue);
//            break;
//
//        default:
//            ApplyNormalValue(target, attributeType, changeValue);
//            break;
//    }
//}




// CalculateNewValueByType(float currentValue, float value, ValueType valueType, AttributeType attributeType)
//{
//    float newValue = valueType == ValueType.Percentage
//    ? currentValue * (1 + value / 100f)
//    : currentValue + value;
//    //DamageHandler.AdjustValue();
//
//    return HandleAttributeCap(attributeType, newValue, currentValue, value);
//}
//
//float HandleAttributeCap(AttributeType attributeType, float newValue, float currentValue, float delta)
//{
//    switch (attributeType)
//    {
//        case AttributeType.CurrentHitpoints:
//            newValue = ApplyCap(AttributeType.MaxHitpoints, AttributeType.Overshield, newValue, currentValue, delta);
//            break;
//
//        case AttributeType.CurrentPower:
//            newValue = ApplyCap(AttributeType.MaxPower, AttributeType.PowerSurge, newValue, currentValue, delta);
//            break;
//    }
//    return newValue;
//}
//
//float ApplyCap(AttributeType attributeTypeMax, AttributeType attributeTypeTempMax, float newValue, float currentValue, float delta)
//{
//    float max = GetAttribute(attributeTypeMax);
//    float tempMax = GetAttribute(attributeTypeTempMax);
//    if (newValue > max) // if the newly calculated value (after recieving heal or damage) is greater than the  characters max
//        return max;
//
//    if (delta < 0 && tempMax > 0) // if damage is being calculated and one has overshield
//    {
//        if (tempMax + delta <= 0) // if the damage is more than the shield
//        {
//            SetAttribute(attributeTypeTempMax, 0); // set overshield to 0
//            return currentValue + tempMax + delta; // return current health plus overshield value, minus damage value,
//        }
//        else
//            SetAttribute(attributeTypeTempMax, tempMax + delta); // otherwise lower overshield value by the damage
//        return currentValue; // return current health or mana
//    }
//    return newValue;
//}

//publicic static IEnumerator ApplyAttributeRepeatedly(LivingBeing target, AttributeType attributeType, float changeValue, float duration)
//{
//    AttributeType tempMax = AttributeType.None;
//    AttributeType max = AttributeType.None;
//    AttributeType current = AttributeType.None;
//
//
//    float elapsed = 0f;
//    if (attributeType == AttributeType.CurrentHitpoints)
//    {
//        tempMax = AttributeType.Overshield;
//        max = AttributeType.MaxHitpoints;
//        current = AttributeType.CurrentHitpoints;
//    }
//    if (attributeType == AttributeType.CurrentPower)
//    {
//        tempMax = AttributeType.PowerSurge;
//        max = AttributeType.MaxPower;
//        current = AttributeType.CurrentPower;
//    }
//    while (elapsed < duration)
//    {
//        if (tempMax != AttributeType.None)
//        {
//            ApplyValue(target, attributeType, changeValue);
//        }
//        else
//            AdjustForOverValue(target, tempMax, max, current, changeValue);
//
//        yield return tickRate;
//        elapsed += 1f;
//        Logging.Info($"applying {elapsed}/{duration} ticks of {changeValue} {attributeType}");
//    }
//}