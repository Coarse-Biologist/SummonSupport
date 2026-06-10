using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.Splines;


public class Aura : MonoBehaviour
{
    LivingBeing caster;
    Ability ability;

    private AbilityModHandler modHandler;
    private Mod_Base abilityMod;

    List<LivingBeing> ignoreTargets = new();
    [SerializeField] public float ActivationTime { get; private set; } = .5f;
    [field: SerializeField] public float SizeScalar = .1f;
    private bool Active = false;
    private Collider capsuleCollider;

    private LivingBeing target;
    private ConjureAbility ConjureAbility = null;
    private ParticleSystem ps;
    private float radius = 0;
    private float speed = 1;
    private float duration;
    private int maxPierce = 3;
    private int piercedAlready = 0;
    private int maxSplit = 0;
    private int splitAlready = 0;



    void Start()
    {
        if (gameObject.TryGetComponent(out ParticleSystem systemParticular))
            ps = systemParticular;
        //Invoke("Activate", ActivationTime);
    }
    public void HandleInstantiation(LivingBeing caster, LivingBeing target, Ability ability)
    {

        SetAuraStats(caster, target, ability);

        CheckTargetsAtSpawnTime(caster);

        HandleConjureAbilitySpecifics();

        if (ability is AuraAbility auraAbility)
            radius = auraAbility.Radius;

        AddMods();

        AlterApparentRadius();

        CombatStatHandler.HandleEffectPackage(ability, caster, caster, ability.SelfEffects);


        Destroy(gameObject, duration);

    }
    private void AlterApparentRadius()
    {
        //Debug.Log($"radius = {radius}");
        gameObject.transform.localScale *= 1 + radius * SizeScalar;
        if (gameObject.TryGetComponent(out CapsuleCollider collider))
        {
            collider.radius *= 1 + radius * .5f;
        }
    }
    private void HandleConjureAbilitySpecifics()
    {
        if (ability is ConjureAbility conjureAbility)
        {
            ConjureAbility = conjureAbility;
            radius = conjureAbility.Radius;
            speed = conjureAbility.Speed;
            if (conjureAbility.SeeksTarget)
            {
                StartCoroutine(SeekTarget(target));
            }
        }
    }
    public void SetAuraStats(LivingBeing caster, LivingBeing target, Ability ability)
    {
        duration = ability.Duration;
        this.caster = caster;
        this.ability = ability;
        this.target = target;

        Activate();

    }
    public void CheckTargetsAtSpawnTime(LivingBeing caster = null)
    {
        Collider[] hits = Physics.OverlapCapsule(caster.transform.position + Vector3.up, caster.transform.position - Vector3.up, radius);
        foreach (Collider hitObject in hits)
        {
            if (hitObject.TryGetComponent(out LivingBeing livingBeing))
            {
                //Debug.Log("IHave found a living being in this starting spherecast");
                HandleTriggerEntered(livingBeing);
            }
        }
    }

    private void AddMods()
    {

        if (caster.CharacterTag != CharacterTag.Enemy)
        {
            duration += AbilityModHandler.Instance.GetModAttributeByType(ability, AbilityModTypes.Duration);
            radius += AbilityModHandler.Instance.GetModAttributeByType(ability, AbilityModTypes.Size);
            speed += AbilityModHandler.Instance.GetModAttributeByType(ability, AbilityModTypes.Speed);
            maxPierce += AbilityModHandler.Instance.GetModAttributeByType(ability, AbilityModTypes.MaxPierce);
            maxSplit += AbilityModHandler.Instance.GetModAttributeByType(ability, AbilityModTypes.MaxSplit);
            //Debug.Log($"max split = {maxSplit}");

        }
    }

    private void Activate()
    {
        Active = true;
        //if (capsuleCollider != null)
        //    capsuleCollider.enabled = true;

    }

    void OnTriggerEnter(Collider other)
    {
        if (Active)
        {
            if (other.gameObject.TryGetComponent(out LivingBeing otherLivingBeing))
            {
                if (!ignoreTargets.Contains(otherLivingBeing))
                    HandleTriggerEntered(otherLivingBeing);
            }
            //else Debug.Log($"OnTrigger Enter func called but the trigger object {ability} is not a living being");
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.gameObject.TryGetComponent(out LivingBeing otherLivingBeing))
            otherLivingBeing.AlterAbilityList(ability, false);
    }
    private void SpawnOnHitEffect(LivingBeing targetLivingBeing, GameObject SpawnEffectOnHit)
    {
        GameObject instance;
        if (SpawnEffectOnHit != null)
        {
            instance = Instantiate(SpawnEffectOnHit, targetLivingBeing.transform.position, Quaternion.identity, targetLivingBeing.transform.transform);
            Destroy(instance, instance.GetComponent<ParticleSystem>().main.duration);
        }
    }



    private void HandleTriggerEntered(LivingBeing otherLivingBeing)
    {

        if (ability.ThoroughIsUsableOn(caster, otherLivingBeing))
        {
            if (ability.OnHitEffect != null)
            {
                SpawnOnHitEffect(otherLivingBeing, ability.OnHitEffect);
            }
            otherLivingBeing.AlterAbilityList(ability, true);
            CombatStatHandler.HandleEffectPackage(ability, caster, otherLivingBeing, ability.TargetEffects);
            piercedAlready += 1;
            if (ConjureAbility != null)
            {
                if (ConjureAbility.SeeksTarget)
                {
                    if (splitAlready < maxSplit)
                    {
                        SplitConjuredSeeker(otherLivingBeing);
                    }
                    if (piercedAlready >= maxPierce)
                        Destroy(gameObject, .2f);

                }

            }
        }
    }
    private void SplitConjuredSeeker(LivingBeing livingBeing)
    {
        GameObject newObject = null;
        if (ability is ConjureAbility conjureAbility)
            newObject = Instantiate(conjureAbility.ObjectToSpawn, transform.position, transform.rotation);

        splitAlready += 1;
        if (newObject.TryGetComponent(out Aura newAura))
        {
            newAura.HandleInstantiation(caster, null, ability);
            newAura.splitAlready = splitAlready;
            newAura.ignoreTargets.Add(livingBeing);
        }
    }

    private IEnumerator SeekTarget(LivingBeing target)
    {
        //if (target == null) yield break;
        if (target == null)
        {
            target = caster;
        }
        if (TryGetComponent(out Rigidbody rb))
        {

            while (true)
            {
                if (target == null) yield break;

                Vector3 directionToTarget = target.transform.position - transform.position;

                if (directionToTarget.sqrMagnitude <= radius * radius)
                    yield break;

                if (rb != null)
                {
                    rb.linearVelocity = directionToTarget.normalized * speed;
                    transform.LookAt(directionToTarget);
                }

                yield return null;
            }
        }
    }

}

