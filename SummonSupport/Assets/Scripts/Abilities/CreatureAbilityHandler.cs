
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



    private Ability selectedAbility { get; set; } = null;


    new void Awake()
    {
        base.Awake();
        SetAbilityLists();
    }

    public Ability GetAbilityForTarget(LivingBeing target, bool forSelf = false)
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
        }
        if (RelationshipHandler.GetRelationshipType(target.CharacterTag, casterStats.CharacterTag) == RelationshipType.Hostile)
        {
            targetIsFriendly = false;
            if (attackAbilities.Count == 0) return null;
        }
        //Logging.Info($" elapsed time : {stopwatch.ElapsedMilliseconds}");

        return SelectAbility(targetIsFriendly, target, forSelf);


    }
    private Ability SelectAbility(bool friendlyTarget, LivingBeing target, bool forSelf = false)
    {
        foreach (Ability ability in attackAbilities)
        {
            if (Ability.HasElementalSynergy(ability, target))
            {
                allSupportAbilities.Add(ability);
                synergyAbilities.Add(ability);
            }
            //else UnityEngine.Debug.Log($"{ability.name} would not synergize with {target}");
        }
        if (friendlyTarget)
        {
            if (allSupportAbilities.Count == 0) return null;

            selectedAbility = allSupportAbilities[Random.Range(0, allSupportAbilities.Count)];
        }
        else selectedAbility = attackAbilities[Random.Range(0, attackAbilities.Count)];

        foreach (Ability ability in synergyAbilities) { if (allSupportAbilities.Contains(ability)) allSupportAbilities.Remove(ability); }

        if (IsOnCoolDown(selectedAbility))
        {
            //UnityEngine.Debug.Log($"Returning null insytead of an ability");
            return null;
        }
        if (forSelf && selectedAbility is ProjectileAbility)
            return null;
        else
            return selectedAbility;
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
    public new void LearnAbility(Ability ability)
    {
        if (!Abilities.Contains(ability) && ability != null)
        {
            Abilities.Add(ability);
            abilitiesOnCooldownCrew.Add(ability, false);
            abilitiesOnCooldown.Add(false);
        }
        SetAbilityLists();
    }



    public void UseAbility(LivingBeing target, Ability ability)
    {
        if (ability != null)
        {
            Vector3 direction = (target.transform.position - transform.position).normalized;
            float angle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, angle, 0);
            CastAbility(ability, target.transform.position, transform.rotation);
        }
    }

}
