
using UnityEngine;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine.Rendering;



public class CreatureAbilityHandler : AbilityHandler
{
    private int AI_Hostility { get; set; } = 50; // percent chance, the higher the number, the more likely AI will use attack abilities if possible.
    private List<Ability> supportAbilities { get; set; } = new();
    private List<Ability> attackAbilities { get; set; } = new();
    private List<Ability> allSupportAbilities { get; set; } = new();
    private List<Ability> synergyAbilities { get; set; } = new();
    private List<Ability> abilityOnCooldown = new();



    private Ability selectedAbility { get; set; } = null;


    new void Awake()
    {
        base.Awake();
        SetAbilityLists();
    }

    public Ability GetAbilityForTarget(LivingBeing target)
    {
        Stopwatch stopwatch = Stopwatch.StartNew();
        selectedAbility = null;
        bool targetIsFriendly = false;

        LivingBeing casterStats = GetComponent<LivingBeing>();

        if (target == null)
            return null;

        if (RelationshipHandler.GetRelationshipType(target.CharacterTag, casterStats.CharacterTag) == RelationshipType.Friendly)
        {
            targetIsFriendly = true;
            //if (supportAbilities.Count == 0) return null;
        }
        if (RelationshipHandler.GetRelationshipType(target.CharacterTag, casterStats.CharacterTag) == RelationshipType.Hostile)
        {
            targetIsFriendly = false;
            if (attackAbilities.Count == 0) return null;
        }
        //Logging.Info($" elapsed time : {stopwatch.ElapsedMilliseconds}");

        return SelectAbility(targetIsFriendly, target);

    }
    private Ability SelectAbility(bool friendlyTarget, LivingBeing target)
    {
        foreach (Ability ability in attackAbilities)
        {
            if (Ability.HasElementalSynergy(ability, target) && !(ability is ProjectileAbility))
            {
                allSupportAbilities.Add(ability);
                synergyAbilities.Add(ability);
                UnityEngine.Debug.Log($"{ability.name} added to list of support and synergy abilities");
            }
            else UnityEngine.Debug.Log($"{ability.name} would not synergize with {target}");
        }
        if (friendlyTarget)
        {
            UnityEngine.Debug.Log($"The target {target} is friendly. the numnber of all support abilities= {allSupportAbilities.Count}");
            if (allSupportAbilities.Count == 0)
            {
                return null;
            }
            selectedAbility = allSupportAbilities[Random.Range(0, allSupportAbilities.Count)];
        }
        else selectedAbility = attackAbilities[Random.Range(0, attackAbilities.Count)];

        foreach (Ability ability in synergyAbilities) { if (allSupportAbilities.Contains(ability)) allSupportAbilities.Remove(ability); }

        if (IsOnCoolDown(selectedAbility))
        {
            UnityEngine.Debug.Log($"Returning null insytead of an ability");
            return null;
        }
        else
        {
            return selectedAbility;
        }
    }


    public void SetAbilityLists()
    {
        foreach (Ability ability in Abilities) // make list of support and attack abilities
        {
            if (ability.AbilityTypeTag == AbilityTypeTag.DebuffsTarget)
                attackAbilities.Add(ability);
            else if (ability.AbilityTypeTag == AbilityTypeTag.BuffsTarget)
            {
                supportAbilities.Add(ability);
            }
        }
        allSupportAbilities = supportAbilities;
        if (attackAbilities.Count != 0) AI_Hostility /= attackAbilities.Count;
        AI_Hostility *= attackAbilities.Count;
    }
    private bool RollForAggression() // if true, aggressive ability should be selected if possible.
    {
        if (supportAbilities.Count == 0) return true;
        if (attackAbilities.Count == 0) return false;
        else
            return UnityEngine.Random.Range(0, 100) < AI_Hostility;
    }



    public void UseAbility(LivingBeing target, Ability ability)
    {
        float range = ability.Range;
        if (ability != null)
        {
            //UnityEngine.Debug.Log($"{ability} = ability selected by {GetComponent<LivingBeing>().Name} against {target}");
            CastAbility(Abilities.IndexOf(ability), target.transform.position, abilityDirection.transform.rotation);
        }
    }

}
