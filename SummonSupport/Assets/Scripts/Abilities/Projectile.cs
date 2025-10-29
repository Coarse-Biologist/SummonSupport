using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Projectile : MonoBehaviour
{
    List<GameObject> ignoreGameObjects = new List<GameObject>();
    public GameObject SpawnEffectOnHit { get; set; }

    public ProjectileAbility ability;
    public int piercedAlready { private set; get; } = 0;
    public int ricochedAlready { private set; get; } = 0;

    public bool splitAlready = false;
    LivingBeing userLivingBeing = null;
    private bool active = false;

    public AbilityModHandler modHandler { private set; get; }
    public Mod_Base projectileMod { private set; get; }
    Rigidbody2D rb;



    public void Shoot(GameObject user, GameObject spawnAt = null, Vector3 lookAt = default(Vector3))
    {
        if (user.TryGetComponent(out LivingBeing livingBeing))
            userLivingBeing = livingBeing;
        if (user.TryGetComponent(out AbilityModHandler handler))
        {
            modHandler = handler;
        }
        //else
        //Debug.Log($"user isnt a living being, weird right?");
        ignoreGameObjects.Add(user);
        SetProjectilePhysics(spawnAt, lookAt);
        Destroy(gameObject, ability.Lifetime); // TODO: change from lifetime to range
    }
    public void SetActive(ProjectileAbility ab, LivingBeing lb, AbilityModHandler handler = null)
    {
        ability = ab;

        userLivingBeing = lb;
        modHandler = handler;
        if (modHandler != null)
        {
            projectileMod = modHandler.GetAbilityMod(ability);
        }
        // set ricochet properties if it should.
        if (ability.PiercingMode == OnHitBehaviour.Ricochet || (projectileMod != null && projectileMod.GetModdedAttribute(AbilityModTypes.MaxRicochet) > 0))
        {
            if (ricochedAlready < projectileMod.GetModdedAttribute(AbilityModTypes.MaxRicochet))
            {
                Debug.Log($"physics mat is being assigned");
                PhysicsMaterial2D mat = new PhysicsMaterial2D();
                mat.bounciness = 2f;
                mat.friction = 0f;
                GetComponent<Collider2D>().sharedMaterial = mat;
            }
        }
        ignoreGameObjects.Add(lb.gameObject);
        active = true;

    }




    public void SetProjectilePhysics(GameObject spawnPoint, Vector3 newDirection)
    {
        if (newDirection == Vector3.zero)
            newDirection = spawnPoint.transform.right;
        rb = GetComponent<Rigidbody2D>();
        float speed = ability.Speed;
        if (modHandler != null)
        {
            speed += modHandler.GetModAttributeByType(ability, AbilityModTypes.Speed);
        }
        rb.linearVelocity = newDirection * speed;

        float angle = Mathf.Atan2(newDirection.y, newDirection.x) * Mathf.Rad2Deg;

        transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle));

    }
    public void SetEffects(GameObject effectOnHit)
    {
        SpawnEffectOnHit = effectOnHit;
    }

    public void IncrementRicochet()
    {
        if (projectileMod == null)
        {
            Destroy(gameObject);
        }
        else if (ricochedAlready >= projectileMod.GetModdedAttribute(AbilityModTypes.MaxRicochet))
        {
            Destroy(gameObject);
        }
        else
        {
            ReorientSpin();
            ricochedAlready++;
        }
    }
    private void ReorientSpin()
    {
        Vector2 v = rb.linearVelocity;
        if (v.sqrMagnitude > 0.01f)
        {
            float angle = Mathf.Atan2(v.y, v.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }
    }

    private void SpawnEffect(LivingBeing targetLivingBeing, GameObject SpawnEffectOnHit)
    {
        GameObject instance;
        if (SpawnEffectOnHit != null)
        {
            Debug.Log($"The on hit effect for {ability.Name} is being spawned");
            instance = Instantiate(ability.SpawnEffectOnHit, targetLivingBeing.transform.position, Quaternion.identity, targetLivingBeing.transform.transform);
            Destroy(instance, instance.GetComponent<ParticleSystem>().main.duration);
        }
        else Debug.Log($"there is no on hit effect for {ability.Name}");
    }
    public void SetParticleTrailEffects(Vector2 direction) // -user.transform.right
    {
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        GameObject particleSystem;
        if (ability.ProjectileParticleSystem != null)
        {
            particleSystem = Instantiate(ability.ProjectileParticleSystem, gameObject.transform.position, Quaternion.identity, gameObject.transform);
            //particleSystem.transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle));
            Quaternion rotation = Quaternion.LookRotation(-direction);
            particleSystem.transform.rotation = rotation;

        }

    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (active)
        {
            if (other.gameObject.CompareTag("Projectile") || ignoreGameObjects.Contains(other.gameObject))
                return;

            if (!other.TryGetComponent(out LivingBeing otherLivingBeing))
            {
                //DestroyProjectile();
                return;
            }

            if (!userLivingBeing || (!Ability.HasElementalSynergy(ability, otherLivingBeing) && !ability.ThoroughIsUsableOn(userLivingBeing, otherLivingBeing)))
                return;

            SpawnEffect(otherLivingBeing, ability.SpawnEffectOnHit);
            CombatStatHandler.HandleEffectPackages(ability, userLivingBeing, otherLivingBeing, false);
            HandleOnHitBehaviour(otherLivingBeing);
        }
        else Debug.Log("Not active and therefore just a moving sprite");

    }

    void HandleOnHitBehaviour(LivingBeing other)
    {
        bool splitMode = false;
        Debug.Log($"Handling On hit behavior");

        if (!splitAlready)
            if (ability.PiercingMode == OnHitBehaviour.Split || modHandler != null && modHandler.GetModAttributeByType(ability, AbilityModTypes.MaxSplit) != 0)
            {
                SplitProjectile(other);
                splitMode = true;
            }
        if (ability.PiercingMode == OnHitBehaviour.Pierce || (modHandler != null && modHandler.GetModAttributeByType(ability, AbilityModTypes.MaxPierce) != 0))
        {

            if (piercedAlready == modHandler.GetModAttributeByType(ability, AbilityModTypes.MaxPierce) + ability.MaxPierce)
            {
                if (!splitMode || splitAlready)
                {
                    Debug.Log($"pierced already = {piercedAlready}.");
                    Destroy(gameObject);
                }
            }
            else
                piercedAlready++;
        }
        else Destroy(gameObject);
    }




    void SplitProjectile(LivingBeing other)
    {
        splitAlready = true;
        int maxSplit = ability.MaxSplit;
        int totalAngle = ability.SplitAngleOffset;
        if (modHandler != null)
        {
            maxSplit += modHandler.GetModAttributeByType(ability, AbilityModTypes.MaxSplit) + 1;
        }
        //int left = -1;
        for (int i = 0; i < maxSplit; i += 1) // i starts at -1, code block is completed as long as i is less than or equal to one. upon completion it goes up by 2. the code block will therefore happen once?
        {

            Debug.Log("for loop is being carried out in the split func of the projectile script");
            Quaternion rotation;
            rotation = Quaternion.Euler(0, 0, (float)Math.Sin(45 * i) * (10 + 5 * i));

            Vector3 direction = rotation * transform.right;
            GameObject newProjectile = Instantiate(ability.Projectile, transform.position, Quaternion.identity);
            Projectile projectileScript = newProjectile.GetComponent<Projectile>();
            projectileScript.ignoreGameObjects = new List<GameObject>(ignoreGameObjects) { other.gameObject };

            projectileScript.splitAlready = this.splitAlready;
            projectileScript.SetActive(ability, userLivingBeing, modHandler);
            projectileScript.SetProjectilePhysics(gameObject, direction);
            projectileScript.SetParticleTrailEffects(direction);
            Destroy(newProjectile, ability.Lifetime);
        }
    }

}




