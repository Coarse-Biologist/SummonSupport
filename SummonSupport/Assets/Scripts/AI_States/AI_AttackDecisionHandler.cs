using System.Collections.Generic;
using UnityEngine;

public static class AI_AttackDecisionHandler
{
    public static List<Crew_Ability_SO> GetUsableAbilities(GameObject caster, GameObject target)
    {
        List<Crew_Ability_SO> allCasterAbilities = caster.GetComponent<LivingBeing>().Abilties;
        List<Crew_Ability_SO> usableAbilities = new List<Crew_Ability_SO>();
        foreach (Crew_Ability_SO ability in allCasterAbilities)
        {

        }
        return new List<Crew_Ability_SO>();
    }
}