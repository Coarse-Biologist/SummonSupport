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




    public override bool Activate(LivingBeing casterStats)
    {
        Transform spawnPoint = casterStats.transform;
        //Debug.Log($"spawnpoint rotation = {spawnPoint.rotation.y}");
        int shots = 1;
        Vector3 sizeMod = new();
        if (casterStats.CharacterTag != CharacterTag.Enemy)
        {
            shots += AbilityModHandler.Instance.GetModAttributeByType(this, AbilityModTypes.Number);
            sizeMod.x += AbilityModHandler.Instance.GetModAttributeByType(this, AbilityModTypes.Size) / 2;
        }
        for (int i = 0; i < shots; i += 1)
        {
            GameObject projectile = Instantiate(Projectile, spawnPoint.position, Quaternion.identity);
            projectile.transform.localScale += sizeMod;
            Projectile projectileScript = projectile.GetComponent<Projectile>();

            Material glowMaterial = ColorChanger.GetGlowStrengthByElement(ElementTypes[0]);
            ColorChanger.ChangeMatByAffinity(projectile.GetComponent<Renderer>(), glowMaterial);

            float rotY = (float)Math.Sin(45 * i) * (10 + 5 * i);
            Quaternion rotation = Quaternion.Euler(0, rotY, 0);
            Vector3 newDirection = rotation * spawnPoint.forward;
            //Debug.Log($"  user: {user.GetComponent<LivingBeing>().Name} is using the ability {projectileScript.name}");
            projectileScript.SetActive(this, casterStats, AbilityModHandler.Instance);
            projectileScript.SetProjectilePhysics(newDirection);
            projectileScript.SetParticleTrailEffects(newDirection);

            Destroy(projectile, Lifetime);
        }

        return true;
    }


}


