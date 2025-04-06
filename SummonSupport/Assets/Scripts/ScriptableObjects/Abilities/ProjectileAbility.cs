using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/Projectile Ability")]
public class ProjectileAbility : Ability
{
    [field: SerializeField, Header("Projectile settings")]
                            public float                Speed               { get; protected set; }
    [field: SerializeField] public GameObject           Projectile          { get; protected set; }
    [field: SerializeField] public float                MaxRange            { get; protected set; }
    [field: SerializeField] public float                Lifetime            { get; protected set; }
    [field: SerializeField] public PiercingBehaviour    PiercingMode        { get; protected set; } //Pass through, break on first hit, break on x hit,
    [field: SerializeField] public int                  MaxPierceTimes      { get; protected set; }
    [field: SerializeField] public int                  MaxSplitInto        { get; protected set; }
    [field: SerializeField] public OnEventDo            OnHit               { get; protected set; } // nothing, effect, cast,
    [field: SerializeField] public OnEventDo            OnDestroy           { get; protected set; } // nothing, effect, cast,


    public override void Activate(GameObject user, GameObject spawnPoint)
    {
        Activate(user, spawnPoint, null);
    }
    public void Activate(GameObject user, GameObject spawnPoint, Transform direction = null)
    {
        //TODO: clean up this mess ..
        Logging.Info($"{user.name} casted a {Name}");
        GameObject projectile = Instantiate(Projectile, spawnPoint.transform.position, Quaternion.identity);
        Projectile projectileScript = projectile.GetComponent<Projectile>();
        projectileScript.ability = this;
        projectileScript.Shoot(user, spawnPoint, direction);
    }
}