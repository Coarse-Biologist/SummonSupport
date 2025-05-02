using System.Collections;
using UnityEngine;
using System.Collections.Generic;

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

    protected void CastAbility(int abilityIndex)
    {
        Ability ability = abilities[abilityIndex];

        if (abilitiesOnCooldown[abilityIndex])
            return;
            
        if (!HasEnoughPower(ability.PowerCost))
            return;

        HandleAbilityType(ability);
        StartCoroutine(SetOnCooldown(abilityIndex));
    }   

    void HandleAbilityType(Ability ability)
    {
        switch (ability)
        {
            case ProjectileAbility projectile:
                HandleProjectile(projectile);
                break;

            case TargetMouseAbility pointAndClickAbility:
                HandlePointAndClick(pointAndClickAbility);
                break;
            
            case ConjureAbility conjureAbility:
                HandleConjureAbility(conjureAbility);
                break;
        }
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

    void HandleProjectile(ProjectileAbility ability)
    {
        ability.Activate(gameObject, abilitySpawn, abilityDirection.transform);
        Logging.Verbose($"{gameObject.name} fires ability {ability.Name}");
    }

    void HandlePointAndClick(TargetMouseAbility ability)
    {
        ability.Activate(gameObject);
    }

    void HandleConjureAbility(ConjureAbility ability)
    {
        ability.Activate(gameObject);
    }


    private IEnumerator SetOnCooldown(int abilityIndex)
    {
        Ability ability = abilities[abilityIndex];
        try
        {
            Logging.Info("set cooldown for " + ability.Cooldown + " secods");
            abilitiesOnCooldown[abilityIndex] = true;
            yield return new WaitForSeconds(ability.Cooldown);
        }
        finally
        {
            Logging.Verbose("cooldown over");
            abilitiesOnCooldown[abilityIndex] = false; 
        }
    }
}

