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
    private AbilityModHandler modHandler;

    private WaitForSeconds movementWait = new WaitForSeconds(.1f);






    public override bool Activate(LivingBeing casterStats)
    {
        abilityHandler = casterStats.abilityHandler;

        originTransform = abilityHandler.abilitySpawn.transform;
        SetModHandler(casterStats);
        if (originTransform == null)
        {
            originTransform = casterStats.transform;
        }

        return AttemptActivation(casterStats);

    }
    
    private void SetModHandler(LivingBeing casterStats)
    {
        if (casterStats.CharacterTag != CharacterTag.Enemy)
        {
            modHandler = AbilityModHandler.Instance;
        }
        else modHandler = null;
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

        if (!ThoroughIsUsableOn(casterStats, Target))
        {
            if (!HasElementalSynergy(this, Target))
                return false;
        }
        return VerifyWithinShape(casterStats, targetStats);
    }

    private bool VerifyWithinShape(LivingBeing casterStats, LivingBeing targetStats)
    {
        float sizeMod = Width;
        if (modHandler != null)
        {
            sizeMod += modHandler.GetModAttributeByType(this, AbilityModTypes.Size);
        }
        Vector3 hitLocation = targetStats.transform.position;
        if ((hitLocation - originTransform.position).magnitude <= .2)
            return true;
        if (Shape == AreaOfEffectShape.Sphere)
        {
            if ((casterStats.transform.position - targetStats.transform.position).magnitude <= Range + sizeMod)
                return true;
        }
        if (Shape == AreaOfEffectShape.Cone)
        {
            return Vector3.Angle(hitLocation, originTransform.position) <= Angle + sizeMod;
        }
        if (Shape == AreaOfEffectShape.Rectangle)
        {
            Vector3 halfExtents = new Vector3(Width + sizeMod, Width, Range + sizeMod);
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
            Destroy(particleSystem, particleSystem.GetComponentInChildren<ParticleSystem>().main.duration);

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

