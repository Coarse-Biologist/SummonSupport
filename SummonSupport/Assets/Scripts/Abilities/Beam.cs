using System;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;

public class Beam : MonoBehaviour
{
    private ParticleSystem ParticleSystem;
    private LivingBeing Caster;

    private BeamAbility Ability;

    private Transform abilityRotation;
    private List<LivingBeing> AlreadyCollided = new();

    private float Range = 5f;

    private float deletionDelay = 5f;
    private float TickRate = .5f;
    private float speedMultiplier = 1f;

    private bool onCoolDown = false;



    void Start()
    {
        SetParticleSettings();
    }

    void OnParticleCollision(GameObject other)
    {
        if (!onCoolDown)
        {
            if (other.TryGetComponent(out LivingBeing targetStatsHandler) && !AlreadyCollided.Contains(targetStatsHandler))
            {

                onCoolDown = true;
                Invoke("OffCoolDown", TickRate);
                AlreadyCollided.Add(targetStatsHandler);
                GameObject hitEffect = Instantiate(Ability.OnHitEffect, other.transform.position, Quaternion.identity, other.transform);
                Destroy(hitEffect, 5f);
                StartCoroutine(DelayedDeletion(targetStatsHandler));
                if (Ability.ThoroughIsUsableOn(Caster, targetStatsHandler))
                    CombatStatHandler.HandleEffectPackage(Ability, Caster, targetStatsHandler, Ability.TargetEffects);
            }
        }
    }

    private void OffCoolDown()
    {
        onCoolDown = false;
    }

    void SetParticleSettings()
    {
        ParticleSystem = GetComponent<ParticleSystem>();
        if (ParticleSystem == null)
        {
            Debug.LogError($"{name} is missing its ParticleSystem and cannot act as a beam.");
            return;
        }

        var main = ParticleSystem.main;
        main.simulationSpeed *= speedMultiplier;

        var collision = ParticleSystem.collision;
        collision.mode = ParticleSystemCollisionMode.Collision3D; // Use 3D collisions
        InvokeRepeating("UpdateRotation", 0f, .1f);

    }

    public void SetAbilitySettings(LivingBeing caster, BeamAbility ability, Transform rotationObject)
    {
        TickRate = ability.TickRate;
        Range = ability.Range;
        deletionDelay = Mathf.Max(5f, ability.Duration);
        speedMultiplier = 1f;

        if (caster != null && caster.CharacterTag != CharacterTag.Enemy && AbilityModHandler.Instance != null)
        {
            AbilityModHandler modHandler = AbilityModHandler.Instance;
            int sizeMod = modHandler.GetModAttributeByType(ability, AbilityModTypes.Size);
            int speedMod = modHandler.GetModAttributeByType(ability, AbilityModTypes.Speed);
            int durationMod = modHandler.GetModAttributeByType(ability, AbilityModTypes.Duration);

            TickRate = Mathf.Max(.05f, ability.TickRate - speedMod * .05f);
            Range += sizeMod;
            deletionDelay += durationMod;
            speedMultiplier = 1f + speedMod * .1f;
            transform.localScale *= 1f + sizeMod * .15f;
        }

        abilityRotation = rotationObject;
        //Debug.Log($"does the rotation object exist? ({rotationObject})");
        Caster = caster;
        Ability = ability;
        AlreadyCollided.Clear();
        if (caster != null) AlreadyCollided.Add(caster);
    }

    private void UpdateRotation()
    {
        //Debug.Log("rotation func being called");
        if (abilityRotation != null)
            transform.rotation = abilityRotation.rotation;// * Rotation90Z;
        else Debug.Log("You are trying to use ability rotation but for some reason it doesnt exist.");
    }

    private IEnumerator DelayedDeletion(LivingBeing collidedObject)
    {
        yield return deletionDelay;
        AlreadyCollided.Remove(collidedObject);
    }




}
