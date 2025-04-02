using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/Projectile Ability")]
public class ProjectileAbility : Ability
{
    [field: SerializeField, Header("Projectile settings")]
                            public float                Speed               { get; protected set; }
    [field: SerializeField] public GameObject           Projectile          { get; protected set; }
    [field: SerializeField] public float                Direction           { get; protected set; }
    [field: SerializeField] public float                MaxRange            { get; protected set; }
    [field: SerializeField] public float                Lifetime            { get; protected set; }
    [field: SerializeField] public PiercingBehaviour    PiercingMode        { get; protected set; } //Pass through, break on first hit, break on x hit, 
    [field: SerializeField] public OnEventDo            OnHit               { get; protected set; } // nothing, effect, cast, 
    [field: SerializeField] public OnEventDo            OnDestroy           { get; protected set; } // nothing, effect, cast, 
    [field: SerializeField] public StatusEffect         StatusEffect        { get; protected set; } 


    public override void Activate(GameObject user, GameObject spawnPoint)
    {
        Activate(user, spawnPoint, null);
    }
    public void Activate(GameObject user, GameObject spawnPoint, Transform direction = null)
    {
    Logging.Info($"{user.name} casted a {Name}");
    GameObject projectile = Instantiate(Projectile, spawnPoint.transform.position, Quaternion.identity);
    Rigidbody2D rb = projectile.GetComponent<Rigidbody2D>();
    Projectile projectileScript = projectile.GetComponent<Projectile>();
    projectileScript.statusEffect = StatusEffect;
    projectileScript.avoid = user;
    Vector2 moveDirection = direction != null ? direction.right : spawnPoint.transform.right;
    rb.linearVelocity = moveDirection * Speed;
    float angle = Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg;
    projectile.transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle));
    Destroy(projectile, Lifetime);
    }
}