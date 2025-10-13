using System.Collections;
using UnityEngine;
using System.Collections.Generic;

public class AbilityHandler : MonoBehaviour
{
    [SerializeField] protected GameObject abilitySpawn;
    [SerializeField] public GameObject abilityDirection { get; private set; }
    [SerializeField] protected LivingBeing statsHandler;
    [field: SerializeField] public List<Ability> Abilities { private set; get; } = new();
    [SerializeField] protected List<bool> abilitiesOnCooldown = new();
    public Dictionary<Ability, bool> abilitiesOnCooldownCrew = new();
    private Dictionary<BeamAbility, GameObject> toggledAbilitiesDict = new();
    [field: SerializeField] public WeaponInfo WeaponInfo { get; private set; }
    private bool charging = false;
    private AbilityModHandler modHandler;





    protected virtual void Awake()
    {
        if (abilitySpawn == null)
            abilitySpawn = gameObject;

        if (statsHandler == null)
            statsHandler = gameObject.GetComponent<LivingBeing>();
        if (abilityDirection == null)
            abilityDirection = gameObject.transform.GetChild(0).gameObject;
        foreach (Ability ability in Abilities)
        {
            abilitiesOnCooldown.Add(false);
            abilitiesOnCooldownCrew.Add(ability, false);
        }
        if (gameObject.TryGetComponent(out AbilityModHandler modScript))
            modHandler = modScript;
        else throw new System.Exception("NOPENOPENOPENOPENOPENOPENOPE");
    }

    public void LearnAbility(Ability ability)
    {
        Debug.Log($"Learning ability {ability}");
        if (!Abilities.Contains(ability) && ability != null)
        {
            Abilities.Add(ability);
            abilitiesOnCooldownCrew.Add(ability, false);
            abilitiesOnCooldown.Add(false);
        }
    }

    protected bool CastAbility(int abilityIndex, Vector2 targetPosition, Quaternion rotation)
    {
        //Logging.Info($"Ability at index {abilityIndex} trying to be used by {statsHandler.Name}!!!!");
        Ability ability = Abilities[abilityIndex];

        if (Abilities.Count <= 0 || abilitiesOnCooldown[abilityIndex])
            return false;


        if (!HasEnoughPower(ability.Cost))
            return false;

        bool usedAbility = HandleAbilityType(ability, targetPosition, rotation);

        if (!usedAbility)
            return false;

        StartCoroutine(SetOnCooldown(abilityIndex));
        int costMod = modHandler.GetModAttributeByType(ability, AbilityModTypes.Cost);
        statsHandler?.ChangeAttribute(AttributeType.CurrentPower, -ability.Cost + costMod);
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
            case ChargeAbility chargeAbility:
                if (!charging)
                {
                    SetCharging(true);
                    usedAbility = chargeAbility.Activate(gameObject);
                }

                break;
        }
        //Logging.Info($"able to use {ability.Name} = {usedAbility}");
        if (usedAbility) StartCoroutine(SetOnCooldown(Abilities.IndexOf(ability)));

        return usedAbility;
    }
    private bool HandleBeamAbility(BeamAbility beamAbility, LivingBeing statsHandler)
    {
        GameObject beamInstance = null;

        if (toggledAbilitiesDict.TryGetValue(beamAbility, out GameObject activeAbility))
        {
            StopToggledAbility(beamAbility, activeAbility);

            return false;
        }
        else
        {
            beamInstance = beamAbility.ToggleBeam(statsHandler.gameObject, abilityDirection.transform);
            toggledAbilitiesDict.TryAdd(beamAbility, beamInstance);
            statsHandler.ChangeRegeneration(beamAbility.CostType, -beamAbility.Cost);

            return true;
        }

    }
    public void SetCharging(bool alreadyCharging)
    {
        charging = alreadyCharging;
    }
    private void StopToggledAbility(BeamAbility beamAbility, GameObject activeAbility)
    {
        statsHandler.ChangeRegeneration(AttributeType.CurrentPower, beamAbility.Cost);
        toggledAbilitiesDict.Remove(beamAbility);
        Destroy(activeAbility);
    }



    public bool HasEnoughPower(float powerCost, AttributeType costType = AttributeType.CurrentPower)
    {
        return !statsHandler || powerCost < statsHandler.GetAttribute(AttributeType.CurrentPower);
    }
    public void HandleNoMana()
    {
        foreach (KeyValuePair<BeamAbility, GameObject> ability in toggledAbilitiesDict)
        { Destroy(ability.Value); }
        toggledAbilitiesDict.Clear();
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

    public IEnumerator SetOnCooldown(int abilityIndex)
    {
        Ability ability = Abilities[abilityIndex];
        try
        {
            abilitiesOnCooldownCrew[ability] = true;
            abilitiesOnCooldown[abilityIndex] = true;
            yield return new WaitForSeconds(ability.Cooldown + modHandler.GetModAttributeByType(ability, AbilityModTypes.Cooldown));
        }
        finally
        {
            abilitiesOnCooldownCrew[ability] = false;
            abilitiesOnCooldown[abilityIndex] = false;
        }
    }
    protected bool IsOnCoolDown(Ability ability)
    {
        bool onCooldown = abilitiesOnCooldownCrew[ability];
        return onCooldown;
    }

}

