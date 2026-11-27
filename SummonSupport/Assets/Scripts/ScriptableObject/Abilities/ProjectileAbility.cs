using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.UIElements;

[CreateAssetMenu(menuName = "Abilities/Projectile Ability")]
public class ProjectileAbility : Ability
{
    [field: Header("Projectile settings")]
    [field: SerializeField] public float Speed { get; protected set; }
    [field: SerializeField] public NetworkObject Projectile { get; protected set; }
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
        int shots = 1 + Mathf.Max(0, AbilityModHandler.GetPlayerModValue(this, AbilityModTypes.Number, casterStats));
        int sizeModValue = Mathf.Max(0, AbilityModHandler.GetPlayerModValue(this, AbilityModTypes.Size, casterStats));
        int durationMod = AbilityModHandler.GetPlayerModValue(this, AbilityModTypes.Duration, casterStats);
        Vector3 sizeMod = new(sizeModValue / 2f, 0f, 0f);
        float lifetime = Mathf.Max(.1f, Lifetime + durationMod);
        AbilityModHandler modHandler = casterStats != null && casterStats.CharacterTag != CharacterTag.Enemy ? AbilityModHandler.Instance : null;

        for (int i = 0; i < shots; i += 1)
        {
            NetworkObject projectile = Instantiate(Projectile, spawnPoint.position, Quaternion.identity);
            projectile.transform.localScale += sizeMod;
            Projectile projectileScript = projectile.GetComponent<Projectile>();

            if (ElementTypes.Count != 0)
            {
                Material glowMaterial = ColorChanger.GetGlowStrengthByElement(ElementTypes[0]);
                ColorChanger.ChangeMatByAffinity(projectile.GetComponent<Renderer>(), glowMaterial);
            }

            float rotY = (float)Math.Sin(45 * i) * (10 + 5 * i);
            Quaternion rotation = Quaternion.Euler(0, rotY, 0);
            Vector3 newDirection = rotation * spawnPoint.forward;
            //Debug.Log($"  user: {user.GetComponent<LivingBeing>().Name} is using the ability {projectileScript.name}");
            projectileScript.SetActive(this, casterStats, modHandler);
            projectileScript.SetProjectilePhysics(newDirection);
            projectileScript.SetParticleTrailEffects(newDirection);

            Destroy(projectile, lifetime);
        }

        return true;
    }


}


