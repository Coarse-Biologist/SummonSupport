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
        Logging.Info($" elapsed time : {stopwatch.ElapsedMilliseconds}");
        Logging.Info($"{selectedAbility} = selected Ability. {target.Name} = selected target");


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




//   private void HandleAllOptionsAvailable()
//   {
//       int diceRoll = UnityEngine.Random.Range(0, 100);
//       if (diceRoll > AI_Hostility) // selected ability set as either attack or support depending on dice roll compared to AI hostility level
//       {
//           SelectFriendlyAbilityandTarget();
//       }
//       else
//       {
//           SelectHostileAbilityandTarget();
//       }
//   }
//   private void HandleLimitedAbilityOptions()
//   {
//       if (supportAbilities.Count > attackAbilities.Count)
//       {
//           SelectFriendlyAbilityandTarget();
//       }
//       if (attackAbilities.Count > supportAbilities.Count)
//       {
//           SelectHostileAbilityandTarget();
//       }
//   }
//   private void HandleLimitedTargetOptions()
//   {
//       if (hostileTargets.Count > friendlyTargets.Count)
//       {
//           SelectHostileAbilityandTarget();
//       }
//       if (friendlyTargets.Count > hostileTargets.Count)
//       {
//           SelectFriendlyAbilityandTarget();
//       }
//   }
//   private void SelectFriendlyAbilityandTarget()
//   {
//       selectedAbility = supportAbilities[UnityEngine.Random.Range(0, supportAbilities.Count)];
//       selectedTarget = friendlyTargets[UnityEngine.Random.Range(0, friendlyTargets.Count)];
//   }
//   private void SelectHostileAbilityandTarget()
//   {
//       selectedAbility = attackAbilities[UnityEngine.Random.Range(0, attackAbilities.Count)];
//       selectedTarget = hostileTargets[UnityEngine.Random.Range(0, hostileTargets.Count)];
//   }
//  // public Tuple<Ability, LivingBeing> GetAbilityAndTarget(AbilityHandler casterAbilityHandler, List<LivingBeing> targets)
//  // {
//  //     //Stopwatch stopwatch = Stopwatch.StartNew();
//     selectedAbility = null;
//     selectedTarget = null;
//     attackAbilities.Clear();
//     supportAbilities.Clear();
//     hasFriendsAndEnemies = false;
//     hasAttackAndSupport = false;
//     friendlyTargets.Clear();
//     hostileTargets.Clear();
//     LivingBeing casterStats = casterAbilityHandler.GetComponent<LivingBeing>();
//
//     if (targets.Count == 0)
//         return null;
//
//     SetAbilityLists(casterAbilityHandler);
//
//     SetTargetsList(casterStats, targets);
//
//
//     if (friendlyTargets.Count == 0 && attackAbilities.Count == 0)
//         return null;
//     if (hostileTargets.Count == 0 && supportAbilities.Count == 0)
//         return null;
//
//     if (hostileTargets.Count > 0 && friendlyTargets.Count > 0) hasFriendsAndEnemies = true;
//     if (supportAbilities.Count > 0 && attackAbilities.Count > 0) hasAttackAndSupport = true;
//
//     if (hasAttackAndSupport && hasFriendsAndEnemies) HandleAllOptionsAvailable();
//     if (hasAttackAndSupport && !hasFriendsAndEnemies) HandleLimitedTargetOptions();
//     if (!hasAttackAndSupport && hasFriendsAndEnemies) HandleLimitedAbilityOptions();
//
//Logging.Info($" elapsed time : {stopwatch.ElapsedMilliseconds}");
//Logging.Info($"{selectedAbility} = selected Ability. {selectedTarget.Name} = selected target");
//        return new Tuple<Ability, LivingBeing>(selectedAbility, selectedTarget);
//
//    }
//
//}


//check if there are attack abilities
//check if there are support abilities

// check if there are hostile targets
// check if there are friendly targets



//     if (supportAbilities.Count == 0) // selected ability shall be attack if there are no support abilities
//            selectedAbility = attackAbilities[UnityEngine.Random.Range(0, attackAbilities.Count - 1)];
//        else if (attackAbilities.Count == 0) // selected ability shall be support if there are no attack abilities
//            selectedAbility = attackAbilities[UnityEngine.Random.Range(0, supportAbilities.Count - 1)];
//


//private void SetTargetsList(LivingBeing casterStats, List<LivingBeing> targets)
//{
//    foreach (LivingBeing target in targets) // make list of friendly and hostile targets
//    {
//        if (target == null) continue;
//        if (RelationshipHandler.GetRelationshipType(target.CharacterTag, casterStats.CharacterTag) == RelationshipType.Hostile)
//            hostileTargets.Add(target);
//        else friendlyTargets.Add(target);
//    }
//}