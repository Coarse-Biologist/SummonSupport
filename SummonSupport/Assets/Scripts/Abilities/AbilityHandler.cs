using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using System;
using Unity.VisualScripting;

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

    protected bool CastAbility(int abilityIndex, Vector2 targetPosition, Quaternion rotation)
    {
        if (abilitiesOnCooldown[abilityIndex])
            return false;

        Ability ability = abilities[abilityIndex];

        if (!HasEnoughPower(ability.PowerCost))
            return false;

        bool usedAbility = HandleAbilityType(ability, targetPosition, rotation);

        if (!usedAbility)
            return false;

        StartCoroutine(SetOnCooldown(abilityIndex));
        statsHandler?.ChangeAttribute(AttributeType.CurrentPower, -ability.PowerCost);
        return true;
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
            case AuraAbility auraAbility:
                usedAbility = HandleAuraAbility(auraAbility, statsHandler, targetPosition);
                break;
        }
        return usedAbility;
    }

    bool HasEnoughPower(float powerCost)
    {
        return !statsHandler || powerCost < statsHandler.GetAttribute(AttributeType.CurrentPower);
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
    bool HandleAuraAbility(AuraAbility auraAbility, LivingBeing statsHandler, Vector2 targetLoc)
    {

        return auraAbility.ActivateAura(statsHandler.gameObject, targetLoc);
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

