using UnityEngine;

public class Crew_ProjectileMono : MonoBehaviour
{
    public Crew_EffectPackage projectileData;

    private Vector2 spawnLoc;

    public void Awake()
    {
        spawnLoc = (Vector2)transform.position;
    }



    private void OnTriggerEnter2D(Collider2D collision)
    {
        Logging.Info($"Crews projectile mono words like a charm? {projectileData.EffectDescription} = description");
    }

    public void SetAbilityData(Crew_EffectPackage Data)
    {
        projectileData = Data;
    }

    public void SetProjectilePhysics(Vector2 direction)
    {
        if (direction == Vector2.zero)
            direction = transform.right;
        direction = direction - spawnLoc;
        direction = direction.normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        rb.linearVelocity = direction * projectileData.Projectile[0].Speed;
        transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle));
    }
}
