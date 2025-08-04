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




    public void Shoot(GameObject user, GameObject spawnAt = null, Vector3 lookAt = default(Vector3))
    {
        if (user.TryGetComponent(out LivingBeing livingBeing))
            userLivingBeing = livingBeing;
        else
            Debug.Log($"user isnt a living being, weird right?");
        ignoreGameObjects.Add(user);
        SetProjectilePhysics(spawnAt, lookAt);
        Destroy(gameObject, ability.Lifetime); // TODO: change from lifetime to range
    }



    void SetProjectilePhysics(GameObject spawnPoint)
    {
        SetProjectilePhysics(spawnPoint, Vector3.zero);
    }
    void SetProjectilePhysics(GameObject spawnPoint, Vector3 direction)
    {
        if (direction == Vector3.zero)
            direction = spawnPoint.transform.right;
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        rb.linearVelocity = direction * ability.Speed;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle));
    }
    public void SetEffects(GameObject effectOnHit)
    {
        SpawnEffectOnHit = effectOnHit;
    }

    private void SpawnEffect(LivingBeing targetLivingBeing)
    {
        GameObject instance;
        if (SpawnEffectOnHit != null)
        {
            instance = Instantiate(ability.SpawnEffectOnHit, targetLivingBeing.transform.position, Quaternion.identity, targetLivingBeing.transform.transform);
            Destroy(instance, 3f);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Projectile") || ignoreGameObjects.Contains(other.gameObject))
            return;

        if (!other.TryGetComponent(out LivingBeing otherLivingBeing))
        {
            SpawnEffect(otherLivingBeing);
            DestroyProjectile();
            return;
        }

        if (!userLivingBeing || (!Ability.HasElementalSynergy(ability, otherLivingBeing) && !ability.IsUsableOn(userLivingBeing.CharacterTag, otherLivingBeing.CharacterTag)))
            return;

        Debug.Log($"ability = {ability.name}.");
        Debug.Log($"caster = {userLivingBeing}.");
        Debug.Log($" target = {otherLivingBeing}");

        CombatStatHandler.HandleEffectPackages(ability, userLivingBeing, otherLivingBeing, false);
        HandleOnHitBehaviour(otherLivingBeing);

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
            }
        }
        DestroyProjectile(other);
    }



}
