using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.Splines;


public class Aura : MonoBehaviour
{
    LivingBeing caster;
    Ability ability;
    List<LivingBeing> listLivingBeingsInAura = new();
    [SerializeField] public float ActivationTime { get; private set; } = .5f;
    private bool Active = false;
    private CircleCollider2D circleCollider;

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
        Debug.Log("Aura script starts here and now");
        if (SetSplineAnimator())
        {
            if (TryGetComponent<CircleCollider2D>(out CircleCollider2D colliderCito))
            {
                circleCollider = colliderCito;
                circleCollider.enabled = false;
                //Debug.Log($"Disabling {circleCollider}"); 
                InvokeRepeating("CheckEndNear", .5f, .01f);
            }
        }
        Invoke("Activate", ActivationTime);
        if (TryGetComponent<Light2D>(out Light2D FoundlightScript))
        {
            Debug.Log("Light script detected");
            lightScript = FoundlightScript;
            StartCoroutine(LightManager.MakeLightOscillate(lightScript));
            if (TryGetComponent<ParticleSystem>(out ParticleSystem ps))
            {
                Debug.Log("changing color to color");
                lightScript.color = Color.green;
                lightScript.intensity = 3;
                Debug.Log($"changing color to {lightScript.color}");

            }
        }
        else Debug.Log("No light script found");
    }
    public void HandleInstantiation(LivingBeing caster, LivingBeing target, Ability ability, float radius, float duration)
    {
        if (TryGetComponent<CircleCollider2D>(out CircleCollider2D collider))
            collider.radius = radius;
        SetAuraStats(caster, target, ability, duration);
        CombatStatHandler.HandleEffectPackages(ability, caster, caster, true);
        if (ability is ConjureAbility)
        {
            conjureAbility = (ConjureAbility)ability;
            if (conjureAbility.SeeksTarget)
            {
                this.target = FindTarget(conjureAbility.SearchRadius);
                if (this.target != null) StartCoroutine(SeekTarget(this.target.gameObject));
            }
        }
        Destroy(gameObject, duration);
    }
    public void SetAuraStats(LivingBeing caster, LivingBeing target, Ability ability, float duration)
    {
        this.caster = caster;
        this.ability = ability;
        this.target = target;
        SetAuraTimer(duration);
        Activate();
        //transform.Rotate(new Vector3(-110f, 0, 0));

    }
    private void CheckEndNear()
    {
        //Debug.Log($"Spline animator normalized  time =  {splineAnimator.NormalizedTime}");

        if (splineAnimator.NormalizedTime > .75)
        {
            circleCollider.enabled = true;
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


    void OnTriggerEnter2D(Collider2D other)
    {
        if (Active)
        {
            if (other.gameObject.TryGetComponent(out LivingBeing otherLivingBeing))
            {
                if (ability.ListUsableOn.Contains(RelationshipHandler.GetRelationshipType(caster.CharacterTag, otherLivingBeing.CharacterTag)))
                {
                    //Debug.Log($"usable on {otherLivingBeing.Name}");
                    if (ability is ConjureAbility)
                    {
                        SpawnOnHitEffect(otherLivingBeing, conjureAbility.SpawnEffectOnHit);
                    }
                    otherLivingBeing.AlterAbilityList(ability, true);
                    Debug.Log($"Effects of {ability.Name} is being handled.");
                    CombatStatHandler.HandleEffectPackages(ability, caster, otherLivingBeing, false);
                }
            }
            else Debug.Log($"NOT usable on {other.gameObject.name}");
        }
        else Debug.Log($"OnTrigger Enter func called but the trigger object is not active");
    }

    void OnTriggerExit2D(Collider2D other)
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
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, SearchRadius);
        foreach (Collider2D collider in colliders)
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
        Vector2 directionToTarget = target.transform.position - transform.position;
        TryGetComponent<Rigidbody2D>(out Rigidbody2D rb);

        while (directionToTarget.sqrMagnitude > conjureAbility.Radius)
        {
            yield return waitFor;
            if (rb != null) rb.linearVelocity = (target.transform.position - transform.position) * conjureAbility.Speed;
            //transform.position = Vector2.Lerp(transform.position, target.transform.position, conjureAbility.Speed);
        }
    }
}
