
using Unity.Entities.UniversalDelegates;
using UnityEngine;


public class CreatureAbilityHandler : AbilityHandler
{

    new void Awake()
    {
        base.Awake();
    }

    public void UseAbility(Vector2 targetLocation)
    {
        CastAbility(GetAbilityIndex(), targetLocation, abilityDirection.transform.rotation);
    }

    private int GetAbilityIndex()
    {
        return Random.Range(0, Abilities.Count);
    }

}
