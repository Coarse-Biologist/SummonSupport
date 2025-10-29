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

        return Activate(user, spawnPoint.transform);
    }
    public bool Activate(GameObject user, Transform direction)
    {
        int shots = 1;
        if (user.TryGetComponent(out AbilityModHandler modHandler))
        {
            shots += modHandler.GetModAttributeByType(this, AbilityModTypes.ProjectileNumber);
        }
        for (int i = 0; i < shots; i += 1)
        {
            GameObject projectile = Instantiate(Projectile, user.transform.position, Quaternion.identity);
            Projectile projectileScript = projectile.GetComponent<Projectile>();
            float rotZ = (float)Math.Sin(45 * i) * (10 + 5 * i);
            Quaternion rotation = Quaternion.Euler(0, 0, rotZ);
            Vector3 newDirection = rotation * direction.right;
            projectileScript.ability = this;
            projectileScript.SetProjectilePhysics(projectile, newDirection);
            projectileScript.SetParticleTrailEffects(newDirection);
            projectileScript.SetActive(user);

            Destroy(projectile, Lifetime);
        }

        return true;
    }


}



//int shots = 1;
//        if (user.TryGetComponent(out AbilityModHandler modHandler))
//        {
//            shots += modHandler.GetModAttributeByType(this, AbilityModTypes.ProjectileNumber);
//        }
//        for (int i = 0; i < shots; i += 1)
//        {
//            Debug.Log("for loop is being carried out in the Shoot multiple func of the projectile script");
//            Quaternion rotation;
//            float rotZ = (float)Math.Sin(45 * i) * (10 + 5 * i);
//            rotation = Quaternion.Euler(0, 0, rotZ);
//            Vector3 newDirection = rotation * direction.right;
//
//            GameObject projectile = Instantiate(Projectile, user.transform.position, Quaternion.identity);
//            Projectile projectileScript = projectile.GetComponent<Projectile>();
//            projectileScript.ability = this;
//            projectileScript.SetParticleTrailEffects(newDirection);
//            projectileScript.SetProjectilePhysics(projectile, newDirection);
//
//            Destroy(projectile, Lifetime);