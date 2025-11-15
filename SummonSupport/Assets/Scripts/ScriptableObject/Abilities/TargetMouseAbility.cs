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
        CharacterTag desiredTag;

        int targetNum = 1;
        List<LivingBeing> targets = new();
        if (caster.TryGetComponent(out AbilityModHandler modHandler))
        {
            targetNum += modHandler.GetModAttributeByType(this, AbilityModTypes.ProjectileNumber);
        }

        if (AbilityTypeTag == AbilityTypeTag.BuffsTarget) // logic assumes that only players will be using this ability type.
        {
            desiredTag = CharacterTag.Minion;
        }
        else desiredTag = CharacterTag.Enemy;
        Transform spawnPoint = caster.GetComponent<AbilityHandler>().abilitySpawn.transform;


        //Debug.Log($"number of Targets found: {targets.Count}. max targets = {targetNum}");
        return GetTargetfromSphereCast(this, spawnPoint, targetNum, desiredTag);
    }

    public override bool Activate(GameObject user)
    {
        //Debug.Log($"Activating the targetMouse ability {this.Name}");
        LivingBeing casterStats = user.GetComponent<LivingBeing>();

        bool usedAbility = false;
        foreach (LivingBeing target in GetTargets(casterStats))
        {
            if (target != null)
            {
                usedAbility = ActivateAbility(user, target);

                //Debug.Log($"success in activating ability = {usedAbility}");
                if (usedAbility)
                {
                    SpawnEffect(target, user);
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


    public bool ActivateAbility(GameObject user, LivingBeing targetLivingBeing)
    {
        if (user.TryGetComponent<AI_CC_State>(out AI_CC_State ccState) && ccState.isCharmed)
        {
            //Debug.Log($"returning false");
            return true;
        }

        if (user.TryGetComponent<LivingBeing>(out var userLivingBeing))
        {
            if (!IsUsableOn(userLivingBeing.CharacterTag, targetLivingBeing.CharacterTag))
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





    private void SpawnEffect(LivingBeing targetLivingBeing, GameObject Caster)
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

//if (AlchemyHandler.Instance.activeMinions.Count != 0)
//                for (int i = 0; i < targetNum;)
//                {
//                    GameObject minionObject = minions[UnityEngine.Random.Range(0, minionNum)];
//                    if (!targets.Contains(minionObject.GetComponent<LivingBeing>()))
//                    {
//                        targets.Add(minionObject.GetComponent<LivingBeing>());
//                        i++;
//                    }
//                }


