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

    private bool onCoolDown = false;



    void Start()
    {
        SetParticleSettings();
    }

    void OnParticleCollision(GameObject other)
    {
        if (other.TryGetComponent(out LivingBeing targetStatsHandler) && !AlreadyCollided.Contains(targetStatsHandler))
        {
            if (!onCoolDown)
            {
                onCoolDown = true;
                Invoke("OffCoolDown", TickRate);
                AlreadyCollided.Add(targetStatsHandler);
                GameObject hitEffect = Instantiate(Ability.SpawnEffectOnHit, other.transform.position, Quaternion.identity, other.transform);
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
        var collision = ParticleSystem.collision;
        collision.mode = ParticleSystemCollisionMode.Collision3D; // Use 3D collisions
        InvokeRepeating("UpdateRotation", 0f, .1f);

    }

    public void SetAbilitySettings(LivingBeing caster, BeamAbility ability, Transform rotationObject)
    {
        TickRate = ability.TickRate;
        Range = ability.Range;
        abilityRotation = rotationObject;
        Caster = caster;
        Ability = ability;
    }

    private void UpdateRotation()
    {
        //Debug.Log("rotation func being called");
        transform.rotation = abilityRotation.rotation;// * Rotation90Z;
    }

    private IEnumerator DelayedDeletion(LivingBeing collidedObject)
    {
        yield return deletionDelay;
        AlreadyCollided.Remove(collidedObject);
    }




}
