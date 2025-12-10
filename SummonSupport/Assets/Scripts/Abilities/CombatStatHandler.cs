using UnityEngine;
using System.Collections;
using System.Diagnostics;
using UnityEngine.InputSystem;
using Unity.VisualScripting;
using System.Collections.Generic;
using SummonSupportEvents;

public static class CombatStatHandler
{
    //public static CombatStatHandler Instance;

    // doesnt use or recognize percentage based damage
    private static float tickRateFloat = 1f;
    private static WaitForSeconds tickRate = new WaitForSeconds(tickRateFloat);
    private static AbilityModHandler modHandler;
    private static Mod_Base mod;
    private static LivingBeing currentTarget;
    private static LivingBeing currentCaster;
    private static Ability currentAbility;
    private static List<StatusEffects> currentStatusEffects = new();





    public static void HandleEffectPackage(Ability ability, LivingBeing caster, LivingBeing target, EffectPackage effectPackage)
    {
        currentAbility = ability;
        currentCaster = caster;
        currentTarget = target;
        modHandler = caster.GetComponent<AbilityModHandler>();
        //UnityEngine.Debug.Log($"caster = {caster.Name}, target = {target.Name} mod handler = {modHandler}");

        if (effectPackage.Heal.Value > 0)
        {
            AdjustHealValue(effectPackage.Heal.Value);
        }
        if (effectPackage.HealOverTime.Value > 0)
        {
            HandleApplyDOT(AttributeType.CurrentHitpoints, effectPackage.HealOverTime.Value, AbilityModTypes.HealOverTime);
        }
        if (effectPackage.Damage.Value > 0)
        {
            EventDeclarer.ShakeCamera?.Invoke(1f);
            AdjustDamageValue(effectPackage.Damage, effectPackage.SpecialAbilityAttribute);
        }
        if (effectPackage.DamageOverTime.Value > 0)
        {
            //UnityEngine.Debug.Log($"effectPackage.DamageOverTime.Value = {effectPackage.DamageOverTime.Value}.");
            AdjustAndApplyDOT(effectPackage.DamageOverTime);
        }
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
        //currentStatusEffects.Add(AbilityLibrary.GetStatusEffectLibrary().entries[0].Effect);
        SetAllCurrentStatusEffects(ability, effectPackage);
        //UnityEngine.Debug.Log($"current status effect list = {currentStatusEffects}");
        if (currentStatusEffects != null && currentStatusEffects.Count > 0)
        {
            foreach (StatusEffects status in currentStatusEffects)
            {
                target.AlterStatusEffectList(status.EffectType, true);
                target.StartCoroutine(RemoveStatusEffect(status));
            }
            if (target.TryGetComponent(out AI_CC_State ccState))
            {
                foreach (StatusEffects status in currentStatusEffects)
                {
                    ccState.RecieveCC(status, currentCaster);
                    if (status.EffectType == StatusEffectType.Pulled) ccState.SetPullEpicenter(Camera.main.ScreenToWorldPoint(Input.mousePosition));
                    if (status.EffectType == StatusEffectType.KnockInTheAir) ability.KnockInTheAir(caster, target.GetComponent<Rigidbody>());
                    //UnityEngine.Debug.Log($"elapsed time in the combat handler package handler function: {stopwatch.ElapsedMilliseconds}");
                }
            }
        }
    }

    private static void SetAllCurrentStatusEffects(Ability ability, EffectPackage effectPackage)
    {
        if (modHandler != null)
        {
            currentStatusEffects = modHandler.GetModStatusEffects(ability);
            {
                foreach (StatusEffects status in effectPackage.StatusEffects)
                {
                    if (!currentStatusEffects.Contains(status)) currentStatusEffects.Add(status);
                }
            }
        }
        else currentStatusEffects = effectPackage.StatusEffects;
    }


    #region Adjust and apply damage, heal, temo attributes and damage over times
    public static float AdjustDamageValue(Damage_AT damage_AT, SpecialAbilityAttribute specialAbilityAttribute = SpecialAbilityAttribute.None)
    {
        float damageValue = GetDamageByType(damage_AT);
        if (modHandler != null)
        {
            damageValue += modHandler.GetModAttributeByType(currentAbility, AbilityModTypes.Damage);
        }
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
    private static float GetDamageByType(Damage_AT damage_AT)
    {
        if (damage_AT.ValueType == ValueType.Flat) return damage_AT.Value;
        if (damage_AT.ValueType == ValueType.Percentage) return damage_AT.Value / 100f * currentTarget.GetAttribute(AttributeType.MaxHitpoints);
        else return damage_AT.Value * currentTarget.GetAttribute(AttributeType.MaxHitpoints) / (currentTarget.GetAttribute(AttributeType.CurrentHitpoints) * 2);
    }
    public static float AdjustHealValue(float healValue)
    {
        if (mod != null) healValue += mod.GetModdedAttribute(AbilityModTypes.Heal);
        float newHP = Mathf.Min(healValue + currentTarget.GetAttribute(AttributeType.CurrentHitpoints), currentTarget.GetAttribute(AttributeType.MaxHitpoints));
        currentTarget.SetAttribute(AttributeType.CurrentHitpoints, newHP);
        //UnityEngine.Debug.Log($"healing {target.Name} for {healValue}");
        return healValue;
    }
    public static float AdjustAndApplyTempChange(TempAttrChange tempAttr)
    {
        UnityEngine.Debug.Log("Adjusting and applying tempchange");
        float changeValue = tempAttr.Value;
        float duration = currentAbility.Duration;
        if (tempAttr is TempAttrDecrease_AT) changeValue = -changeValue; // set to negative if it decreases
        if (tempAttr.ResourceAttribute == AttributeType.CurrentHitpoints || tempAttr.ResourceAttribute == AttributeType.MaxHitpoints)
        {
            if (currentAbility.ElementTypes.Count > 0) changeValue = AdjustBasedOnAffinity(currentAbility.ElementTypes[0], changeValue);
            if (currentAbility.PhysicalType != PhysicalType.None) changeValue = AdjustBasedOnArmor(currentAbility.PhysicalType, changeValue);
        }
        UnityEngine.Debug.Log($"the change value = {changeValue}. tempAttr.PhysicalResistance = {tempAttr.ElementalAffinity}");

        if (tempAttr.ResourceAttribute != AttributeType.None) ApplyTempValue(tempAttr.ResourceAttribute, changeValue, duration);
        if (tempAttr.PhysicalResistance != PhysicalType.None) ApplyTempValue(tempAttr.PhysicalResistance, changeValue, duration);
        if (tempAttr.ElementalAffinity != Element.None) ApplyTempValue(tempAttr.ElementalAffinity, changeValue, duration);
        if (tempAttr.MovementAttribute != MovementAttributes.None) ApplyTempValue(tempAttr.MovementAttribute, changeValue, duration);

        //target.StartCoroutine(ResetTempAttribute(target, tempAttr.AttributeType, -changeValue, duration)); //reset by using opposite

        return changeValue;
    }
    public static float AdjustAndApplyDOT(Damage_AT damageOT)
    {
        float damageValue = -damageOT.Value;
        foreach (Element element in currentAbility.ElementTypes)
        {
            PhysicalType physical = currentAbility.PhysicalType;
            if (element != Element.None) damageValue = AdjustBasedOnAffinity(element, damageValue); // damage value is opposite?
            if (currentAbility.PhysicalType != PhysicalType.None) damageValue = AdjustBasedOnArmor(physical, damageValue);
        }
        HandleApplyDOT(AttributeType.CurrentHitpoints, damageValue, AbilityModTypes.DamageOverTime);

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
        if (relevantResistance > 0)
        {
            value -= value * relevantResistance / 100;
            //new value equals old value, plus old value times relevant affinity converted into percentage
        }

        return value;
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

    private static void HandleApplyDOT(AttributeType attributeType, float changeValue, AbilityModTypes modType)
    {
        float duration = currentAbility.Duration;
        if (modHandler != null)
        {
            duration += modHandler.GetModAttributeByType(currentAbility, AbilityModTypes.Duration);
        }
        //UnityEngine.D
        // ebug.Log($"Changing {target}s regen by {newValue}");
        if (mod != null)
        {
            changeValue += mod.GetModdedAttribute(modType);
            duration += mod.GetModdedAttribute(AbilityModTypes.Duration);
        }
        currentTarget.ChangeRegeneration(attributeType, changeValue);
        currentTarget.StartCoroutine(ResetRegeneration(currentAbility, currentTarget, attributeType, changeValue, duration));
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

    #region remove temp status effect

    private static IEnumerator RemoveStatusEffect(StatusEffects status)
    {
        yield return new WaitForSeconds(status.Duration);
        currentTarget.AlterStatusEffectList(status.EffectType, false);

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


//public static void AdjustForOverValue(LivingBeing target, AttributeType attributeTypeTempMax, AttributeType attributeTypeMax, AttributeType typeCurrentValue, float changeValue)

//can be for power or health.
// the question:
// how much current and over points should the target have?


//Stopwatch stopwatch = new();
//        //UnityEngine.Debug.Log($"ability = {ability.name}. caster = {caster.Name}. target = {target.Name}. for self? {forSelf}");
//        LivingBeing casterStats = caster.GetComponent<LivingBeing>(); //#TODO NANI?
//        LivingBeing targetStats = target.GetComponent<LivingBeing>();
//        LivingBeing theTarget = casterStats;
//        currentAbility = ability;
//        modHandler = caster.gameObject.GetComponent<AbilityModHandler>();
//        if (!forSelf) theTarget = targetStats;
//
//        mod = modHandler.GetAbilityMod(ability);
//
//        // handle self effects
//        foreach (EffectPackage package in ability.TargetTypeAndEffects)
//        {
//            if (forSelf && package.TargetType == TargetType.Self || !forSelf && package.TargetType == TargetType.Target)
//            {
//                if (package.Heal.Value > 0)
//                {
//                    AdjustHealValue(package.Heal.Value, theTarget, casterStats);
//                }
//                if (package.HealOverTime.Value > 0)
//                {
//                    HandleApplyDOT(ability, target, AttributeType.CurrentHitpoints, package.HealOverTime.Value, package.HealOverTime.Duration, AbilityModTypes.HealOverTime);
//                }
//                if (package.Damage.Count > 0)
//                {
//                    foreach (Damage_AT damage in package.Damage)
//                    {
//                        AdjustDamageValue(damage, theTarget, casterStats, package.SpecialAbilityAttribute);
//                    }
//                }
//                if (package.DamageOverTime.Count > 0)
//                {
//                    foreach (DamageoT_AT damage in package.DamageOverTime)
//                    {
//                        AdjustAndApplyDOT(ability, damage, theTarget, casterStats);
//                    }
//                }
//                if (package.AttributeUp.Count > 0)
//                {
//                    foreach (TempAttrIncrease_AT tempChange in package.AttributeUp)
//                    {
//                        AdjustAndApplyTempChange(ability, tempChange, theTarget, casterStats);
//                    }
//                }
//                if (package.AttributeDown.Count > 0)
//                {
//                    foreach (TempAttrDecrease_AT tempChange in package.AttributeDown)
//                    {
//                        AdjustAndApplyTempChange(ability, tempChange, theTarget, casterStats);
//                    }
//                }
//
//                if (package.StatusEffects.Count > 0)
//                {
//                    foreach (StatusEffects status in package.StatusEffects)
//                    {
//                        target.AlterStatusEffectList(status.EffectType, true);
//                        target.StartCoroutine(RemoveStatusEffect(target, status));
//                    }
//                    if (target.TryGetComponent<AI_CC_State>(out AI_CC_State ccState))
//                    {
//                        foreach (StatusEffects status in package.StatusEffects)
//                        {
//                            ccState.RecieveCC(status, casterStats);
//                            if (status.EffectType == StatusEffectType.Pulled) ccState.SetPullEpicenter(Camera.main.ScreenToWorldPoint(Input.mousePosition));
//
//                        }
//                    }
//                }
//            }
//
//        }