using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using SummonSupportEvents;
using System.Linq;

public class AbilityHandler : MonoBehaviour
{
    [field: SerializeField] public GameObject abilitySpawn { private set; get; }
    [SerializeField] protected LivingBeing statsHandler;
    [field: SerializeField] public List<Ability> Abilities { private set; get; } = new();
    public Dictionary<Ability, bool> abilitiesOnCooldownCrew = new();
    private Dictionary<BeamAbility, GameObject> toggledAbilitiesDict = new();
    private bool charging = false;
    public AbilityModHandler modHandler { protected set; get; }
    private AnimationControllerScript anim;
    //private bool AbilityToggledRecently = false;


    protected virtual void Awake()
    {
        anim = GetComponent<AnimationControllerScript>();
        //if (anim == null) throw new System.Exception($"Animation controller is null. it was not found among children objects.");

        if (abilitySpawn == null)
            abilitySpawn = gameObject;

        if (statsHandler == null)
            statsHandler = gameObject.GetComponent<LivingBeing>();
        foreach (Ability ability in Abilities)
        {
            abilitiesOnCooldownCrew.Add(ability, false);
        }
    }
    void Start()
    {
        modHandler = AbilityModHandler.Instance;
    }

    public void LearnAbility(Ability ability)
    {
        //Debug.Log($"Learning ability {ability}");
        if (!Abilities.Contains(ability) && ability != null)
        {
            Abilities.Add(ability);
            abilitiesOnCooldownCrew.Add(ability, false);
        }
    }

    protected bool CastAbility(Ability ability, Vector2 targetPosition, Quaternion rotation)
    {
        if (ability is not BeamAbility beam || !toggledAbilitiesDict.TryGetValue(beam, out GameObject gameObject)) StopAllToggledAbilities();
        //when casting a beam which is already toggled on, toggle beam off. 
        bool usedAbility = HandleAbilityType(ability, targetPosition, rotation);

        if (!usedAbility)
            return false;

        //handle Ionization status effect
        int ionizationValue = statsHandler.SE_Handler.GetStatusEffectValue(StatusEffectType.Ionized);
        if (ionizationValue > 0)
        {
            statsHandler.ChangeAttributeByPercent(AttributeType.CurrentHitpoints, (float)-.01 * ionizationValue);
            EventDeclarer.IonizedAttack?.Invoke(statsHandler);
        }
        StartCoroutine(SetOnCooldown(ability));
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
            case DashAbility dashAbility:
                usedAbility = HandleDashAbility(dashAbility);
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
        return usedAbility;
    }
    private bool HandleBeamAbility(BeamAbility beamAbility, LivingBeing statsHandler)
    {
        GameObject beamInstance;
        if (IsOnCoolDown(beamAbility)) return false;
        if (toggledAbilitiesDict.TryGetValue(beamAbility, out GameObject activeAbility))
        {
            StartCoroutine(SetOnCooldown(beamAbility));
            StopToggledAbility(beamAbility, activeAbility);
            return false;
        }
        else
        {
            if (anim != null) anim.ChangeLayerAnimation("BeamStart", 1, 2f, true);


            beamInstance = beamAbility.ToggleBeam(statsHandler.gameObject, abilitySpawn.transform);
            toggledAbilitiesDict.TryAdd(beamAbility, beamInstance);
            statsHandler.ChangeRegeneration(beamAbility.CostType, -beamAbility.Cost);
            //AbilityToggledRecently = true;
            return true;
        }
    }
    public void SetCharging(bool alreadyCharging)
    {
        if (anim != null) anim.ChangeLayerAnimation("Sprint", 1, .1f);

        charging = alreadyCharging;
    }
    private void StopAllToggledAbilities()
    {
        foreach (KeyValuePair<BeamAbility, GameObject> kvp in toggledAbilitiesDict)
        {
            statsHandler.ChangeRegeneration(AttributeType.CurrentPower, kvp.Key.Cost);

            Destroy(kvp.Value);
        }
        toggledAbilitiesDict.Clear();
    }
    private void StopToggledAbility(BeamAbility beamAbility, GameObject activeAbility)
    {
        if (anim != null)
        {
            anim.SeLayerWeight(1, 0);
        }
        statsHandler.ChangeRegeneration(AttributeType.CurrentPower, beamAbility.Cost);
        toggledAbilitiesDict.Remove(beamAbility);
        //AbilityToggledRecently = false;
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
        //Debug.Log("Time to see the heavy throw animation");
        if (anim != null) anim.ChangeLayerAnimation("OneArmedThrow", 1, 2f);

        return ability.Activate(statsHandler);
    }

    bool HandlePointAndClick(TargetMouseAbility ability)
    {
        if (anim != null) anim.ChangeLayerAnimation("SpellCast", 1, 1f);

        return ability.Activate(gameObject);
    }

    bool HandleConjureAbility(ConjureAbility ability, Vector2 targetPosition, Quaternion rotation)
    {
        if (anim != null) anim.ChangeLayerAnimation("HeavyThrow", 1, 1f);

        return ability.Activate(gameObject, rotation);
    }

    bool HandleDashAbility(DashAbility dashAbility)
    {
        if (anim != null) anim.ChangeLayerAnimation("Sprint", 1, .1f);
        return dashAbility.Activate(gameObject);
    }
    bool HandleAuraAbility(AuraAbility auraAbility, LivingBeing statsHandler)
    {
        if (anim != null) anim.ChangeLayerAnimation("Buff", 1, 1f);

        return auraAbility.Activate(statsHandler.gameObject);
    }

    public IEnumerator SetOnCooldown(Ability ability)
    {
        try
        {
            float coolDown = ability.Cooldown + modHandler.GetModAttributeByType(ability, AbilityModTypes.Cooldown) + statsHandler.SE_Handler.GetStatusEffectValue(StatusEffectType.Lethargic);
            // default, plus modifier, plus lethargy value
            abilitiesOnCooldownCrew[ability] = true;
            yield return new WaitForSeconds(coolDown);
        }
        finally
        {
            abilitiesOnCooldownCrew[ability] = false;
        }
    }
    protected bool IsOnCoolDown(Ability ability)
    {
        bool onCooldown = abilitiesOnCooldownCrew[ability];
        return onCooldown;
    }

}

