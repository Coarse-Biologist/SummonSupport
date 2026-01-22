using UnityEngine;
using System.Collections;
using System.Diagnostics;
using UnityEngine.InputSystem;
using Unity.VisualScripting;
using System.Collections.Generic;
using SummonSupportEvents;
using UnityEngine.ResourceManagement.Profiling;
using UnityEditor.Rendering;
using UnityEditor.ShaderGraph.Internal;
using System;
using System.Linq;

public static class CombatStatHandler
{
    //public static CombatStatHandler Instance;

    // doesnt use or recognize percentage based damage
    private static float AffinityDamageScalar = 100f; // relevant caster affinity will be divided by this to modify damage. max affinity doubles elemental damage
    private static float MinDamageScalar = .4f; //minimum modifier. once 40 affinity is surpassed, affinity scaling will increase.
    private static float tickRateFloat = 1f;
    private static WaitForSeconds tickRate = new WaitForSeconds(tickRateFloat);
    private static AbilityModHandler modHandler;
    private static Mod_Base mod;
    private static LivingBeing currentTarget;
    private static LivingBeing currentCaster;
    private static Ability currentAbility;
    private static List<StatusEffects> currentStatusEffects = new();
    private static float currentDotValue;
    private static float currentHealOverTimeValue;
    private static float currentDamageValue;
    private static float currentHealValue;
    private static float currentAbilityDuration;










    public static void HandleEffectPackage(Ability ability, LivingBeing caster, LivingBeing target, EffectPackage effectPackage)
    {
        currentAbility = ability;
        currentCaster = caster;
        currentTarget = target;
        if (caster is not EnemyStats) modHandler = AbilityModHandler.Instance;
        else modHandler = null;
        SetCurrentValues(effectPackage);
        UnityEngine.Debug.Log($"caster = {caster.Name}, target = {target.Name} mod handler = {modHandler}");
        AddMods();

        AdjustandApplyHealValue();

        AdjustAndApplyDOT();

        HandleApplyDOT(AttributeType.CurrentHitpoints, currentHealOverTimeValue);

        AdjustDamageValue(effectPackage.Damage, effectPackage.SpecialAbilityAttribute);

        HandleApplyStatusEffects();

        if (effectPackage.AttributeUp.Count > 0)
        {
            foreach (TempAttrIncrease_AT tempChange in effectPackage.AttributeUp)
            {
                AdjustAndApplyTempChange(tempChange);
            }
        }

        if (effectPackage.AttributeDown.Count > 0)
        {
            foreach (TempAttrDecrease_AT tempChange in effectPackage.AttributeDown)
            {
                AdjustAndApplyTempChange(tempChange);
            }
        }

    }

    private static void HandleApplyStatusEffects()
    {
        if (currentStatusEffects.Count > 0)
        {
            foreach (StatusEffects status in currentStatusEffects)
            {
                currentTarget.SE_Handler.AlterStatusEffectList(status, true);
            }
        }
    }
    private static void SetCurrentValues(EffectPackage currentAbilityEffects)
    {

        currentStatusEffects = currentAbilityEffects.StatusEffects;

        currentDamageValue = currentAbilityEffects.Damage.Value;
        currentDotValue = currentAbilityEffects.DamageOverTime.Value;
        currentHealValue = currentAbilityEffects.Heal.Value;
        currentHealOverTimeValue = currentAbilityEffects.HealOverTime.Value;
        currentAbilityDuration = currentAbility.Duration;
    }

    public static void AddMods()
    {
        if (modHandler != null)
        {
            foreach (StatusEffects se in modHandler.GetModStatusEffects(currentAbility))
            {
                currentStatusEffects.Add(se);
            }

            currentDamageValue += modHandler.GetModAttributeByType(currentAbility, AbilityModTypes.Damage); // damage increase from mods
            currentDotValue += modHandler.GetModAttributeByType(currentAbility, AbilityModTypes.DamageOverTime); // DOT increase from mods

            currentHealValue += modHandler.GetModAttributeByType(currentAbility, AbilityModTypes.Heal); // heal increase from mods
            currentHealOverTimeValue += modHandler.GetModAttributeByType(currentAbility, AbilityModTypes.HealOverTime); // HOT increase from mods


            currentAbilityDuration += modHandler.GetModAttributeByType(currentAbility, AbilityModTypes.Duration);
        }
    }


    #region Adjust and apply damage, heal, temp attributes and damage over times
    public static float AdjustDamageValue(Damage_AT damage_AT, SpecialAbilityAttribute specialAbilityAttribute = SpecialAbilityAttribute.None)
    {
        if (currentDamageValue > 0)
        {
            EventDeclarer.ShakeCamera?.Invoke(1f);
            float damageValue = GetDamageByType(damage_AT);
            damageValue = ModifyDamageValueByCasterAffinity(damageValue);


            foreach (Element element in currentAbility.ElementTypes)
            {
                if (element != Element.None) damageValue = AdjustBasedOnAffinity(element, damageValue);

            }
            if (currentAbility.PhysicalType != PhysicalType.None)
            {
                damageValue = AdjustBasedOnArmor(currentAbility.PhysicalType, damageValue);
            }
            AdjustForOverValue(AttributeType.Overshield, AttributeType.MaxHitpoints, AttributeType.CurrentHitpoints, -damageValue);
            if (specialAbilityAttribute == SpecialAbilityAttribute.Syphon)
                AdjustForOverValue(AttributeType.Overshield, AttributeType.MaxHitpoints, AttributeType.CurrentHitpoints, damageValue);

            return damageValue;
        }
        else return 0;
    }
    private static float GetDamageByType(Damage_AT damage_AT)
    {
        if (damage_AT.ValueType == ValueType.Flat) return currentDamageValue;
        if (damage_AT.ValueType == ValueType.Percentage) return currentDamageValue / 100f * currentTarget.GetAttribute(AttributeType.MaxHitpoints);
        else return currentDamageValue * currentTarget.GetAttribute(AttributeType.MaxHitpoints) / (currentTarget.GetAttribute(AttributeType.CurrentHitpoints) * 2);
    }
    public static float AdjustandApplyHealValue()
    {
        if (currentHealValue > 0)
        {
            float newHP = Mathf.Min(currentHealValue + currentTarget.GetAttribute(AttributeType.CurrentHitpoints), currentTarget.GetAttribute(AttributeType.MaxHitpoints));
            currentTarget.SetAttribute(AttributeType.CurrentHitpoints, newHP);
            //UnityEngine.Debug.Log($"healing {target.Name} for {healValue}");
            return newHP;
        }
        else return 0;

    }
    public static float AdjustAndApplyTempChange(TempAttrChange tempAttr)
    {
        UnityEngine.Debug.Log("Adjusting and applying tempchange");
        float changeValue = tempAttr.Value;
        if (tempAttr is TempAttrDecrease_AT) changeValue = -changeValue; // set to negative if it decreases
        if (tempAttr.ResourceAttribute == AttributeType.CurrentHitpoints || tempAttr.ResourceAttribute == AttributeType.MaxHitpoints)
        {
            if (currentAbility.ElementTypes.Count > 0) changeValue = AdjustBasedOnAffinity(currentAbility.ElementTypes[0], changeValue);
            if (currentAbility.PhysicalType != PhysicalType.None) changeValue = AdjustBasedOnArmor(currentAbility.PhysicalType, changeValue);
        }
        UnityEngine.Debug.Log($"the change value = {changeValue}. tempAttr.PhysicalResistance = {tempAttr.ElementalAffinity}");

        if (tempAttr.ResourceAttribute != AttributeType.None) ApplyTempValue(tempAttr.ResourceAttribute, changeValue, currentAbilityDuration);
        if (tempAttr.PhysicalResistance != PhysicalType.None) ApplyTempValue(tempAttr.PhysicalResistance, changeValue, currentAbilityDuration);
        if (tempAttr.ElementalAffinity != Element.None) ApplyTempValue(tempAttr.ElementalAffinity, changeValue, currentAbilityDuration);
        if (tempAttr.MovementAttribute != MovementAttributes.None) ApplyTempValue(tempAttr.MovementAttribute, changeValue, currentAbilityDuration);

        //target.StartCoroutine(ResetTempAttribute(target, tempAttr.AttributeType, -changeValue, duration)); //reset by using opposite

        return changeValue;
    }
    public static float AdjustAndApplyDOT()
    {
        if (currentDotValue == 0) return 0;

        float damageValue = -currentDotValue;
        UnityEngine.Debug.Log($"Applying DOT of value {damageValue}");
        damageValue = ModifyDamageValueByCasterAffinity(damageValue);

        foreach (Element element in currentAbility.ElementTypes)
        {
            PhysicalType physical = currentAbility.PhysicalType;
            if (element != Element.None) damageValue = AdjustBasedOnAffinity(element, damageValue); // damage value is opposite?
            if (currentAbility.PhysicalType != PhysicalType.None) damageValue = AdjustBasedOnArmor(physical, damageValue);
        }
        HandleApplyDOT(AttributeType.CurrentHitpoints, damageValue);

        return damageValue;
    }

    #endregion

    #region modify resistable damages
    private static float AdjustBasedOnAffinity(Element element, float damageValue)
    {
        // change value based on Affinity;
        //UnityEngine.Debug.Log($"element = {element}, damageValue = {damageValue}, target = {target}");

        float relevantAffinity = currentTarget.Affinities[element].Get();
        if (relevantAffinity > 0)
        {
            damageValue -= damageValue * relevantAffinity / 100; //new value equals old value, plus old value times relevant affinity converted into percentage
        }

        //Logging.Info($"Based on affinity the damagebvalue has been changed to {damageValue}");

        return damageValue;
    }

    private static float AdjustBasedOnArmor(PhysicalType physical, float value)
    {
        // change value based Physical Resistance;

        float relevantResistance = currentTarget.PhysicalDict[physical].Get();
        relevantResistance -= currentTarget.SE_Handler.GetStatusEffectValue(StatusEffectType.Dissolving) * 25; // magic number for effect of acid status effect where does it belog?
        if (relevantResistance > 0)
        {
            value -= value * relevantResistance / 100;
            //new value equals old value, plus old value times relevant affinity converted into percentage
        }

        return value;
    }
    private static float ModifyDamageValueByCasterAffinity(float changeValue)
    {
        if (currentAbility.ElementTypes.Count == 0) return changeValue;
        else
        {
            float damageModifier = Math.Max(MinDamageScalar, currentCaster.GetAffinity(currentAbility.ElementTypes[0]) / AffinityDamageScalar);
            return changeValue *= damageModifier;
        }
    }
    public static void AdjustForOverValue(AttributeType attributeTypeTempMax, AttributeType attributeTypeMax, AttributeType typeCurrentValue, float changeValue)
    {
        // skipping until we actually have mechanism to show overhealth.

        float max = currentTarget.GetAttribute(attributeTypeMax);
        float tempMax = currentTarget.GetAttribute(attributeTypeTempMax);
        float currentValue = currentTarget.GetAttribute(typeCurrentValue);

        //if (newValue > max) // if the newly calculated value (after recieving heal or damage) is greater than the  characters max
        //    return max;

        if (changeValue < 0 && tempMax > 0) // if damage is being calculated and one has overshield
        {
            if (tempMax + changeValue <= 0) // if the damage is more than the shield
            {
                currentTarget.SetAttribute(AttributeType.CurrentHitpoints, currentValue + changeValue - tempMax);

                currentTarget.SetAttribute(attributeTypeTempMax, 0); // set overshield to 0
            }
            else
                currentTarget.SetAttribute(attributeTypeTempMax, tempMax + changeValue); // otherwise lower overshield value by the damage
        }
        else
        {
            if (changeValue + currentValue >= max)
            {
                currentTarget.SetAttribute(AttributeType.CurrentHitpoints, max);
            }

            else
            {
                currentTarget.SetAttribute(AttributeType.CurrentHitpoints, currentTarget.GetAttribute(AttributeType.CurrentHitpoints) + changeValue);
            }
        }
    }

    #endregion

    private static void HandleApplyDOT(AttributeType attributeType, float changeValue)
    {
        if (changeValue != 0)
        {
            UnityEngine.Debug.Log($"Applying DOT of value {changeValue}to attribute {attributeType}");
            currentTarget.ChangeRegeneration(attributeType, changeValue);
            currentTarget.StartCoroutine(ResetRegeneration(currentAbility, currentTarget, attributeType, changeValue, currentAbilityDuration));
        }
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
    private static void ApplyValue(AttributeType attributeType, float newValue)
    {
        //Logging.Info($"{target.name} has had {attributeType} changed by {newValue}");
        currentTarget.SetAttribute(attributeType, currentTarget.GetAttribute(attributeType) + newValue);
    }
    #region multiple temp value overrides
    public static void ApplyTempValue(AttributeType attributeType, float newValue, float duration)
    {
        currentTarget.SetAttribute(attributeType, currentTarget.GetAttribute(attributeType) + newValue);
        currentTarget.StartCoroutine(ResetTempAttribute(attributeType, -newValue, duration));
    }
    #region temp movement change

    public static void ApplyTempValue(MovementAttributes movementAttribute, float tempValue, float duration)
    {
        //Logging.Info($"{currentTarget.name} has had {movementAttribute} changed by {tempValue}");
        if (currentTarget.TryGetComponent<MovementScript>(out MovementScript movementScript))
        {
            float preChangeValue = movementScript.GetMovementAttribute(movementAttribute);
            movementScript.StartCoroutine(ResetTempMovementAttribute(movementScript, movementAttribute, preChangeValue, duration));
            movementScript.SetMovementAttribute(movementAttribute, preChangeValue + tempValue);
        }
    }

    private static IEnumerator ResetTempMovementAttribute(MovementScript movementScript, MovementAttributes movementAttribute, float preChangeValue, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)// && target.AffectedByAbilities.Contains(ability))
        {
            yield return tickRate;
            if (!currentTarget.IsAffectedByAbility(currentAbility))
            {
                elapsed += tickRateFloat;
            }
        }
        movementScript.SetMovementAttribute(movementAttribute, preChangeValue);
        currentTarget.AlterAbilityList(currentAbility, false);
    }
    #endregion

    #region apply temp physical resistance

    public static void ApplyTempValue(PhysicalType physicalType, float newValue, float duration)
    {
        //Logging.Info($"{currentTarget.name} has had {physicalType} changed by {newValue}");
        currentTarget.SetPhysicalResist(physicalType, currentTarget.PhysicalDict[physicalType].Get() + newValue);
        currentTarget.StartCoroutine(ResetTempPhysicalResist(physicalType, newValue, duration));
    }

    private static IEnumerator ResetTempPhysicalResist(PhysicalType physicalType, float preChangeValue, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)// && target.AffectedByAbilities.Contains(ability))
        {
            yield return tickRate;
            if (!currentTarget.IsAffectedByAbility(currentAbility))
            {
                elapsed += tickRateFloat;
            }
        }
        currentTarget.SetPhysicalResist(physicalType, preChangeValue);
        currentTarget.AlterAbilityList(currentAbility, false);
    }
    #endregion

    #region apply temp affinity

    public static void ApplyTempValue(Element affinity, float changeValue, float duration)
    {
        //Logging.Info($"{currentTarget.name} has had {affinity} changed by {changeValue}");
        float preAffinityValue = currentTarget.Affinities[affinity].Get();
        currentTarget.ChangeAffinity(affinity, changeValue);
        currentTarget.StartCoroutine(ResetTempAffinity(affinity, preAffinityValue, duration));
    }

    private static IEnumerator ResetTempAffinity(Element elementalAffinity, float preChangeValue, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)// && target.AffectedByAbilities.Contains(ability))
        {
            yield return tickRate;
            if (!currentTarget.IsAffectedByAbility(currentAbility))
            {
                elapsed += tickRateFloat;
            }
        }
        //Logging.Info($"{currentTarget.name} has had {elementalAffinity} changed back to {preChangeValue}");

        currentTarget.SetAffinity(elementalAffinity, preChangeValue);

        currentTarget.AlterAbilityList(currentAbility, false);
    }

    #endregion


    private static IEnumerator ResetTempAttribute(AttributeType attributeType, float changeValue, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)// && target.AffectedByAbilities.Contains(ability))
        {
            yield return tickRate;
            if (!currentTarget.IsAffectedByAbility(currentAbility))
            {
                elapsed += tickRateFloat;
            }
        }
        UnityEngine.Debug.Log($"attribute type {attributeType} is being reset by adding {changeValue}");
        ApplyValue(attributeType, changeValue); // resets by adding the opposite of what was added before (which may have been negative)
        currentTarget.AlterAbilityList(currentAbility, false);
    }
    #endregion

    #endregion


}

