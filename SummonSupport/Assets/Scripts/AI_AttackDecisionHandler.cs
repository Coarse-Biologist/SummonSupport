using System.Collections.Generic;
using UnityEngine;

public static class AI_AttackDecisionHandler
{
    public static List<Ability> GetUsableAbilities(GameObject caster, GameObject target)
    {
        List<Ability> allCasterAbilities = caster.GetComponent<LivingBeing>().Abilties;
        List<Ability> usableAbilities = new List<Ability>();
        foreach (Ability ability in allCasterAbilities)
        {

        }
        return new List<Ability>();
    }
}
