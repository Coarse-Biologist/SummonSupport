using System.Collections;
using UnityEngine;
using System.Collections.Generic;

public class AbilityHandler : MonoBehaviour
{
    [SerializeField] protected GameObject abilitySpawn;
    [SerializeField] public GameObject abilityDirection { get; private set; }
    [SerializeField] protected LivingBeing statsHandler;
    [SerializeField] protected List<Ability> abilities;
    [SerializeField] protected List<bool> abilitiesOnCooldown;
    private Dictionary<BeamAbility, GameObject> toggledAbilitiesDict = new();




    protected virtual void Awake()
    {
        if (abilitySpawn == null)
            abilitySpawn = gameObject;

        if (statsHandler == null)
            statsHandler = gameObject.GetComponent<LivingBeing>();
        if (abilityDirection == null)
            abilityDirection = gameObject.transform.GetChild(0).gameObject;
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
                usedAbility = HandleAuraAbility(auraAbility, statsHandler);
                break;
            case TeleportAbility teleportAbility:
                usedAbility = teleportAbility.Activate(gameObject, targetPosition);
                break;
            case MeleeAbility meleeAbility:
                usedAbility = meleeAbility.Activate(statsHandler.gameObject);
                break;
            case BeamAbility beamAbility:
                usedAbility = HandleBeamAbility(beamAbility, statsHandler);
                break;
        }
        return usedAbility;
    }
    private bool HandleBeamAbility(BeamAbility beamAbility, LivingBeing statsHandler)
    {
        GameObject beamInstance = null;
        // if this is currently active, i.e in the dictionary, delete the game object, for the ability should now be toggled off.
        // if this is not active, i.e NOT in the dictionary, call the normal function.
        if (toggledAbilitiesDict.TryGetValue(beamAbility, out GameObject activeAbility)) // if ability is among the 
        {
            toggledAbilitiesDict.Remove(beamAbility);
            Destroy(activeAbility);
            return false;
        }
        else
        {
            beamInstance = beamAbility.ToggleBeam(statsHandler.gameObject, abilityDirection.transform);
            toggledAbilitiesDict.TryAdd(beamAbility, beamInstance);
            return true;
        }


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
    bool HandleAuraAbility(AuraAbility auraAbility, LivingBeing statsHandler)
    {
        return auraAbility.Activate(statsHandler.gameObject);
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





//if (toggledAbilitiesDict.TryGetValue(beamAbility, out bool toggled)) // if ability is among the 
//        {
//            toggledAbilitiesDict[beamAbility] = !toggled;
//            beamAbility.ToggleBeam(statsHandler.gameObject, !toggled);
//        }
//        else
//        {
//            toggledAbilitiesDict.TryAdd(beamAbility, true);
//            beamAbility.ToggleBeam(beamAbility, statsHandler, true);
//        }