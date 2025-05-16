
using UnityEngine;


public class CreatureAbilityHandler : AbilityHandler
{

    new void Awake()
    {
        base.Awake();
    }

    public void UseAbility(Vector2 targetLocation)
    {
        CastAbility(0, targetLocation, abilityDirection.transform.rotation);
    }

}
