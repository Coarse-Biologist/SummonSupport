using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
//using System.Numerics;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(Collider))]
public class Projectile : MonoBehaviour
{
    [field: SerializeField]
    public Collider physicsCollider { private set; get; } = null;
    List<GameObject> ignoreGameObjects = new List<GameObject>();
    public GameObject SpawnEffectOnHit { get; set; }
    private GameObject particleTrailSystem;

    public ProjectileAbility ability;
    public int piercedAlready { private set; get; } = 0;
    public int ricochedAlready { private set; get; } = 0;
    public bool splitAlready = false;

    int split = 0;
    int pierce = 5;
    int ricochet = 0;
    LivingBeing userLivingBeing = null;
    private bool active = false;

    public AbilityModHandler modHandler { private set; get; }
    public Mod_Base projectileMod { private set; get; }
    Rigidbody rb;
    float speed;



    public void SetActive(ProjectileAbility ab, LivingBeing lb, AbilityModHandler handler = null)
    {
        ability = ab;

        userLivingBeing = lb;
        modHandler = handler;
        if (modHandler != null)
        {
            projectileMod = modHandler.GetAbilityMod(ability);
        }

        ignoreGameObjects.Add(lb.gameObject);
        active = true;

    }


    public void SetProjectilePhysics(Vector3 newDirection)
    {

        rb = GetComponent<Rigidbody>();
        speed = ability.Speed;
        if (modHandler != null)
        {
            speed += modHandler.GetModAttributeByType(ability, AbilityModTypes.Speed);
        }

        IgnoreCasterCollider();

        rb.linearVelocity = newDirection * speed;

        float angle = Mathf.Atan2(newDirection.x, newDirection.z) * Mathf.Rad2Deg;

        transform.rotation = Quaternion.Euler(new Vector3(0, angle + 90, 0));

    }

    private void IgnoreCasterCollider()
    {
        Physics.IgnoreCollision(rb.GetComponent<Collider>(), userLivingBeing.GetComponent<Collider>(), true);
    }
    public void SetEffects(GameObject effectOnHit)
    {
        SpawnEffectOnHit = effectOnHit;
    }
    public void HandleRicochet()
    {
        if (ricochedAlready >= ricochet)
        {
            Debug.Log($"destroying because: ricocheted already = {ricochedAlready}. max ricochet = {ricochet}");
            Destroy(gameObject);

        }
        else if (Physics.Raycast(transform.position, rb.linearVelocity.normalized, out RaycastHit hit, 1f))
        {
            Debug.Log("ray cast hits in handle ricochet func");
            Vector3 normal = hit.normal;
            Vector3 incomingV = rb.linearVelocity;
            Vector3 reflectDir = Vector3.Reflect(incomingV.normalized, normal);
            rb.linearVelocity = reflectDir * speed;
            SpawnEffect(transform.position);
            ReorientSpin();
            ricochedAlready++;
        }

    }
    private void ReorientSpin()
    {
        if (particleTrailSystem != null) Destroy(particleTrailSystem);

        SetParticleTrailEffects(rb.linearVelocity);
    }

    private void SpawnEffect(Transform targetTransform = null)
    {
        GameObject instance;
        if (ability.SpawnEffectOnHit != null)
        {
            if (targetTransform == transform)
                instance = Instantiate(ability.SpawnEffectOnHit, targetTransform.position, Quaternion.identity);
            else
            {
                instance = Instantiate(ability.SpawnEffectOnHit, targetTransform.position, Quaternion.identity, targetTransform);
            }
            Destroy(instance, instance.GetComponent<ParticleSystem>().main.duration);
        }
    }
    private void SpawnEffect(Vector2 loc)
    {
        GameObject instance;
        if (ability.SpawnEffectOnHit != null)
        {
            instance = Instantiate(ability.SpawnEffectOnHit, loc, Quaternion.identity);
            Destroy(instance, instance.GetComponent<ParticleSystem>().main.duration);
        }
    }
    public GameObject SetParticleTrailEffects(Vector3 direction) // -user.transform.right
    {
        GameObject particleSystem = null;
        if (ability.ProjectileParticleSystem != null)
        {
            particleSystem = Instantiate(ability.ProjectileParticleSystem, gameObject.transform.position, Quaternion.identity, gameObject.transform);
            Quaternion rotation = Quaternion.LookRotation(-direction);
            particleSystem.transform.rotation = rotation;

        }
        particleTrailSystem = particleSystem;
        return particleSystem;
    }

    void OnTriggerEnter(Collider other)
    {

        if (active)
        {
            if (other.gameObject.TryGetComponent(out NavMeshObstacle obstacle))
            {
                Debug.Log("Box collider entered");
                HandleRicochet();
                SpawnEffect(transform);
            }
            else if (other.gameObject.TryGetComponent(out Projectile otherProjectileScript) || ignoreGameObjects.Contains(other.gameObject))
                return;

            else if (!other.TryGetComponent(out LivingBeing otherLivingBeing))
            {
                Destroy(gameObject);
                return;
            }

            else if (!userLivingBeing || (!Ability.HasElementalSynergy(ability, otherLivingBeing) && !ability.ThoroughIsUsableOn(userLivingBeing, otherLivingBeing)))
                return;
            else
            {
                SpawnEffect(otherLivingBeing.transform);
                CombatStatHandler.HandleEffectPackage(ability, userLivingBeing, otherLivingBeing, ability.TargetEffects);
                HandleOnHitBehaviour(otherLivingBeing);
            }
        }

    }

    void HandleOnHitBehaviour(LivingBeing other, Collision col = null)
    {
        if (modHandler != null)
        {
            split = modHandler.GetModAttributeByType(ability, AbilityModTypes.MaxSplit);
            pierce = modHandler.GetModAttributeByType(ability, AbilityModTypes.MaxPierce);
            ricochet = modHandler.GetModAttributeByType(ability, AbilityModTypes.MaxRicochet);
        }

        if (!splitAlready && (ability.PiercingMode == OnHitBehaviour.Split || split > 0))
        {
            SplitProjectile(other, split);
        }
        else if (piercedAlready < ability.MaxPierce + pierce)
        {
            piercedAlready++;
        }
        else if (ability.PiercingMode == OnHitBehaviour.Ricochet || ricochet > 0)
        {
            if (ricochedAlready < ricochet)
            {

                HandleRicochet();
            }
        }
        else
        {

            Destroy(gameObject);
        }
    }




    void SplitProjectile(LivingBeing other, int splits)
    {
        splitAlready = true;
        int maxSplit = ability.MaxSplit + splits + 1;

        //int left = -1;
        for (int i = 0; i < maxSplit; i += 1) // i starts at -1, code block is completed as long as i is less than or equal to one. upon completion it goes up by 2. the code block will therefore happen once?
        {

            Quaternion rotation;
            rotation = Quaternion.Euler(0, 0, (float)Math.Sin(45 * i) * (30 + 5 * i));

            Vector3 direction = rotation * transform.forward;
            GameObject newProjectile = Instantiate(ability.Projectile, transform.position, Quaternion.identity);
            Projectile projectileScript = newProjectile.GetComponent<Projectile>();

            Material glowMaterial = ColorChanger.GetGlowByElement(ability.ElementTypes[0]);

            ColorChanger.ChangeMatByAffinity(newProjectile.GetComponent<Renderer>(), glowMaterial);
            projectileScript.ignoreGameObjects = new List<GameObject>(ignoreGameObjects) { other.gameObject };

            projectileScript.splitAlready = this.splitAlready;
            projectileScript.SetActive(ability, userLivingBeing, modHandler);
            projectileScript.SetProjectilePhysics(direction);
            projectileScript.SetParticleTrailEffects(direction);
            Destroy(newProjectile, ability.Lifetime);
        }
    }

}




