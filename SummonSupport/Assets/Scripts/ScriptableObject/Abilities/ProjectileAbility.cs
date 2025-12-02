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



    public override bool Activate(GameObject user)
    {
        return Activate(user, user);
    }
    public bool Activate(GameObject user, GameObject spawnPoint)
    {

        return Activate(user, spawnPoint.transform);
    }
    public bool Activate(GameObject user, Transform spawnPoint)
    {
        //Debug.Log($"spawnpoint rotation = {spawnPoint.rotation.y}");
        int shots = 1;
        if (user.TryGetComponent(out AbilityModHandler modHandler))
        {
            shots += modHandler.GetModAttributeByType(this, AbilityModTypes.Number);
        }
        for (int i = 0; i < shots; i += 1)
        {
            GameObject projectile = Instantiate(Projectile, spawnPoint.position, Quaternion.identity);
            Projectile projectileScript = projectile.GetComponent<Projectile>();

            Material glowMaterial = ColorChanger.GetGlowByElement(ElementTypes[0]);
            ColorChanger.ChangeMatByAffinity(projectile.GetComponent<Renderer>(), glowMaterial);

            float rotY = (float)Math.Sin(45 * i) * (10 + 5 * i);
            Quaternion rotation = Quaternion.Euler(0, rotY, 0);
            Vector3 newDirection = rotation * spawnPoint.forward;
            Debug.Log($"  user: {user.GetComponent<LivingBeing>().Name} is using the ability {projectileScript.name}");
            projectileScript.SetActive(this, user.GetComponent<LivingBeing>(), modHandler);
            projectileScript.SetProjectilePhysics(newDirection);
            projectileScript.SetParticleTrailEffects(newDirection);

            Destroy(projectile, Lifetime);
        }

        return true;
    }


}


