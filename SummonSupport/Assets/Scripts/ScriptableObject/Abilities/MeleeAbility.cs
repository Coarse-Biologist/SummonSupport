using UnityEngine;
using System;
using UnityEngine.Splines;
using System.Linq;

[CreateAssetMenu(menuName = "Abilities/Melee Ability")]

public class MeleeAbility : Ability
{
    [Header("Melee Ability settings")]


    [field: SerializeField] public float Angle { get; private set; }
    [field: SerializeField] public float Width { get; private set; }
    [field: SerializeField] public EffectSpawnLoc SpawnLocEnum { get; private set; }

    [field: SerializeField] public AreaOfEffectShape Shape { get; private set; }
    [field: SerializeField] public GameObject MeleeParticleSystem { get; protected set; }

    private Transform originTransform;
    private LivingBeing Caster;
    private LivingBeing Target;
    private AbilityHandler abilityHandler;

    private WaitForSeconds movementWait = new WaitForSeconds(.1f);






    public override bool Activate(LivingBeing casterStats)
    {
        abilityHandler = casterStats.abilityHandler;

        originTransform = abilityHandler != null && abilityHandler.abilitySpawn != null
            ? abilityHandler.abilitySpawn.transform
            : casterStats.transform;

        int swings = 1 + Mathf.Max(0, AbilityModHandler.GetPlayerModValue(this, AbilityModTypes.Number, casterStats));
        bool activated = false;
        for (int swing = 0; swing < swings; swing++)
        {
            if (AttemptActivation(casterStats)) activated = true;
        }

        return activated;

    }




    private bool AttemptActivation(LivingBeing casterStats)
    {
        Collider[] hits = Physics.OverlapSphere(casterStats.transform.position, Range);
        bool activated = false;
        foreach (Collider collider in hits)
        {
            if (collider != null && collider.gameObject != casterStats.gameObject)
            {
                if (!collider.TryGetComponent(out LivingBeing targetStats))
                {
                    continue;
                }
                if (VerifyActivate(targetStats, casterStats))
                {
                    SetEffects(casterStats, targetStats);
                    CombatStatHandler.HandleEffectPackage(this, casterStats, targetStats, this.TargetEffects);
                    SpawnHitEffect(targetStats);
                    activated = true;
                }
            }

        }
        return activated;
    }


    private bool VerifyActivate(LivingBeing targetStats, LivingBeing casterStats)
    {

        Target = targetStats;
        // if (AbilityTypeTag == AbilityTypeTag.BuffsTarget) return VerifyWithinShape(casterStats, targetStats);

        if (!ThoroughIsUsableOn(casterStats, Target))
        {
            if (!HasElementalSynergy(this, Target))
                return false;
        }
        return VerifyWithinShape(casterStats, targetStats);
    }

    private bool VerifyWithinShape(LivingBeing casterStats, LivingBeing targetStats)
    {
        float sizeMod = Width + Mathf.Max(0, AbilityModHandler.GetPlayerModValue(this, AbilityModTypes.Size, casterStats));
        float speedMod = Mathf.Max(0, AbilityModHandler.GetPlayerModValue(this, AbilityModTypes.Speed, casterStats));
        float rangeMod = Range + speedMod * .5f;
        Vector3 hitLocation = targetStats.transform.position;
        if ((hitLocation - originTransform.position).magnitude <= .2)
            return true;
        if (Shape == AreaOfEffectShape.Sphere)
        {
            if ((casterStats.transform.position - targetStats.transform.position).magnitude <= rangeMod + sizeMod)
                return true;
        }
        if (Shape == AreaOfEffectShape.Cone)
        {
            return Vector3.Angle(hitLocation, originTransform.position) <= Angle + sizeMod;
        }
        if (Shape == AreaOfEffectShape.Rectangle)
        {
            Vector3 halfExtents = new Vector3(Width + sizeMod, Width, rangeMod + sizeMod);
            Vector3 offset = originTransform.forward * halfExtents.z;

            return Physics.OverlapBox(originTransform.position + offset, halfExtents, originTransform.rotation).Contains(targetStats.gameObject.GetComponent<Collider>());
        }
        else return false;
    }




    private void SpawnHitEffect(LivingBeing targetLivingBeing)
    {
        GameObject instance;

        if (OnHitEffect != null)
        {
            instance = Instantiate(OnHitEffect, targetLivingBeing.transform.position, Quaternion.identity, targetLivingBeing.transform.transform);
            //EffectColorChanger.SetImmersiveBleedEffect(instance.GetComponent<ParticleSystem>(), targetLivingBeing);
            Destroy(instance, 3f);
        }

    }
    private void SetEffects(LivingBeing caster, LivingBeing target)
    {
        GameObject particleSystem;
        abilityHandler = caster.GetComponent<AbilityHandler>();
        //originTransform = caster.transform;
        Vector3 spawnLoc = GetSpawnLoc(target);
        if (MeleeParticleSystem != null)
        {

            //Debug.Log("going to instantiate el chico maldito");
            particleSystem = Instantiate(MeleeParticleSystem, spawnLoc, Quaternion.identity, caster.transform);
            Quaternion rotation = Quaternion.LookRotation(originTransform.transform.forward);
            particleSystem.transform.rotation = rotation;

            int speedMod = Mathf.Max(0, AbilityModHandler.GetPlayerModValue(this, AbilityModTypes.Speed, caster));
            float simulationSpeed = 1f + speedMod * .1f;
            ParticleSystem system = particleSystem.GetComponentInChildren<ParticleSystem>();
            if (system != null)
            {
                var main = system.main;
                main.simulationSpeed *= simulationSpeed;
                Destroy(particleSystem, main.duration / simulationSpeed);
            }
            else Destroy(particleSystem, 3f);

        }
    }
    private Vector3 GetSpawnLoc(LivingBeing target)
    {
        Vector3 spawnLoc = originTransform.position;
        if (SpawnLocEnum == EffectSpawnLoc.TargetTransform) spawnLoc = target.transform.position;
        if (SpawnLocEnum == EffectSpawnLoc.CasterTransform) spawnLoc = originTransform.position;
        return spawnLoc;
    }



}

