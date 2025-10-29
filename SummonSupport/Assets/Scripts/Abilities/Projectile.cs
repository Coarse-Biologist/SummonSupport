using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    List<GameObject> ignoreGameObjects = new List<GameObject>();
    public GameObject SpawnEffectOnHit { get; set; }

    public ProjectileAbility ability;
    public int piercedAlready = 0;
    public bool splitAlready = false;
    LivingBeing userLivingBeing = null;
    private bool active = false;

    private AbilityModHandler modHandler;
    private Mod_Base projectileMod;



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
    public void SetActive(GameObject user = null)
    {
        if (user.TryGetComponent(out LivingBeing livingBeing))
            userLivingBeing = livingBeing;
        ignoreGameObjects.Add(user);
        active = true;
    }




    public void SetProjectilePhysics(GameObject spawnPoint, Vector3 newDirection)
    {
        if (newDirection == Vector3.zero)
            newDirection = spawnPoint.transform.right;
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
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
            projectileScript.ability = ability;
            projectileScript.splitAlready = this.splitAlready;
            projectileScript.SetProjectilePhysics(gameObject, direction);
            projectileScript.SetParticleTrailEffects(direction);
            projectileScript.SetActive();
            Destroy(newProjectile, ability.Lifetime);
        }
    }

}




