using UnityEngine;
using System;

public static class DamageHandler
{

    private static float AdjustValue(Ability ability, LivingBeing caster, LivingBeing target)
    {
        float value;

        value = AdjustBasedOnAffinity(ability, caster, target);
        value = AdjustBasedOnArmor(value, target);

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
            if (relevantAffinity > 100)
                value = (float)Math.Abs(Math.Round(value * ((relevantAffinity - 100) / 100.0)));

            if (relevantAffinity <= 100 && relevantAffinity > 0)
                value = (float)Math.Round(value * 1.0 - (value * (relevantAffinity / 100)));
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
            if (relevantResistance > 100)
                value = (float)Math.Abs(Math.Round(value * ((relevantResistance - 100) / 100.0)));

            if (relevantResistance <= 100 && relevantResistance > 0)
                value = (float)Math.Round(value * 1.0 - (value * (relevantResistance / 100)));
        }

        return value;
    }
}


