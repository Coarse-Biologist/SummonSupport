using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

[CreateAssetMenu(menuName = "Abilities/Projectile Ability")]
public class ProjectileAbility : Ability
{
    [field: Header("Projectile settings")]
    [field: SerializeField] public float Speed { get; protected set; }
    [field: SerializeField] public GameObject Projectile { get; protected set; }
    [field: SerializeField] public float MaxRange { get; protected set; } = 15;
    [field: SerializeField] public float Lifetime { get; protected set; } = 3;
    [field: SerializeField] public OnHitBehaviour PiercingMode { get; protected set; }
    [field: SerializeField] public int MaxPierce { get; protected set; }
    [field: SerializeField] public int MaxSplit { get; protected set; }
    [field: SerializeField] public int SplitAngleOffset { get; protected set; }
    [field: SerializeField] public GameObject ProjectileParticleSystem { get; protected set; }
    [field: SerializeField] public GameObject SpawnEffectOnHit { get; set; } = null;



    public override bool Activate(GameObject user)
    {
        return Activate(user, user);
    }
    public bool Activate(GameObject user, GameObject spawnPoint)
    {

        return Activate(user, spawnPoint, spawnPoint.transform);
    }
    public bool Activate(GameObject user, GameObject spawnPoint, Transform direction)
    {
        GameObject projectile = Instantiate(Projectile, user.transform.position, Quaternion.identity);
        Projectile projectileScript = projectile.GetComponent<Projectile>();

        projectileScript.ability = this;
        projectileScript.Shoot(user, spawnPoint, direction.right);
        projectileScript.SetParticleTrailEffects(direction.right);
        Destroy(projectile, Lifetime);

        return true;
    }


}