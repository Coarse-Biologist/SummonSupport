using UnityEngine;
using System.Collections;
using System.Diagnostics;
using UnityEngine.InputSystem;
using Unity.VisualScripting;

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
                    foreach (StatusEffects status in package.StatusEffects)
                    {
                        target.AlterStatusEffectList(status.EffectType, true);
                        target.StartCoroutine(RemoveStatusEffect(target, status));
                    }
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
        UnityEngine.Debug.Log("Adjusting and applying tempchange");
        float changeValue = tempAttr.Value;
        float duration = tempAttr.Duration;
        if (tempAttr is TempAttrDecrease_AT) changeValue = -changeValue; // set to negative if it decreases
        if (tempAttr.ResourceAttribute == AttributeType.CurrentHitpoints || tempAttr.ResourceAttribute == AttributeType.MaxHitpoints)
        {
            if (ability.ElementTypes.Count > 0) changeValue = AdjustBasedOnAffinity(ability.ElementTypes[0], changeValue, caster, target);
            if (ability.PhysicalType != PhysicalType.None) changeValue = AdjustBasedOnArmor(ability.PhysicalType, changeValue, target);
        }
        UnityEngine.Debug.Log($"the change value = {changeValue}. tempAttr.PhysicalResistance = {tempAttr.ElementalAffinity}");

        if (tempAttr.ResourceAttribute != AttributeType.None) ApplyTempValue(ability, target, tempAttr.ResourceAttribute, changeValue, duration);
        if (tempAttr.PhysicalResistance != PhysicalType.None) ApplyTempValue(ability, target, tempAttr.PhysicalResistance, changeValue, duration);
        if (tempAttr.ElementalAffinity != Element.None) ApplyTempValue(ability, target, tempAttr.ElementalAffinity, changeValue, duration);
        if (tempAttr.MovementAttribute != MovementAttributes.None) ApplyTempValue(ability, target, tempAttr.MovementAttribute, changeValue, duration);

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
                target.SetAttribute(AttributeType.CurrentHitpoints, currentValue + changeValue - tempMax);

                target.SetAttribute(attributeTypeTempMax, 0); // set overshield to 0
            }
            else
                target.SetAttribute(attributeTypeTempMax, tempMax + changeValue); // otherwise lower overshield value by the damage
        }
        else
        {
            if (changeValue + currentValue >= max)
            {
                target.SetAttribute(AttributeType.CurrentHitpoints, max);
            }

            else
            {
                target.SetAttribute(AttributeType.CurrentHitpoints, target.GetAttribute(AttributeType.CurrentHitpoints) + changeValue);
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
            }
        }
        target.ChangeRegeneration(attributeType, -newValue);
        target.AlterAbilityList(ability, false);

    }

    #region apply already adjusted values, temporarily, repeatedly.
    private static void ApplyValue(LivingBeing target, AttributeType attributeType, float newValue)
    {
        //Logging.Info($"{target.name} has had {attributeType} changed by {newValue}");
        target.SetAttribute(attributeType, target.GetAttribute(attributeType) + newValue);
    }
    #region multiple temp value overrides
    public static void ApplyTempValue(Ability ability, LivingBeing target, AttributeType attributeType, float newValue, float duration)
    {
        target.SetAttribute(attributeType, target.GetAttribute(attributeType) + newValue);
        target.StartCoroutine(ResetTempAttribute(ability, target, attributeType, newValue, duration));
    }
    #region temp movement change

    public static void ApplyTempValue(Ability ability, LivingBeing target, MovementAttributes movementAttribute, float newValue, float duration)
    {
        //Logging.Info($"{target.name} has had {attributeType} changed by {newValue}");
        if (target.TryGetComponent<MovementScript>(out MovementScript movementScript))
        {
            movementScript.SetMovementAttribute(movementAttribute, movementScript.GetMovementAttribute(movementAttribute) + newValue);
            movementScript.StartCoroutine(ResetTempMovementAttribute(ability, target, movementScript, movementAttribute, newValue, duration));
        }
    }

    private static IEnumerator ResetTempMovementAttribute(Ability ability, LivingBeing target, MovementScript movementScript, MovementAttributes movementAttribute, float preChangeValue, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)// && target.AffectedByAbilities.Contains(ability))
        {
            yield return tickRate;
            if (!target.IsAffectedByAbility(ability))
            {
                elapsed += tickRateFloat;
            }
        }
        movementScript.SetMovementAttribute(movementAttribute, preChangeValue);
        target.AlterAbilityList(ability, false);
    }
    #endregion

    #region apply temp physical resistance

    public static void ApplyTempValue(Ability ability, LivingBeing target, PhysicalType physicalType, float newValue, float duration)
    {
        Logging.Info($"{target.name} has had {physicalType} changed by {newValue}");
        target.SetPhysicalResist(physicalType, target.PhysicalDict[physicalType].Get() + newValue);
        target.StartCoroutine(ResetTempPhysicalResist(ability, target, physicalType, newValue, duration));
    }

    private static IEnumerator ResetTempPhysicalResist(Ability ability, LivingBeing target, PhysicalType physicalType, float preChangeValue, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)// && target.AffectedByAbilities.Contains(ability))
        {
            yield return tickRate;
            if (!target.IsAffectedByAbility(ability))
            {
                elapsed += tickRateFloat;
            }
        }
        target.SetPhysicalResist(physicalType, preChangeValue);
        target.AlterAbilityList(ability, false);
    }
    #endregion

    #region apply temp affinity

    public static void ApplyTempValue(Ability ability, LivingBeing target, Element affinity, float changeValue, float duration)
    {
        Logging.Info($"{target.name} has had {affinity} changed by {changeValue}");
        float preAffinityValue = target.Affinities[affinity].Get();
        target.ChangeAffinity(affinity, changeValue);
        target.StartCoroutine(ResetTempAffinity(ability, target, affinity, preAffinityValue, duration));
    }

    private static IEnumerator ResetTempAffinity(Ability ability, LivingBeing target, Element elementalAffinity, float preChangeValue, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)// && target.AffectedByAbilities.Contains(ability))
        {
            yield return tickRate;
            if (!target.IsAffectedByAbility(ability))
            {
                elapsed += tickRateFloat;
            }
        }
        Logging.Info($"{target.name} has had {elementalAffinity} changed back to {preChangeValue}");

        target.SetAffinity(elementalAffinity, preChangeValue);

        target.AlterAbilityList(ability, false);
    }

    #endregion

    #region remove temp status effect

    private static IEnumerator RemoveStatusEffect(LivingBeing target, StatusEffects status)
    {
        yield return new WaitForSeconds(status.Duration);
        target.AlterStatusEffectList(status.EffectType, false);

    }
    #endregion

    private static IEnumerator ResetTempAttribute(Ability ability, LivingBeing target, AttributeType attributeType, float changeValue, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)// && target.AffectedByAbilities.Contains(ability))
        {
            yield return tickRate;
            if (!target.IsAffectedByAbility(ability))
            {
                elapsed += tickRateFloat;
            }
        }
        ApplyValue(target, attributeType, -changeValue); // resets by adding the opposite of what was added before (which may have been negative)
        target.AlterAbilityList(ability, false);
    }
    #endregion

    #endregion


}


//public static void AdjustForOverValue(LivingBeing target, AttributeType attributeTypeTempMax, AttributeType attributeTypeMax, AttributeType typeCurrentValue, float changeValue)

//can be for power or health.
// the question:
// how much current and over points should the target have?