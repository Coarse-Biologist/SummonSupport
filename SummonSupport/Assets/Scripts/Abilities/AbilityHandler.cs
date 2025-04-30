using UnityEngine;

public class AbilityHandler : MonoBehaviour
{
    [SerializeField] protected GameObject abilitySpawn;
    [SerializeField] protected GameObject abilityDirection;
    [SerializeField] protected LivingBeing statsHandler;

    protected virtual void Awake()
    {
        if (abilitySpawn == null)
            abilitySpawn = gameObject;
        if (abilityDirection == null)
            abilityDirection = gameObject;
        if (statsHandler == null)
            statsHandler = gameObject.GetComponent<LivingBeing>();
    }

    protected void CastAbility(Ability ability)
    {
        Logging.Info($"Trying to cast ability: {ability.name}");
        if (statsHandler)
        {
            if (ability.PowerCost > statsHandler.GetAttribute(AttributeType.CurrentPower))
                return; //Not enough power to use ability
            else
                statsHandler.ChangeAttribute(AttributeType.CurrentPower, -ability.PowerCost);
        }

        switch (ability)
        {
            case ProjectileAbility projectile:
                Logging.Verbose("Cast Ability is a projectile: " + ability.Name);
                HandleProjectile(projectile);
                break;

            case TargetMouseAbility pointAndClickAbility:
                Logging.Verbose($"Cast Ability is a point and click ability: {ability.Name}");
                HandlePointAndClick(pointAndClickAbility);
                break;
        }
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
}

