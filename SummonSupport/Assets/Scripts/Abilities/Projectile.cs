using System.Collections;
using UnityEditor.Rendering;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public GameObject user;
    public ProjectileAbility ability;

    int piercedAlready = 0;
    int splitAlready = 0;

    public void Shoot(GameObject user, GameObject spawnAt = null, Transform lookAt = null)
    {
        this.user = user;
        SetProjectilePhysics(spawnAt, lookAt);
        Destroy(this, ability.Lifetime); // TODO: change from lifetime to range
    }

    public void SetProjectilePhysics(GameObject spawnPoint, Transform direction)
    {
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        Vector2 moveInDirection = direction != null ? direction.right : spawnPoint.transform.right;
        rb.linearVelocity = moveInDirection * ability.Speed;
        float angle = Mathf.Atan2(moveInDirection.y, moveInDirection.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle));
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject == user)
            return;
        else if (other.gameObject.GetComponent<LivingBeing>() != null)
        {
            HandleOnEventDo(ability.OnHit, other);
            HandlePiercingLogic(other);
        }
        else
        {
            DestroyProjectile();
        }
        Logging.Verbose($"projectile hit {other.name}");
    }

    void HandlePiercingLogic(Collider2D other)
    {
        switch (ability.PiercingMode)
            {
                case PiercingBehaviour.Ricochet:
                    break;
                case PiercingBehaviour.Pierce:
                    piercedAlready++;
                    if (piercedAlready == ability.MaxPierceTimes)
                        DestroyProjectile(other);
                    break;
                case PiercingBehaviour.Split:
                    splitAlready++;
                    if (splitAlready == ability.MaxSplitInto)
                        DestroyProjectile(other);
                    else
                    {
                        // TODO: split projectile logic
                    }
                    break;
                case PiercingBehaviour.Off:
                    DestroyProjectile(other);
                    break;
            }
    }

    void DestroyProjectile(Collider2D other = null)
    {
        if (other != null)
            HandleOnEventDo(ability.OnDestroy, other);
        Destroy(gameObject);
    }

    void HandleOnEventDo(OnEventDo onEvent, Collider2D other)
    {
        Logging.Verbose($"HandleOnEventDo {onEvent} with {other.name}");
        switch (onEvent)
        {
            case OnEventDo.Nothing:
                break;
            case OnEventDo.Ability:
                break;
            case OnEventDo.StatusEffect:
                Logging.Verbose($"{ability.StatusEffect.EffectName}");
                if (ability.StatusEffect != null)
                    ability.StatusEffect.ApplyStatusEffect(other.gameObject);
                break;
        }
    }
}
