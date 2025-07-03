using Unity.VisualScripting;
using UnityEngine;

public class Crew_AbilityMono : MonoBehaviour
{
    public Crew_EffectPackage effectPackage;

    private Vector2 spawnLoc;

    private LivingBeing CasterStats;

    public void Awake()
    {
        spawnLoc = (Vector2)transform.position;
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        LivingBeing targetStats = collision.gameObject.GetComponent<LivingBeing>();
        Logging.Info($"Crews projectile mono words like a charm? {effectPackage.EffectDescription} = description");
        if (targetStats != null)
        {
            AbilityUseHandler_1.HandleEffects(CasterStats, targetStats, effectPackage);
        }
    }

    #region setters
    public void SetAbilityData(Crew_EffectPackage Data)
    {
        effectPackage = Data;
    }

    public void SetCasterStats(LivingBeing Caster)
    {
        CasterStats = Caster;
    }
    public void SetSprite(Sprite sprite)
    {
        gameObject.GetComponent<SpriteRenderer>().sprite = sprite;

    }

    public void SetRadius(float radius)
    {
        GetComponent<CircleCollider2D>().radius = radius;
    }

    public void SetProjectilePhysics(Vector2 direction)
    {
        if (direction == Vector2.zero)
            direction = transform.right;
        direction = direction - spawnLoc;
        direction = direction.normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        rb.linearVelocity = direction * effectPackage.Projectile.Speed;
        transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle));
    }
    #endregion



}
