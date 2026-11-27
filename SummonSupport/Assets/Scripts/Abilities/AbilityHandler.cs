using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using SummonSupportEvents;
using System.Linq;
using Unity.VisualScripting;
using SS_Structs;

public class AbilityHandler : MonoBehaviour
{
    [field: SerializeField] public GameObject abilitySpawn { private set; get; }
    protected LivingBeing statsHandler;
    [field: SerializeField] public List<Ability> Abilities { private set; get; } = new();
    [field: SerializeField] public List<SlottedAbilities> SlottedAbilities { protected set; get; } = new();

    public Dictionary<Ability, bool> abilitiesOnCooldown = new();
    protected Dictionary<BeamAbility, GameObject> toggledAbilitiesDict = new();
    private bool charging = false;
    public AbilityModHandler modHandler { protected set; get; }
    public AnimationControllerScript anim { private set; get; }
    protected LivingBeingAudioHandler audioHandler;
    private NetworkCombatant networkCombatant;
    //private bool AbilityToggledRecently = false;

    /// <summary>True on the machine that decides when this being casts: always offline, and the owner online.</summary>
    protected bool HasCastAuthority => networkCombatant == null || networkCombatant.HasCastAuthority;


    protected virtual void Awake()
    {
        anim = GetComponent<AnimationControllerScript>();
        //if (anim == null) throw new System.Exception($"Animation controller is null. it was not found among children objects.");

        if (abilitySpawn == null)
            abilitySpawn = gameObject;

        statsHandler = gameObject.GetComponent<LivingBeing>();
        networkCombatant = GetComponent<NetworkCombatant>();
        audioHandler = GetComponent<LivingBeingAudioHandler>();


        foreach (Ability ability in Abilities)
        {
            AddAbilityCooldownSlot(ability);
        }
    }
    protected void Start()
    {
        modHandler = AbilityModHandler.Instance;
        audioHandler = GetComponent<LivingBeingAudioHandler>();
        statsHandler = GetComponent<LivingBeing>();

        if (audioHandler == null) throw new System.Exception($"Audio handler is null. it was not found on the object.");

        //else Debug.Log($"Audio handler found in {statsHandler.name}.");

    }

    public void LearnAbility(Ability ability)
    {
        //Debug.Log($"Learning ability {ability}");
        if (!Abilities.Contains(ability) && ability != null)
        {
            Abilities.Add(ability);
            abilitiesOnCooldown.Add(ability, false);
        }
    }

    /// <summary>
    /// Entry point for a being that decides to use an ability. Runs the cast here, shares it so every other
    /// machine replays it, and pays the cost and cooldown.
    /// </summary>
    protected bool CastAbility(Ability ability, Vector3 targetPosition, Quaternion rotation)
    {
        if (PauseGameHandler.paused) return false;
        if (!HasCastAuthority) return false; // this copy only replays casts sent by its owner

        bool usedAbility = PerformCast(ability, targetPosition, rotation);
        float cost = usedAbility ? GetModifiedCost(ability) : 0f;

        if (networkCombatant != null)
        {
            // Turning a beam off reports false but still changes what everyone should see,
            // so beams always share their new state.
            if (ability is BeamAbility beam)
                networkCombatant.ShareCast(ability, targetPosition, rotation, AbilityCastKind.Cast, toggledAbilitiesDict.ContainsKey(beam), cost);
            else if (usedAbility)
                networkCombatant.ShareCast(ability, targetPosition, rotation, AbilityCastKind.Cast, false, cost);
        }

        if (!usedAbility)
            return false;

        StartCoroutine(SetOnCooldown(ability));
        statsHandler?.ChangeAttribute(AttributeType.CurrentPower, -cost);

        return true;
    }

    /// <summary>
    /// Fires an ability on behalf of something other than a normal cast (for example the movement dash).
    /// No cost, cooldown, animation or sound, and toggled abilities keep running. Everyone sees it.
    /// </summary>
    public bool TriggerAbility(Ability ability)
    {
        if (ability == null || !HasCastAuthority) return false;

        bool usedAbility = ActivateTriggered(ability);
        if (usedAbility && networkCombatant != null)
            networkCombatant.ShareCast(ability, transform.position, transform.rotation, AbilityCastKind.Trigger);
        return usedAbility;
    }

    /// <summary>
    /// Replays a cast that the owner of this being made on another machine. Spawns the same ability objects,
    /// animation and sound, but pays no cost and starts no cooldown (the host charges client casts itself,
    /// see NetworkCombatant). Returns whether the ability was used.
    /// </summary>
    public bool ReplayCast(Ability ability, AbilityCastData cast)
    {
        if (cast.kind == AbilityCastKind.Trigger)
            return ActivateTriggered(ability);

        // Beams carry their state instead of a toggle, so a copy that missed a cast cannot end up inverted.
        if (ability is BeamAbility beam && toggledAbilitiesDict.ContainsKey(beam) == cast.toggledOn) return false;

        return PerformCast(ability, cast.targetPosition, cast.aimRotation);
    }

    /// <summary>True while a beam is switched on.</summary>
    public bool IsToggledOn(BeamAbility beam) => toggledAbilitiesDict.ContainsKey(beam);

    /// <summary>Host only: charges the power for a cast a client made, as the client did when it cast.</summary>
    public void PayReplayedCastCost(Ability ability)
    {
        statsHandler?.ChangeAttribute(AttributeType.CurrentPower, -GetModifiedCost(ability));
    }

    private bool ActivateTriggered(Ability ability)
    {
        return ability switch
        {
            TeleportAbility teleport => teleport.Activate(statsHandler, transform.position),
            ConjureAbility conjure and not DashAbility => conjure.Activate(statsHandler, transform.rotation),
            _ => ability.Activate(statsHandler)
        };
    }

    /// <summary>The part of a cast that every machine runs: ability objects, animation and sound.</summary>
    private bool PerformCast(Ability ability, Vector3 targetPosition, Quaternion rotation)
    {
        //when casting a beam which is already toggled on, toggle beam off.
        if (ability is not BeamAbility) StopAllToggledAbilities();
        bool usedAbility = HandleAbilityType(ability, targetPosition, rotation);

        PlayCastSound(ability);

        if (usedAbility && NetworkSession.IsAuthority) ApplyIonization();
        return usedAbility;
    }

    private void ApplyIonization()
    {
        int ionizationValue = statsHandler.SE_Handler.GetStatusEffectValue(StatusEffectType.Ionized);
        if (ionizationValue > 0)
        {
            statsHandler.ChangeAttributeByPercent(AttributeType.CurrentHitpoints, (float)-.01 * ionizationValue);
            EventDeclarer.IonizedAttack?.Invoke(statsHandler);
        }
    }


    bool HandleAbilityType(Ability ability, Vector3 targetPosition, Quaternion rotation)
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
                usedAbility = HandleAuraAbility(auraAbility);
                break;
            case TeleportAbility teleportAbility:
                if (anim != null) anim.ChangeLayerAnimation("Buff", 1, 2f, true);
                usedAbility = teleportAbility.Activate(statsHandler, targetPosition);
                break;
            case MeleeAbility meleeAbility:
                if (anim != null) anim.ChangeLayerAnimation("Buff", 1, 2f, true);
                usedAbility = meleeAbility.Activate(statsHandler);
                break;
            case BeamAbility beamAbility:
                usedAbility = HandleBeamAbility(beamAbility, statsHandler);
                break;
            case ChargeAbility chargeAbility:
                if (!charging)
                {
                    SetCharging(true);
                    usedAbility = chargeAbility.Activate(statsHandler);
                }
                break;
        }
        return usedAbility;
    }
    private bool HandleBeamAbility(BeamAbility beamAbility, LivingBeing statsHandler)
    {
        GameObject beamInstance;
        //if (IsOnCoolDown(beamAbility)) return false;
        if (toggledAbilitiesDict.TryGetValue(beamAbility, out GameObject activeAbility))
        {
            //StartCoroutine(SetOnCooldown(beamAbility));
            StopToggledAbility(beamAbility, activeAbility);
            if (anim != null) anim.ChangeLayerAnimation("Reload", 1, 2f, true);

            return false;
        }
        else
        {
            if (anim != null) anim.ChangeLayerAnimation("BeamStart", 1, 2f, true);

            beamInstance = beamAbility.ToggleBeam(statsHandler, abilitySpawn.transform);
            toggledAbilitiesDict.TryAdd(beamAbility, beamInstance);
            statsHandler.ChangeRegeneration(beamAbility.CostType, -GetModifiedCost(beamAbility));
            //AbilityToggledRecently = true;
            return true;
        }
    }
    /// <summary>The beams that are switched on right now.</summary>
    public IEnumerable<BeamAbility> ActiveBeams => toggledAbilitiesDict.Keys;

    public void SetCharging(bool alreadyCharging)
    {
        if (anim != null) anim.ChangeLayerAnimation("Sprint", 1, .1f);

        charging = alreadyCharging;
    }
    public void StopAllToggledAbilities()
    {
        //if (anim != null) anim.SeLayerWeight(1, 0);
        foreach (KeyValuePair<BeamAbility, GameObject> kvp in toggledAbilitiesDict)
        {
            if (HasCastAuthority && GetComponent<PlayerStats>() != null) PotionHandler.SpawnElementalPotionOnBelt(Abilities.IndexOf(kvp.Key), kvp.Key);
            statsHandler.ChangeRegeneration(kvp.Key.CostType, GetModifiedCost(kvp.Key));

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
        statsHandler.ChangeRegeneration(beamAbility.CostType, GetModifiedCost(beamAbility));
        toggledAbilitiesDict.Remove(beamAbility);
        //AbilityToggledRecently = false;
        Destroy(activeAbility);
    }

    public float GetModifiedCost(Ability ability)
    {
        if (ability == null) return 0f;
        if (modHandler == null) modHandler = AbilityModHandler.Instance;
        if (modHandler == null) return ability.Cost;
        return Mathf.Max(0f, ability.Cost + modHandler.GetModAttributeByType(ability, AbilityModTypes.Cost));
    }

    public void HandleNoMana()
    {
        // Copies of a being on other machines wait for its owner to say the beams have stopped.
        if (!HasCastAuthority || toggledAbilitiesDict.Count == 0) return;
        // Same as turning the beams off by hand, so their power drain is given back.
        StopAllToggledAbilities();
        if (networkCombatant != null) networkCombatant.ShareToggledAbilitiesCleared();
    }

    bool HandleProjectile(ProjectileAbility ability)
    {
        //Debug.Log("Time to see the heavy throw animation");
        if (anim != null) anim.ChangeLayerAnimation("OneArmedThrow", 1, 3f);

        return ability.Activate(statsHandler);
    }

    bool HandlePointAndClick(TargetMouseAbility ability)
    {
        if (anim != null) anim.ChangeLayerAnimation("Buff", 1, 3f);

        return ability.Activate(statsHandler);
    }

    bool HandleConjureAbility(ConjureAbility ability, Vector2 targetPosition, Quaternion rotation)
    {
        if (anim != null) anim.ChangeLayerAnimation("HeavyThrow", 1, 3);

        return ability.Activate(statsHandler, rotation);
    }

    bool HandleDashAbility(DashAbility dashAbility)
    {
        if (anim != null) anim.ChangeLayerAnimation("Sprint", 1, .1f);
        return dashAbility.Activate(statsHandler);
    }
    bool HandleAuraAbility(AuraAbility auraAbility)
    {
        if (anim != null) anim.ChangeLayerAnimation("Buff", 1, 3f);

        return auraAbility.Activate(statsHandler);
    }

    public void AddAbilityCooldownSlot(Ability ability = null)
    {
        if (ability != null)
        {
            abilitiesOnCooldown.TryAdd(ability, false);
        }
    }
    public bool CheckAbilityAlreadySlotted(Ability ability)
    {
        bool slotted = false;
        foreach (var slot in SlottedAbilities)
        {
            if (slot.ability == ability)
            {
                return true;
            }
        }
        return slotted;
    }

    public IEnumerator SetOnCooldown(Ability ability)
    {
        try
        {
            float coolDown = GetCooldownDuration(ability);
            abilitiesOnCooldown[ability] = true;

            yield return new WaitForSeconds(coolDown);
        }
        finally
        {
            abilitiesOnCooldown[ability] = false;

        }
    }

    /// <summary>The ability's cooldown, plus its mods and the caster's lethargy.</summary>
    public float GetCooldownDuration(Ability ability)
    {
        if (modHandler == null) modHandler = AbilityModHandler.Instance;
        float modCooldown = modHandler != null ? modHandler.GetModAttributeByType(ability, AbilityModTypes.Cooldown) : 0f;
        float lethargy = statsHandler != null && statsHandler.SE_Handler != null ? statsHandler.SE_Handler.GetStatusEffectValue(StatusEffectType.Lethargic) : 0f;
        return ability.Cooldown + modCooldown + lethargy;
    }

    protected virtual bool IsOnCoolDown(Ability ability)
    {
        return abilitiesOnCooldown[ability];
    }
    public string GetKnownAbilitiesString()
    {
        string KnownAbilities = "Known Abilities:\n";
        foreach (Ability ability in statsHandler.abilityHandler.Abilities)
        {
            KnownAbilities += $"{ability.Name} \n";
        }
        return KnownAbilities;
    }

    private void PlayCastSound(Ability ability)
    {
        if (audioHandler == null) audioHandler = GetComponent<LivingBeingAudioHandler>();
        if (audioHandler != null && ability.Sounds != null && ability.Sounds.CastSounds.Length != 0)
        {
            audioHandler.PlayAbilityCastSound(ability);
        }
    }

}

