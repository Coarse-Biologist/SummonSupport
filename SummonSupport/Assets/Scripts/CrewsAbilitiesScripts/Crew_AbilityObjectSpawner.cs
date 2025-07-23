using Unity.Transforms;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
public static class Crew_AbilityObjectSpawner
{

    private static string prefabAddress = "Assets/Scripts/CrewsAbilitiesScripts/SquareProjectile.prefab";
    private static AsyncOperationHandle<GameObject> handle;
    [SerializeField] public static GameObject projectile;

    public static void SpawnProjectile(LivingBeing CasterStats, Crew_EffectPackage effectPackage, Vector2 SpawnLoc, Vector2 TargetLoc)
    {
        GameObject projectileInstance = GameObject.Instantiate(projectile, SpawnLoc, Quaternion.identity);
        GameObject particleSystem = GameObject.Instantiate(effectPackage.ParticleEffect, SpawnLoc, Quaternion.identity, projectileInstance.transform);
        Crew_AbilityMono abilityMonoScript = projectileInstance.GetComponent<Crew_AbilityMono>();
        abilityMonoScript.SetAbilityData(effectPackage);
        abilityMonoScript.SetProjectilePhysics(TargetLoc);
        //abilityMonoScript.SetSprite(effectPackage.EffectSprite);
        abilityMonoScript.SetCasterStats(CasterStats);
        if (effectPackage.HasAoE)
            abilityMonoScript.SetRadius(effectPackage.Radius);
    }
    public static void SpawnAura(Crew_EffectPackage effectPackage, LivingBeing casterStats)
    {
        Vector2 spawnLoc = casterStats.transform.position;
        GameObject auraInstance = GameObject.Instantiate(projectile, spawnLoc, Quaternion.identity, casterStats.transform);
        GameObject.Instantiate(effectPackage.ParticleEffect, spawnLoc, Quaternion.identity, auraInstance.transform);

        Crew_AbilityMono abilityMonoScript = auraInstance.GetComponent<Crew_AbilityMono>();

        abilityMonoScript.SetAbilityData(effectPackage);
        //abilityMonoScript.SetSprite(effectPackage.EffectSprite);
        abilityMonoScript.SetRadius(effectPackage.Radius);

    }


    public static void LoadPrefab()
    {
        handle = Addressables.LoadAssetAsync<GameObject>(prefabAddress);

        handle.Completed += op =>
        {
            if (op.Status == AsyncOperationStatus.Succeeded)
            {
                projectile = op.Result;
            }
            else
            {
                Debug.LogError("Failed to load addressable prefab: " + prefabAddress);
            }
        };
    }
}
