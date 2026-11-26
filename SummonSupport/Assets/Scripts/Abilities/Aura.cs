using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.Splines;


public class Aura : MonoBehaviour
{
    public const string AuraGroupSuffix = " Aura Group";

    LivingBeing caster;
    Ability ability;

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
    private int pierceMod = 0;
    private int effectiveMaxPierce = int.MaxValue;
    private int piercedAlready = 0;
    private int maxSplit = 0;
    private int splitAlready = 0;



    void Start()
    {
        if (gameObject.TryGetComponent(out ParticleSystem systemParticular))
            ps = systemParticular;
        //Invoke("Activate", ActivationTime);
    }

    public static Transform CreateAuraGroup(LivingBeing caster, Ability ability)
    {
        if (caster == null || ability == null) return null;

        string abilityName = string.IsNullOrEmpty(ability.Name) ? ability.name : ability.Name;
        if (abilityName.EndsWith(" Aura"))
            abilityName = abilityName.Substring(0, abilityName.Length - 5);

        GameObject group = new($"{abilityName}{AuraGroupSuffix}");
        group.transform.SetParent(caster.transform, false);
        group.transform.localPosition = Vector3.zero;
        group.transform.localRotation = Quaternion.identity;
        return group.transform;
    }

    private void OnDestroy()
    {
        Transform parent = transform.parent;
        if (parent != null && parent.name.EndsWith(AuraGroupSuffix) && parent.childCount <= 1)
            Destroy(parent.gameObject, .1f);
    }

    public void HandleInstantiation(LivingBeing caster, LivingBeing target, Ability ability)
    {

        SetAuraStats(caster, target, ability);

        HandleConjureAbilitySpecifics();

        if (ability is AuraAbility auraAbility)
            radius = auraAbility.Radius;

        AddMods();

        StartSplitSeek();

        effectiveMaxPierce = ConjureAbility != null && ConjureAbility.SeeksTarget
            ? maxPierce
            : (pierceMod > 0 ? pierceMod : int.MaxValue);

        AlterApparentRadius();

        // Split auras pick their target first and should only apply effects when they reach it.
        if (splitAlready <= 0)
            CheckTargetsAtSpawnTime();

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
            // Splits with no valid target stay where they spawned instead of flying back to the caster.
            if (conjureAbility.SeeksTarget && (splitAlready <= 0 || target != null))
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
    public void CheckTargetsAtSpawnTime()
    {
        Vector3 position = transform.position;
        Collider[] hits = Physics.OverlapCapsule(position + Vector3.up, position - Vector3.up, radius);
        foreach (Collider hitObject in hits)
        {
            if (!hitObject.TryGetComponent(out LivingBeing livingBeing)) continue;
            if (ignoreTargets.Contains(livingBeing)) continue;
            HandleTriggerEntered(livingBeing);
        }
    }

    private void AddMods()
    {
        duration += AbilityModHandler.GetPlayerModValue(ability, AbilityModTypes.Duration, caster);
        radius += AbilityModHandler.GetPlayerModValue(ability, AbilityModTypes.Size, caster);
        speed += AbilityModHandler.GetPlayerModValue(ability, AbilityModTypes.Speed, caster);
        pierceMod = Mathf.Max(0, AbilityModHandler.GetPlayerModValue(ability, AbilityModTypes.MaxPierce, caster));
        maxPierce += pierceMod;
        maxSplit = Mathf.Max(0, AbilityModHandler.GetPlayerModValue(ability, AbilityModTypes.MaxSplit, caster));
        //Debug.Log($"max split = {maxSplit}");
    }

    private void StartSplitSeek()
    {
        if (splitAlready <= 0 || target == null) return;
        // Conjure abilities that seek already started their own coroutine.
        if (ConjureAbility != null && ConjureAbility.SeeksTarget) return;
        StartCoroutine(SeekTarget(target));
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

            if (splitAlready < maxSplit)
            {
                SplitAura(otherLivingBeing);
            }
            if (piercedAlready >= effectiveMaxPierce)
            {
                Active = false;
                Destroy(gameObject, .2f);
            }
        }
    }
    private LivingBeing FindSplitTarget(LivingBeing justHit, int splitIndex)
    {
        if (caster == null || ability == null) return null;

        float searchRadius = 10f;
        if (ability is ConjureAbility conjureAbility && conjureAbility.SearchRadius > 0)
            searchRadius = conjureAbility.SearchRadius;
        else if (ability is AuraAbility auraAbility && auraAbility.SplitSearchRadius > 0)
            searchRadius = auraAbility.SplitSearchRadius;

        TeamType desiredTeam = ability.GetTargetPreference(caster);
        List<CharacterTag> validTags = CrewsRelationshipHandler.TargetTypeToCharTab(desiredTeam);

        Collider[] hits = Physics.OverlapSphere(transform.position, searchRadius);
        List<LivingBeing> candidates = new();
        foreach (Collider hit in hits)
        {
            if (!hit.TryGetComponent(out LivingBeing candidate)) continue;
            if (candidate == caster || candidate == justHit) continue;
            if (ignoreTargets.Contains(candidate) || candidates.Contains(candidate)) continue;
            if (!validTags.Contains(candidate.CharacterTag)) continue;
            if (!ability.ThoroughIsUsableOn(caster, candidate)) continue;
            candidates.Add(candidate);
        }

        if (candidates.Count == 0) return null;

        // Spread successive splits across nearby targets instead of stacking them on one being.
        Vector3 origin = transform.position;
        candidates.Sort((a, b) =>
            (a.transform.position - origin).sqrMagnitude.CompareTo((b.transform.position - origin).sqrMagnitude));

        return candidates[(Mathf.Max(1, splitIndex) - 1) % candidates.Count];
    }

    private void SplitAura(LivingBeing livingBeing)
    {
        GameObject newObject = null;
        Transform auraParent = transform.parent;
        if (ability is ConjureAbility conjureAbility)
            newObject = Instantiate(conjureAbility.ObjectToSpawn, transform.position, transform.rotation, auraParent);
        else if (ability is AuraAbility auraAbility)
            newObject = Instantiate(auraAbility.AuraObject, transform.position, transform.rotation, auraParent);

        if (newObject == null) return;

        splitAlready += 1;
        LivingBeing splitTarget = FindSplitTarget(livingBeing, splitAlready);

        if (newObject.TryGetComponent(out Aura newAura))
        {
            // Set split state before spawning so spawn-time target checks cannot cascade.
            newAura.splitAlready = splitAlready;
            newAura.ignoreTargets.AddRange(ignoreTargets);
            newAura.ignoreTargets.Add(livingBeing);
            newAura.HandleInstantiation(caster, splitTarget, ability);
        }
        else Destroy(newObject);
    }

    private IEnumerator SeekTarget(LivingBeing target)
    {
        //if (target == null) yield break;
        if (target == null)
        {
            target = caster;
        }

        bool usesRigidbody = TryGetComponent(out Rigidbody rb);

        while (target != null)
        {
            Vector3 directionToTarget = target.transform.position - transform.position;

            if (directionToTarget.sqrMagnitude <= radius * radius)
                yield break;

            Vector3 direction = directionToTarget.normalized;
            if (usesRigidbody)
                rb.linearVelocity = direction * speed;
            else
                transform.position += direction * speed * Time.deltaTime;

            transform.LookAt(target.transform.position);

            yield return null;
        }
    }

}

