using UnityEngine;
using System;

public static class DamageHandler
{

    private static float AdjustValue(Ability ability, LivingBeing caster, LivingBeing target)
    {
        float value = ability.Value;

        if (value < 0) // if value is negative, and therefore a damage value
        {
            value = AdjustBasedOnAffinity(ability, caster, target);
            value = AdjustBasedOnArmor(value, target);
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




}

