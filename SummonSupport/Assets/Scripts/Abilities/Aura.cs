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

    List<LivingBeing> listLivingBeingsInAura = new();
    [SerializeField] public float ActivationTime { get; private set; } = .5f;
    [field: SerializeField] public float SizeScalar = .1f;
    private bool Active = false;
    private Collider sphereCollider;

    private LivingBeing target;
    private ConjureAbility ConjureAbility = null;
    private ParticleSystem ps;
    private float radius = 0;
    private float speed = 1;
    private float duration;
    private int maxPierce = 0;
    private int piercedAlready = 0;



    void Start()
    {
        if (gameObject.TryGetComponent(out ParticleSystem systemParticular))
            ps = systemParticular;
        Invoke("Activate", ActivationTime);
    }
    public void HandleInstantiation(LivingBeing caster, LivingBeing target, Ability ability)
    {

        if (ability.AlterParticleSystemGradient) ColorChanger.ChangeObjectsParticleSystemColor(ability.ElementTypes[0], gameObject);
        SetAuraStats(caster, target, ability);

        HandleConjureAbilitySpecifics();


        if (ability is AuraAbility auraAbility)
            radius = auraAbility.Radius;
        AddMods();


        //if (TryGetComponent(out CapsuleCollider collider))
        //collider.radius = Math.Max(radius, 1);
        AlterApparentRadius();

        CombatStatHandler.HandleEffectPackage(ability, caster, caster, ability.SelfEffects);
        Destroy(gameObject, duration);

    }
    private void AlterApparentRadius()
    {
        //gameObject.transform.localScale = new Vector3(gameObject.transform.localScale.x + radius, gameObject.transform.localScale.y, gameObject.transform.localScale.z + radius);
        gameObject.transform.localScale *= 1 + radius * SizeScalar;// + radius, gameObject.transform.localScale.y + radius, gameObject.transform.localScale.z + radius);
        Debug.Log($" radius = {radius}");
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
                if (target != null) StartCoroutine(SeekTarget(target.gameObject));
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

    private void AddMods()
    {
        modHandler = caster.GetComponent<AbilityModHandler>();
        if (modHandler != null)
        {
            duration += modHandler.GetModAttributeByType(ability, AbilityModTypes.Duration);
            radius += modHandler.GetModAttributeByType(ability, AbilityModTypes.Size);
            speed += modHandler.GetModAttributeByType(ability, AbilityModTypes.Speed);
            maxPierce += modHandler.GetModAttributeByType(ability, AbilityModTypes.MaxPierce);
            Debug.Log($"ability = {ability.Name}. size mod value = {radius}");

        }
    }

    private void Activate()
    {
        Active = true;
    }

    void OnTriggerEnter(Collider other)
    {
        if (Active)
        {
            if (other.gameObject.TryGetComponent(out LivingBeing otherLivingBeing))
            {
                if (ability.ListUsableOn.Contains(RelationshipHandler.GetRelationshipType(caster.CharacterTag, otherLivingBeing.CharacterTag)))
                {
                    //Debug.Log($"usable on {otherLivingBeing.Name}");
                    if (ability.OnHitEffect != null)
                    {
                        SpawnOnHitEffect(otherLivingBeing, ability.OnHitEffect);
                    }
                    otherLivingBeing.AlterAbilityList(ability, true);
                    //Debug.Log($"Effects of {ability.Name} is being handled.");
                    CombatStatHandler.HandleEffectPackage(ability, caster, otherLivingBeing, ability.TargetEffects);
                    piercedAlready += 1;
                    if (ConjureAbility != null && ConjureAbility.SeeksTarget && piercedAlready >= maxPierce) Destroy(gameObject, .2f);
                }
            }
            else Debug.Log($"NOT usable on {other.gameObject.name}");
        }
        else Debug.Log($"OnTrigger Enter func called but the trigger object is not active");
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

    private LivingBeing FindTarget(float SearchRadius)
    {
        LivingBeing target = null;
        Collider[] colliders = Physics.OverlapSphere(transform.position, SearchRadius);
        foreach (Collider collider in colliders)
        {
            if (!collider.TryGetComponent<LivingBeing>(out LivingBeing targetStats))
                continue;
            if (!ability.ThoroughIsUsableOn(targetStats, caster))
                continue;
            else return targetStats;
        }
        return target;
    }

    private IEnumerator SeekTarget(GameObject target)
    {
        TryGetComponent(out Rigidbody rb);
        Debug.Log("Im here");

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

