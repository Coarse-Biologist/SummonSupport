using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using System;

public class AbilityHandler : MonoBehaviour
{
    [SerializeField] protected GameObject abilitySpawn;
    [SerializeField] protected GameObject abilityDirection;
    [SerializeField] protected LivingBeing statsHandler;
    [SerializeField] protected List<Ability> abilities;
    [SerializeField] protected List<bool> abilitiesOnCooldown;


    protected virtual void Awake()
    {
        if (abilitySpawn == null)
            abilitySpawn = gameObject;
        if (abilityDirection == null)
            abilityDirection = gameObject;
        if (statsHandler == null)
            statsHandler = gameObject.GetComponent<LivingBeing>();
        foreach (Ability ablity in abilities)
        {
            abilitiesOnCooldown.Add(false);
        }
    }

    protected void CastAbility(int abilityIndex, Vector2 targetPosition, Quaternion rotation)
    {
        if (abilitiesOnCooldown[abilityIndex])
            return;
            
        Ability ability = abilities[abilityIndex];
            
        if (!HasEnoughPower(ability.PowerCost))
            return;

        if (HandleAbilityType(ability, targetPosition, rotation))
            StartCoroutine(SetOnCooldown(abilityIndex)); 
    }   

    bool HandleAbilityType(Ability ability, Vector2 targetPosition, Quaternion rotation)
    {
        bool usedAbility = false;
        switch (ability)
        {
            case ProjectileAbility projectile:
                usedAbility = HandleProjectile(projectile);
                break;

            case TargetMouseAbility pointAndClickAbility:
                usedAbility = HandlePointAndClick(pointAndClickAbility);
                break;
            
            case ConjureAbility conjureAbility:
                usedAbility = HandleConjureAbility(conjureAbility, targetPosition, rotation);
                break;
        }
        return usedAbility;
    }

    bool HasEnoughPower(float powerCost)
    {
        if (statsHandler)
        {
            if (powerCost > statsHandler.GetAttribute(AttributeType.CurrentPower))
                return false; //Not enough power to use ability
            else
                statsHandler.ChangeAttribute(AttributeType.CurrentPower, -powerCost);
        }
        return true;
    }

    bool HandleProjectile(ProjectileAbility ability)
    {
        return ability.Activate(gameObject, abilitySpawn, abilityDirection.transform);
    }

    bool HandlePointAndClick(TargetMouseAbility ability)
    {
        return ability.Activate(gameObject);
    }

    bool HandleConjureAbility(ConjureAbility ability, Vector2 targetPosition, Quaternion rotation)
    {
        return ability.Activate(gameObject, targetPosition, rotation);
    }


    private IEnumerator SetOnCooldown(int abilityIndex)
    {
        Ability ability = abilities[abilityIndex];
        try
        {
            abilitiesOnCooldown[abilityIndex] = true;
            yield return new WaitForSeconds(ability.Cooldown);
        }
        finally
        {
            abilitiesOnCooldown[abilityIndex] = false; 
        }
    }
}

