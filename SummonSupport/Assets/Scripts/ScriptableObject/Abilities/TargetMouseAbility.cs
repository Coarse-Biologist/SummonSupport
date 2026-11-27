using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/Target Mouse Ability")]
public class TargetMouseAbility : Ability
{
    [field: Header("settings")]
    [field: SerializeField] public GameObject SpawnEffectOnHit { get; set; }
    [field: SerializeField] public EffectOrientation EffectOrientation { get; set; } = EffectOrientation.Identity;


    private List<LivingBeing> GetTargets(LivingBeing caster)
    {
        int targetNum = 1;
        int sizeMod = 0;
        int speedMod = 0;
        if (caster != null && caster.CharacterTag != CharacterTag.Enemy)
        {
            targetNum += Mathf.Max(0, AbilityModHandler.GetPlayerModValue(this, AbilityModTypes.Number, caster));
            sizeMod = Mathf.Max(0, AbilityModHandler.GetPlayerModValue(this, AbilityModTypes.Size, caster));
            speedMod = Mathf.Max(0, AbilityModHandler.GetPlayerModValue(this, AbilityModTypes.Speed, caster));
        }
        TeamType desiredTargetType = GetTargetPreference(caster);
        Transform spawnPoint = caster.GetComponent<AbilityHandler>().abilitySpawn.transform;
        float castRadius = 4f + sizeMod * .5f;
        float castRange = Range + speedMod * .5f;
        //Debug.Log($"number of Targets found: {targets.Count}. max targets = {targetNum}");
        return GetTargetfromSphereCast(caster, spawnPoint, targetNum, desiredTargetType, castRadius, castRange);
    }

    public override bool Activate(LivingBeing casterStats)
    {
        //Debug.Log($"Activating the targetMouse ability {this.Name}");

        bool usedAbility = false;
        foreach (LivingBeing target in GetTargets(casterStats))
        {
            if (target != null)
            {
                usedAbility = ActivateAbility(casterStats, target);

                //Debug.Log($"success in activating ability = {usedAbility}");
                if (usedAbility)
                {
                    SpawnEffect(target, casterStats);
                    if (casterStats == target)
                    {
                        CombatStatHandler.HandleEffectPackage(this, casterStats, target, SelfEffects);
                    }
                    else
                    {
                        CombatStatHandler.HandleEffectPackage(this, casterStats, target, TargetEffects);
                    }
                }
            }
        }
        return usedAbility;
    }


    public bool ActivateAbility(LivingBeing casterStats, LivingBeing targetLivingBeing)
    {
        if (casterStats != null && targetLivingBeing != null)
        {
            if (!ThoroughIsUsableOn(casterStats, targetLivingBeing))
            {
                //Debug.Log($"returning false");
                return false;
            }
            else
            {
                //Debug.Log($"returning true");
                return true;
            }
        }
        else
        {
            //Debug.Log($"returning false");
            return false;
        }
    }





    private void SpawnEffect(LivingBeing targetLivingBeing, LivingBeing Caster)
    {
        GameObject instance;
        Quaternion rotation = Quaternion.identity;
        Transform effectChild; // if there is a special force field which must be handled separately
        if (EffectOrientation == EffectOrientation.TowardCaster)
        {
            var direction = (targetLivingBeing.transform.position - Caster.transform.position).normalized;
            rotation = Quaternion.LookRotation(direction);
        }
        if (SpawnEffectOnHit != null)
        {
            instance = Instantiate(SpawnEffectOnHit, targetLivingBeing.transform.position, rotation, targetLivingBeing.transform.transform);
            if (instance.transform.GetComponentInChildren<ParticleSystemForceField>() != null)
            {

                effectChild = instance.transform.GetComponentInChildren<ParticleSystemForceField>().transform;
                effectChild.transform.position = Caster.transform.position;
                effectChild.transform.SetParent(Caster.transform);

                Destroy(effectChild.gameObject, 3f);
            }
            Destroy(instance, 3f);

        }
    }
}

