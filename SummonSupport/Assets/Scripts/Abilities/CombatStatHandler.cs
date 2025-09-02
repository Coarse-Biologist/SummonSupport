using UnityEngine;
using System.Collections;
using System.Diagnostics;
using UnityEngine.InputSystem;

public static class CombatStatHandler
{
    //public static CombatStatHandler Instance;

    // doesnt use or recognize percentage based damage
    private static float tickRateFloat = 1f;
    private static WaitForSeconds tickRate = new WaitForSeconds(tickRateFloat);



    public static void HandleEffectPackages(Ability ability, LivingBeing caster, LivingBeing target, bool forSelf = false)
    {
        Stopwatch stopwatch = new();
        //UnityEngine.Debug.Log($"ability = {ability.name}. caster = {caster.Name}. target = {target.Name}. for self? {forSelf}");
        LivingBeing casterStats = caster.GetComponent<LivingBeing>();
        LivingBeing targetStats = target.GetComponent<LivingBeing>();
        LivingBeing theTarget = casterStats;
        if (!forSelf) theTarget = targetStats;


        foreach (EffectPackage package in ability.TargetTypeAndEffects)
        {
            if (forSelf && package.TargetType == TargetType.Self || !forSelf && package.TargetType == TargetType.Target)
            {

                if (package.Heal.Value > 0) AdjustHealValue(package.Heal.Value, theTarget, casterStats);
                if (package.HealOverTime.Value > 0) HandleApplyDOT(ability, target, AttributeType.CurrentHitpoints, package.HealOverTime.Value, package.HealOverTime.Duration);

                if (package.Damage.Count > 0)
                {
                    foreach (Damage_AT damage in package.Damage)
                    {
                        AdjustDamageValue(damage, theTarget, casterStats, package.SpecialAbilityAttribute);
                    }
                }
                if (package.DamageOverTime.Count > 0)
                {
                    foreach (DamageoT_AT damage in package.DamageOverTime)
                    {
                        AdjustAndApplyDOT(ability, damage, theTarget, casterStats);
                    }
                }
                if (package.AttributeUp.Count > 0)
                {
                    foreach (TempAttrIncrease_AT tempChange in package.AttributeUp)
                    {
                        AdjustAndApplyTempChange(ability, tempChange, theTarget, casterStats);
                    }
                }
                if (package.AttributeDown.Count > 0)
                {
                    foreach (TempAttrDecrease_AT tempChange in package.AttributeDown)
                    {
                        AdjustAndApplyTempChange(ability, tempChange, theTarget, casterStats);
                    }
                }

                if (package.StatusEffects.Count > 0)
                {
                    if (target.TryGetComponent<AI_CC_State>(out AI_CC_State ccState))
                    {
                        foreach (StatusEffects status in package.StatusEffects)
                        {
                            ccState.RecieveCC(status, casterStats);
                            if (status.EffectType == StatusEffectType.Pulled) ccState.SetPullEpicenter(Camera.main.ScreenToWorldPoint(Input.mousePosition));

                        }
                    }
                }
            }

        }
        //UnityEngine.Debug.Log($"elapsed time in the combat handler package handler function: {stopwatch.ElapsedMilliseconds}");
    }

    #region Adjust and apply damage, heal, temo attributes and damage over times
    public static float AdjustDamageValue(Damage_AT damage_AT, LivingBeing target, LivingBeing caster = null, SpecialAbilityAttribute specialAbilityAttribute = SpecialAbilityAttribute.None)
    {
        float damageValue = GetDamageByType(damage_AT, target);

        if (damage_AT.Element != Element.None) damageValue = AdjustBasedOnAffinity(damage_AT.Element, damageValue, caster, target);
        if (damage_AT.Physical != PhysicalType.None) damageValue = AdjustBasedOnArmor(damage_AT.Physical, damageValue, target);
        AdjustForOverValue(target, AttributeType.Overshield, AttributeType.MaxHitpoints, AttributeType.CurrentHitpoints, -damageValue);
        if (specialAbilityAttribute == SpecialAbilityAttribute.Syphon)
            AdjustForOverValue(target, AttributeType.Overshield, AttributeType.MaxHitpoints, AttributeType.CurrentHitpoints, damageValue);

        return damageValue;
    }
    private static float GetDamageByType(Damage_AT damage_AT, LivingBeing target)
    {
        if (damage_AT.ValueType == ValueType.Flat) return damage_AT.Value;
        if (damage_AT.ValueType == ValueType.Percentage) return damage_AT.Value / 100f * target.GetAttribute(AttributeType.MaxHitpoints);
        else return damage_AT.Value * target.GetAttribute(AttributeType.MaxHitpoints) / (target.GetAttribute(AttributeType.CurrentHitpoints) * 2);
    }
    public static float AdjustHealValue(float healValue, LivingBeing target, LivingBeing caster = null)
    {
        float newHP = Mathf.Min(healValue + target.GetAttribute(AttributeType.CurrentHitpoints), target.GetAttribute(AttributeType.MaxHitpoints));
        target.SetAttribute(AttributeType.CurrentHitpoints, newHP);
        //UnityEngine.Debug.Log($"healing {target.Name} for {healValue}");
        return healValue;
    }
    public static float AdjustAndApplyTempChange(Ability ability, TempAttrChange tempAttr, LivingBeing target, LivingBeing caster = null)
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
        ApplyTempValue(ability, target, tempAttr.AttributeType, changeValue, duration);
        //target.StartCoroutine(ResetTempAttribute(target, tempAttr.AttributeType, -changeValue, duration)); //reset by using opposite

        return changeValue;
    }
    public static float AdjustAndApplyDOT(Ability ability, DamageoT_AT damageOT, LivingBeing target, LivingBeing caster = null)
    {
        float damageValue = -damageOT.Value;
        float duration = damageOT.Duration;
        Element element = damageOT.Element;
        PhysicalType physical = damageOT.Physical;
        if (damageOT.Element != Element.None) damageValue = AdjustBasedOnAffinity(element, damageValue, caster, target); // damage value is opposite?
        if (damageOT.Physical != PhysicalType.None) damageValue = AdjustBasedOnArmor(physical, damageValue, target);
        HandleApplyDOT(ability, target, AttributeType.CurrentHitpoints, damageValue, duration);

        return damageValue;
    }
    #endregion

    #region modify resistable damages
    private static float AdjustBasedOnAffinity(Element element, float damageValue, LivingBeing caster, LivingBeing target)
    {
        // change value based on Affinity;
        //UnityEngine.Debug.Log($"element = {element}, damageValue = {damageValue}, target = {target}");

        float relevantAffinity = target.Affinities[element].Get();
        if (relevantAffinity > 0)
        {
            damageValue -= damageValue * relevantAffinity / 100; //new value equals old value, plus old value times relevant affinity converted into percentage
        }

        //Logging.Info($"Based on affinity the damagebvalue has been changed to {damageValue}");

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
        // skipping until we actually have mechanism to show overhealth.

        float max = target.GetAttribute(attributeTypeMax);
        float tempMax = target.GetAttribute(attributeTypeTempMax);
        float currentValue = target.GetAttribute(typeCurrentValue);

        //if (newValue > max) // if the newly calculated value (after recieving heal or damage) is greater than the  characters max
        //    return max;

        if (changeValue < 0 && tempMax > 0) // if damage is being calculated and one has overshield
        {
            if (tempMax + changeValue <= 0) // if the damage is more than the shield
            {
                //Logging.Info($"{target.name} has had {AttributeType.CurrentHitpoints} changed by {changeValue}. option 1");
                target.SetAttribute(AttributeType.CurrentHitpoints, currentValue + changeValue - tempMax);

                target.SetAttribute(attributeTypeTempMax, 0); // set overshield to 0
            }
            else
                //Logging.Info($"{target.name} has had {AttributeType.CurrentHitpoints} changed by {changeValue}. option  2");

                target.SetAttribute(attributeTypeTempMax, tempMax + changeValue); // otherwise lower overshield value by the damage
        }
        else
        {
            if (changeValue + currentValue >= max)
            {
                //Logging.Info($"{target.name} has had {AttributeType.CurrentHitpoints} changed by {changeValue}. option 3");
                target.SetAttribute(AttributeType.CurrentHitpoints, max);
            }

            else
            {
                target.SetAttribute(AttributeType.CurrentHitpoints, target.GetAttribute(AttributeType.CurrentHitpoints) + changeValue);
                //Logging.Info($"{target.name} has had {AttributeType.CurrentHitpoints} changed by {changeValue}. option 4");

            }
        }
    }

    #endregion

    private static void HandleApplyDOT(Ability ability, LivingBeing target, AttributeType attributeType, float newValue, float duration)
    {
        //UnityEngine.Debug.Log($"Changing {target}s regen by {newValue}");
        target.ChangeRegeneration(attributeType, newValue);
        target.StartCoroutine(ResetRegeneration(ability, target, attributeType, newValue, duration));
    }
    private static IEnumerator ResetRegeneration(Ability ability, LivingBeing target, AttributeType attributeType, float newValue, float duration)
    {
        WaitForSeconds OverTimeReset = new WaitForSeconds(duration);
        float elapsed = 0f;
        while (elapsed < duration)
        {
            yield return tickRate;
            if (!target.IsAffectedByAbility(ability))
            {
                elapsed += tickRateFloat;
                UnityEngine.Debug.Log($"increasing elapsedtime because {target.Name} has left the {ability.Name}");
            }
            else UnityEngine.Debug.Log($"NOT increasing elapsedtime because {target.Name} is still within the {ability.Name}");
            //if (!target.AffectedByAbilities.Contains(ability)) elapsed = duration;
        }
        target.ChangeRegeneration(attributeType, -newValue);
        target.AlterAbilityList(ability, false);

    }

    #region apply already adjusted values, temporrily, repeatedly.
    private static void ApplyValue(LivingBeing target, AttributeType attributeType, float newValue)
    {
        //Logging.Info($"{target.name} has had {attributeType} changed by {newValue}");
        target.SetAttribute(attributeType, target.GetAttribute(attributeType) + newValue);
    }

    public static void ApplyTempValue(Ability ability, LivingBeing target, AttributeType attributeType, float newValue, float duration)
    {
        //Logging.Info($"{target.name} has had {attributeType} changed by {newValue}");

        target.SetAttribute(attributeType, target.GetAttribute(attributeType) + newValue);
        target.StartCoroutine(ResetTempAttribute(ability, target, attributeType, newValue, duration));

    }
    private static IEnumerator ResetTempAttribute(Ability ability, LivingBeing target, AttributeType attributeType, float changeValue, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)// && target.AffectedByAbilities.Contains(ability))
        {
            yield return tickRate;
            if (!target.IsAffectedByAbility(ability))
            {
                elapsed += tickRateFloat;
                UnityEngine.Debug.Log($"increasing elapsedtime because {target.Name} has left the {ability.Name}");
            }
            else UnityEngine.Debug.Log($"NOT increasing elapsedtime because {target.Name} is still within the {ability.Name}");

        }
        UnityEngine.Debug.Log($"Stopping effect {ability.Name} because duration without being directly affected by the ability has ended");
        ApplyValue(target, attributeType, -changeValue); // resets by adding the opposite of what was added before (which may have been negative)
        target.AlterAbilityList(ability, false);

    }


    #endregion

}


//public static void AdjustForOverValue(LivingBeing target, AttributeType attributeTypeTempMax, AttributeType attributeTypeMax, AttributeType typeCurrentValue, float changeValue)

//can be for power or health.
// the question:
// how much current and over points should the target have?