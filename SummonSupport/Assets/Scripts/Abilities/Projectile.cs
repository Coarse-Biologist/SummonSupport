using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor.Rendering;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public List<GameObject> ignoreGameObjects = new List<GameObject>();
    public ProjectileAbility ability;
    public int piercedAlready = 0;
    public int splitAlready = 0;

    public void Shoot(GameObject user, GameObject spawnAt = null, Vector3 lookAt = default(Vector3))
    {
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

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Projectile") || ignoreGameObjects.Contains(other.gameObject))
            {
                Logging.Verbose($"{ability.Name} ignores {other.name}");
                return;
            }
        else if (other.gameObject.GetComponent<LivingBeing>() != null)
        {
            foreach(OnEventDo onHitDo in ability.ListOnHitDo)
                {
                    Logging.Verbose($"apply {onHitDo} to {other.name}");
                    HandleOnEventDo(onHitDo, other);
                }
            HandleOnHitBehaviour(other);
        }
        else
        {
            Logging.Verbose($"{ability.Name} hit {other.name} without any effect");
            DestroyProjectile();
        }
    }

    void HandleOnHitBehaviour(Collider2D other)
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

    void DestroyProjectile(Collider2D other = null)
    {
        if (other != null)
            {
                foreach(OnEventDo onDestroyDo in ability.ListOnDestroyDo)
                    HandleOnEventDo(onDestroyDo, other);
                
            }
        Destroy(gameObject);
    }
    void SplitProjectile(Collider2D other)
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
    void HandleOnEventDo(OnEventDo onEvent, Collider2D other) //TODO: This belongs in its own class!! Other Ability types will definitly use this!
    {
        Logging.Verbose($"HandleOnEventDo {onEvent} with {other.name}");
        switch (onEvent)
        {
            case OnEventDo.Nothing:
                Logging.Verbose("Do nothing");    
                break;
            case OnEventDo.Ability:
                Logging.Verbose("Do Ability");    
                break;
            case OnEventDo.Damage:
                Logging.Verbose($"Do Damage to {other.gameObject.name}");
                if (ability.Attribute != AttributeType.None && ability.Value != 0)
                {
                    
                }
                break;
            case OnEventDo.Heal:
                Logging.Verbose($"Heal {other.gameObject.name}");    
                break;
            case OnEventDo.StatusEffect:
                Logging.Verbose($"Apply {ability.StatusEffect.EffectName} to {other.gameObject.name}");    
                if (ability.StatusEffect != null)
                    ability.StatusEffect.ApplyStatusEffect(other.gameObject);
                break;
        }
    }
}
