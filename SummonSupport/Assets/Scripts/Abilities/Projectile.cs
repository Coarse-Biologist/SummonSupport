using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    List<GameObject> ignoreGameObjects = new List<GameObject>();
    public GameObject SpawnEffectOnHit { get; set; }

    public ProjectileAbility ability;
    public int piercedAlready = 0;
    public int splitAlready = 0;
    LivingBeing userLivingBeing = null;
    private bool active = false;




    public void Shoot(GameObject user, GameObject spawnAt = null, Vector3 lookAt = default(Vector3))
    {
        if (user.TryGetComponent(out LivingBeing livingBeing))
            userLivingBeing = livingBeing;
        //else
        //Debug.Log($"user isnt a living being, weird right?");
        ignoreGameObjects.Add(user);
        SetProjectilePhysics(spawnAt, lookAt);
        Destroy(gameObject, ability.Lifetime); // TODO: change from lifetime to range
        SetActive();
    }
    private void SetActive()
    {
        active = true;
    }



    void SetProjectilePhysics(GameObject spawnPoint)
    {
        SetProjectilePhysics(spawnPoint, Vector3.zero);
    }
    void SetProjectilePhysics(GameObject spawnPoint, Vector3 newDirection)
    {
        if (newDirection == Vector3.zero)
            newDirection = spawnPoint.transform.right;
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        rb.linearVelocity = newDirection * ability.Speed;

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
            //Debug.Log("This happens, excellent");
            instance = Instantiate(ability.SpawnEffectOnHit, transform.position, Quaternion.identity, targetLivingBeing.transform.transform);
            Destroy(instance, instance.GetComponent<ParticleSystem>().main.duration);
        }
        //else Debug.Log("This happens but is null");
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
                DestroyProjectile();
                return;
            }

            if (!userLivingBeing || (!Ability.HasElementalSynergy(ability, otherLivingBeing) && !ability.ThoroughIsUsableOn(userLivingBeing, otherLivingBeing)))
                return;

            SpawnEffect(otherLivingBeing, ability.SpawnEffectOnHit);
            CombatStatHandler.HandleEffectPackages(ability, userLivingBeing, otherLivingBeing, false);
            HandleOnHitBehaviour(otherLivingBeing);
        }

    }

    void HandleOnHitBehaviour(LivingBeing other)
    {
        switch (ability.PiercingMode)
        {
            case OnHitBehaviour.Ricochet:
                break;
            case OnHitBehaviour.Pierce:
                piercedAlready++;
                if (piercedAlready == ability.MaxPierce)
                    DestroyProjectile(other);
                break;
            case OnHitBehaviour.Split:
                SplitProjectile(other);
                break;
            case OnHitBehaviour.Destroy:
                DestroyProjectile(other);
                break;
        }
    }

    void DestroyProjectile(LivingBeing other = null)
    {
        //probably casue effects if hitting an acceptable enemy?

        Destroy(gameObject);
    }
    void SplitProjectile(LivingBeing other)
    {
        if (splitAlready >= ability.MaxSplit)
            DestroyProjectile(other);
        else
        {
            splitAlready++;
            for (int i = -1; i <= 1; i += 2) // -1 and +1
            {
                Quaternion rotation = Quaternion.Euler(0, 0, i * ability.SplitAngleOffset);
                Vector3 direction = rotation * transform.right;

                GameObject newProjectile = Instantiate(ability.Projectile, transform.position, Quaternion.identity);
                Projectile projectileScript = newProjectile.GetComponent<Projectile>();
                projectileScript.ignoreGameObjects = new List<GameObject>(ignoreGameObjects) { other.gameObject };
                projectileScript.ability = ability;
                projectileScript.splitAlready = this.splitAlready;
                projectileScript.SetProjectilePhysics(gameObject, direction);
                projectileScript.SetParticleTrailEffects(direction);
                Destroy(newProjectile, ability.Lifetime);


            }
        }
        DestroyProjectile(other);
    }



}
