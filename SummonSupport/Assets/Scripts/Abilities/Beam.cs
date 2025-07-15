using System;
using Unity.VisualScripting;
using UnityEngine;

public class Beam : MonoBehaviour
{
    private ParticleSystem ParticleSystem;
    private LivingBeing Caster;

    private Ability Ability;

    private Transform abilityRotation;

    [field: SerializeField] public float Range { get; private set; }
    private static readonly Quaternion Rotation90Z = Quaternion.Euler(0, 0, -90);



    void Start()
    {
        SetParticleSettings();
    }

    void OnParticleCollision(GameObject other)
    {
        Debug.Log($"particle collided with {other.name}");
    }
    void SetParticleSettings()
    {
        ParticleSystem = GetComponent<ParticleSystem>();
        var collision = ParticleSystem.collision;
        collision.mode = ParticleSystemCollisionMode.Collision2D; // Use 2D collisions
        InvokeRepeating("UpdateRotation", 0f, .1f);

    }

    public void SetAbilityAndCaster(LivingBeing caster, Ability ability, Transform rotationObject)
    {
        abilityRotation = rotationObject;
        Caster = caster;
        Ability = ability;
    }
    private void UpdateRotation()
    {
        Debug.Log("rotation func being called");
        transform.rotation = abilityRotation.rotation * Rotation90Z;
    }


}
