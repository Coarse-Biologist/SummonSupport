
using UnityEngine;
using System.Collections.Generic;
using System.Diagnostics;



public class CreatureAbilityHandler : AbilityHandler
{
    private int AI_Hostility { get; set; } = 50; // percent chance, the higher the number, the more likely AI will use attack abilities if possible.
    private List<Ability> supportAbilities { get; set; } = new();
    private List<Ability> attackAbilities { get; set; } = new();
    private Ability selectedAbility { get; set; } = null;


    new void Awake()
    {
        base.Awake();
        SetAbilityLists(GetComponent<AbilityHandler>());
    }


    public Ability GetAbilityForTarget(AbilityHandler casterAbilityHandler, LivingBeing target)
    {
        Stopwatch stopwatch = Stopwatch.StartNew();
        selectedAbility = null;
        bool targetIsFriendly = false;


        LivingBeing casterStats = casterAbilityHandler.GetComponent<LivingBeing>();

        if (target == null)
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



    public void UseAbility(Vector2 targetLocation, LivingBeing target)
    {
        Ability ability = GetAbilityForTarget(GetComponent<AbilityHandler>(), target);
        if (ability != null)
        {
            UnityEngine.Debug.Log($"{ability} = ability selected by {GetComponent<LivingBeing>().Name} against {target}");
            CastAbility(Abilities.IndexOf(ability), targetLocation, abilityDirection.transform.rotation);
        }
    }

    //private int GetAbilityIndex()
    //{
    //    return Random.Range(0, Abilities.Count);
    //}

}
