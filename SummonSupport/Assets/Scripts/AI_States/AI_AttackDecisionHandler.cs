using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class AI_AttackDecisionHandler : MonoBehaviour
{
    public int AI_Hostility { get; private set; } = 50; // percent chance, the higher the number, the more likely AI will use attack abilities if possible.
    private List<Ability> supportAbilities { get; set; } = new();
    private List<Ability> attackAbilities { get; set; } = new();
    private Ability selectedAbility { get; set; } = null;


    void Awake()
    {
        SetAbilityLists(GetComponent<AbilityHandler>());
    }




    public Ability GetAbilityForTarget(AbilityHandler casterAbilityHandler, LivingBeing target)
    {
        Stopwatch stopwatch = Stopwatch.StartNew();
        selectedAbility = null;
        bool targetIsFriendly = false;


        LivingBeing casterStats = casterAbilityHandler.GetComponent<LivingBeing>();

        if (target = null)
            return null;

        if (RelationshipHandler.GetRelationshipType(target.CharacterTag, casterStats.CharacterTag) == RelationshipType.Friendly)
        {
            targetIsFriendly = true;
            if (supportAbilities.Count == 0) return null;
        }
        if (RelationshipHandler.GetRelationshipType(target.CharacterTag, casterStats.CharacterTag) == RelationshipType.Hostile)
        {
            targetIsFriendly = false;
            if (attackAbilities.Count == 0) return null;
        }
        //Logging.Info($" elapsed time : {stopwatch.ElapsedMilliseconds}");
        //Logging.Info($"{selectedAbility} = selected Ability. {target.Name} = selected target");


        return SelectedAbility(targetIsFriendly);


    }
    private Ability SelectedAbility(bool friendlyTarget)
    {
        if (friendlyTarget)
        {
            return supportAbilities[UnityEngine.Random.Range(0, supportAbilities.Count)];
        }
        else return attackAbilities[UnityEngine.Random.Range(0, attackAbilities.Count)];
    }


    private void SetAbilityLists(AbilityHandler caster)
    {
        foreach (Ability ability in caster.Abilities) // make list of support and attack abilities
        {
            if (ability.AbilityTypeTag == AbilityTypeTag.DebuffsTarget)
                attackAbilities.Add(ability);
            else if (ability.AbilityTypeTag == AbilityTypeTag.BuffsTarget)
                supportAbilities.Add(ability);
        }
        AI_Hostility /= attackAbilities.Count;
        AI_Hostility *= attackAbilities.Count;
    }
    private bool RollForAggression() // if true, aggressive ability should be selected if possible.
    {
        if (supportAbilities.Count == 0) return true;
        if (attackAbilities.Count == 0) return false;
        else
            return UnityEngine.Random.Range(0, 100) < AI_Hostility;
    }
}

