using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Aura : MonoBehaviour
{
    LivingBeing caster;
    Ability ability;
    List<LivingBeing> listLivingBeingsInAura = new();
    [SerializeField] public float ActivationTime { get; private set; } = .5f;
    private bool Active = false;

    private LivingBeing target;
    private ConjureAbility conjureAbility;



    public void HandleInstantiation(LivingBeing caster, LivingBeing target, Ability ability, float radius, float duration)
    {
        if (ability is ConjureAbility) conjureAbility = (ConjureAbility)ability;
        GetComponent<CircleCollider2D>().radius = radius;
        Invoke("Activate", ActivationTime);
        SetAuraStats(caster, target, ability, duration);
        CombatStatHandler.HandleEffectPackages(ability, caster, caster, true);
        if (conjureAbility.SeeksTarget)
        {
            this.target = FindTarget(conjureAbility.SearchRadius);
            if (this.target != null) StartCoroutine(SeekTarget(this.target.gameObject));
        }
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
            Logging.Info($"{other.gameObject} has entered the bite zone");
            if (other.gameObject.TryGetComponent(out LivingBeing otherLivingBeing))
            {

                if (ability.ListUsableOn.Contains(RelationshipHandler.GetRelationshipType(caster.CharacterTag, otherLivingBeing.CharacterTag)))
                {
                    otherLivingBeing.AlterAbilityList(ability, true);
                    CombatStatHandler.HandleEffectPackages(ability, caster, otherLivingBeing, false);
                }
            }
            else Debug.Log($"other game object = {other.gameObject}");
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.TryGetComponent<LivingBeing>(out LivingBeing otherLivingBeing))
            otherLivingBeing.AlterAbilityList(ability, false);

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
            if (!ability.IsUsableOn(targetStats.CharacterTag, caster.CharacterTag))
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
