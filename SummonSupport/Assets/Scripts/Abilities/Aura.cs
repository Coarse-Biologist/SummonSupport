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
    private bool Active = false;
    private Collider sphereCollider;

    private LivingBeing target;
    private ConjureAbility conjureAbility;
    private SplineAnimate splineAnimator;

    private UnityEngine.Rendering.Universal.Light2D lightScript;


    private bool SetSplineAnimator()
    {
        if (TryGetComponent<SplineAnimate>(out SplineAnimate spline))
        {
            splineAnimator = spline;
            return true;
        }
        else return false;
    }


    void Start()
    {
        //Debug.Log("Aura script starts here and now");
        if (SetSplineAnimator())
        {
            if (TryGetComponent(out Collider colliderCito))
            {
                sphereCollider = colliderCito;
                sphereCollider.enabled = false;
                InvokeRepeating("CheckEndNear", .5f, .01f);
            }
        }
        Invoke("Activate", ActivationTime);

    }
    public void HandleInstantiation(LivingBeing caster, LivingBeing target, Ability ability)
    {
        SetAuraStats(caster, target, ability, ability.Duration);
        float radius = 1;
        if (ability is ConjureAbility conjureAbility)
        {
            radius = conjureAbility.Radius;
            if (conjureAbility.SeeksTarget)
            {
                this.target = FindTarget(conjureAbility.SearchRadius);
                if (this.target != null) StartCoroutine(SeekTarget(this.target.gameObject));
            }
        }
        else if (ability is AuraAbility auraAbility)
            radius = auraAbility.Radius;

        if (TryGetComponent(out CapsuleCollider collider))
            collider.radius = radius;
        if (abilityMod != null) collider.radius += abilityMod.GetModdedAttribute(AbilityModTypes.Size);
        CombatStatHandler.HandleEffectPackage(ability, caster, caster, ability.SelfEffects);

        Destroy(gameObject, ability.Duration);
    }
    public void SetAuraStats(LivingBeing caster, LivingBeing target, Ability ability, float duration)
    {
        this.caster = caster;
        this.ability = ability;
        this.target = target;
        modHandler = caster.GetComponent<AbilityModHandler>();
        if (modHandler != null)
        {
        }
        SetAuraTimer(duration + modHandler.GetModAttributeByType(ability, AbilityModTypes.Duration));
        Activate();
        //transform.Rotate(new Vector3(-110f, 0, 0));

    }
    private void CheckEndNear()
    {
        //Debug.Log($"Spline animator normalized  time =  {splineAnimator.NormalizedTime}");

        if (splineAnimator.NormalizedTime > .75)
        {
            sphereCollider.enabled = true;
            //Debug.Log($"Enabling {circleCollider}");
            CancelInvoke("CheckEndNear");
        }

    }

    private void Activate()
    {
        Active = true;
    }

    public void SetAuraTimer(float timeUp)
    {
        Destroy(gameObject, timeUp);
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
                }
            }
            else Debug.Log($"NOT usable on {other.gameObject.name}");
        }
        else Debug.Log($"OnTrigger Enter func called but the trigger object is not active");
    }

    void OnTriggerExit(Collider other)
    {
        if (other.gameObject.TryGetComponent<LivingBeing>(out LivingBeing otherLivingBeing))
            otherLivingBeing.AlterAbilityList(ability, false);

    }
    private void SpawnOnHitEffect(LivingBeing targetLivingBeing, GameObject SpawnEffectOnHit)
    {
        GameObject instance;
        if (SpawnEffectOnHit != null)
        {
            //Debug.Log("This happens, excellent");
            instance = Instantiate(SpawnEffectOnHit, targetLivingBeing.transform.position, Quaternion.identity, targetLivingBeing.transform.transform);
            Destroy(instance, instance.GetComponent<ParticleSystem>().main.duration);
        }
        //else Debug.Log("This happens but is null");
    }


    void OnDestroy()
    {
        //foreach (LivingBeing livingBeing in listLivingBeingsInAura.ToList())
        // remove status effects ?
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
        WaitForSeconds waitFor = new WaitForSeconds(.4f);
        Vector3 directionToTarget = target.transform.position - transform.position;
        TryGetComponent(out Rigidbody rb);

        while (directionToTarget.sqrMagnitude > conjureAbility.Radius)
        {
            yield return waitFor;
            if (rb != null) rb.linearVelocity = (target.transform.position - transform.position) * conjureAbility.Speed;
            //transform.position = Vector2.Lerp(transform.position, target.transform.position, conjureAbility.Speed);
        }
    }
}

