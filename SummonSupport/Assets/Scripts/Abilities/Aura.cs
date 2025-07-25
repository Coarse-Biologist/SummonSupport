using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Aura : MonoBehaviour
{
    LivingBeing caster;
    Ability ability;
    List<LivingBeing> listLivingBeingsInAura = new();
    [SerializeField] public float ActivationTime { get; private set; } = .5f;
    private bool Active = false;

    private GameObject Target;
    private ConjureAbility conjureAbility;



    public void HandleInstantiation(LivingBeing caster, Ability ability, float radius, float duration)
    {
        if (ability is ConjureAbility) conjureAbility = (ConjureAbility)ability;
        GetComponent<CircleCollider2D>().radius = radius;
        Invoke("Activate", ActivationTime);
        SetAuraStats(caster, ability, duration);
        CombatStatHandler.HandleEffectPackages(ability, caster, caster, true);
        if (conjureAbility.SeeksTarget)
        {
            Target = FindTarget(conjureAbility.SearchRadius);
            if (Target != null) StartCoroutine(SeekTarget(Target));
        }
    }
    public void SetAuraStats(LivingBeing caster, Ability ability, float duration)
    {
        this.caster = caster;
        this.ability = ability;
        SetAuraTimer(duration);
        //transform.Rotate(new Vector3(-110f, 0, 0));

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
        Debug.Log("This happens! 8");

        if (Active)
        {
            Logging.Info($"{other.gameObject} has entered the bite zone");
            if (other.gameObject.TryGetComponent(out LivingBeing otherLivingBeing))
            {
                Debug.Log("This happens! 7");

                if (ability.ListUsableOn.Contains(RelationshipHandler.GetRelationshipType(caster.CharacterTag, otherLivingBeing.CharacterTag)) && !listLivingBeingsInAura.Contains(otherLivingBeing))
                {
                    CombatStatHandler.HandleEffectPackages(ability, caster, otherLivingBeing, false);
                }
            }
            else Debug.Log($"other game object = {other.gameObject}");
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        LivingBeing otherLivingBeing = other.gameObject.GetComponent<LivingBeing>();
        if (listLivingBeingsInAura.Contains(otherLivingBeing))
        {         //possibly remove aura effect? 
        }
    }


    void OnDestroy()
    {
        //foreach (LivingBeing livingBeing in listLivingBeingsInAura.ToList())
        // remove status effects ?
    }


    private GameObject FindTarget(float SearchRadius)
    {
        GameObject target = null;
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, SearchRadius);
        foreach (Collider2D collider in colliders)
        {
            if (!collider.TryGetComponent<LivingBeing>(out LivingBeing targetStats))
                continue;
            if (!ability.IsUsableOn(targetStats.CharacterTag, caster.CharacterTag))
                continue;
            else return collider.gameObject;
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
