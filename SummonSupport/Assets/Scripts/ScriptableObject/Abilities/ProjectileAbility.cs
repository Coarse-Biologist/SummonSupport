using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

[CreateAssetMenu(menuName = "Abilities/Projectile Ability")]
public class ProjectileAbility : Ability
{
    [field: Header("Projectile settings")]
    [field: SerializeField] public float Speed { get; protected set; }
    [field: SerializeField] public GameObject Projectile { get; protected set; }
    [field: SerializeField] public float MaxRange { get; protected set; }
    [field: SerializeField] public float Lifetime { get; protected set; }
    [field: SerializeField] public OnHitBehaviour PiercingMode { get; protected set; }
    [field: SerializeField] public int MaxPierce { get; protected set; }
    [field: SerializeField] public int MaxSplit { get; protected set; }
    [field: SerializeField] public int SplitAngleOffset { get; protected set; }
    [field: SerializeField] public List<OnEventDo> ListOnHitDo { get; protected set; }
    [field: SerializeField] public List<OnEventDo> ListOnDestroyDo { get; protected set; }
    [field: SerializeField] public GameObject ProjectileParticleSystem { get; protected set; }
    public GameObject SpawnEffectOnHit { get; set; }



    public override bool Activate(GameObject user)
    {
        Logging.Info("activated projectile func 1");
        return Activate(user, user);
    }
    public bool Activate(GameObject user, GameObject spawnPoint)
    {
        Logging.Info("activated projectile func 2");

        return Activate(user, spawnPoint, spawnPoint.transform);
    }
    public bool Activate(GameObject user, GameObject spawnPoint, Transform direction)
    {
        Logging.Info("activated projectile func 3");

        GameObject projectile = Instantiate(Projectile, user.transform.position, Quaternion.identity);
        Projectile projectileScript = projectile.GetComponent<Projectile>();
        SetEffects(projectileScript, user);

        projectileScript.ability = this;
        Logging.Info($"direction.right = {direction.right}");
        projectileScript.Shoot(user, spawnPoint, direction.right);
        return true;
    }


    private void SetEffects(Projectile projectile, GameObject user)
    {
        GameObject particleSystem;

        if (ProjectileParticleSystem != null)
        {
            particleSystem = Instantiate(ProjectileParticleSystem, user.transform.position, Quaternion.identity, projectile.transform);
            //Quaternion rotation = Quaternion.LookRotation(-direction);
            Quaternion rotation = Quaternion.LookRotation(-user.transform.right);
            particleSystem.transform.rotation = rotation;
        }

    }
}