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

    [field: SerializeField] public float Range { get; private set; }

    private float deletionDelay = 5f;
    [field: SerializeField] float TickRate = .5f;
    private bool onCoolDown = false;



    void Start()
    {
        SetParticleSettings();
    }

    void OnParticleCollision(GameObject other)
    {
        if (other.TryGetComponent(out LivingBeing statsHandler) && !AlreadyCollided.Contains(statsHandler))
        {
            if (!onCoolDown)
            {
                onCoolDown = true;
                Invoke("OffCoolDown", TickRate);
                Debug.Log($"particle collided with {statsHandler.Name}");
                AlreadyCollided.Add(statsHandler);
                GameObject hitEffect = Instantiate(Ability.SpawnEffectOnHit, other.transform.position, Quaternion.identity, other.transform);
                Destroy(hitEffect, 5f);
                StartCoroutine(DelayedDeletion(statsHandler));
                CombatStatHandler.AdjustDamageValue(Ability, statsHandler, Caster);
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
        collision.mode = ParticleSystemCollisionMode.Collision2D; // Use 2D collisions
        InvokeRepeating("UpdateRotation", 0f, .1f);

    }

    public void SetAbilityAndCaster(LivingBeing caster, BeamAbility ability, Transform rotationObject)
    {
        abilityRotation = rotationObject;
        Caster = caster;
        Ability = ability;
    }

    private void UpdateRotation()
    {
        Debug.Log("rotation func being called");
        transform.rotation = abilityRotation.rotation;// * Rotation90Z;
    }

    private IEnumerator DelayedDeletion(LivingBeing collidedObject)
    {
        yield return deletionDelay;
        AlreadyCollided.Remove(collidedObject);
    }




}
