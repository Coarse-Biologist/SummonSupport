using System.Collections;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;

public class ChargeAbilityMono : MonoBehaviour
{
    private ChargeAbility chargeAbility;

    private Coroutine chargeCoroutine;
    private Rigidbody2D rb;
    private Transform originTransform;
    private Vector2 startLoc;
    private AbilityHandler abilityHandler;
    private LivingBeing caster;
    private WaitForSeconds chargeTickRate = new WaitForSeconds(.01f);
    public GameObject trailEffect { private set; get; }
    private AbilityModHandler modHandler;
    private float speed = 0;
    private float range = 0;
    private int alreadypierced = 0;
    private int maxPierce = 0;

    double maxChargeTime = 2f;

    [field: SerializeField] public StatusEffects attackAnimationCC;

    public void Charge(ChargeAbility assignedAbility)
    {
        GameObject user = transform.parent.gameObject;
        ToggleStuckInAbilityAnimation(user, true);
        chargeAbility = assignedAbility;
        abilityHandler = user.GetComponentInParent<AbilityHandler>();
        abilityHandler.SetCharging(true);
        caster = user.GetComponentInParent<LivingBeing>();
        modHandler = caster.GetComponent<AbilityModHandler>();
        if (caster.gameObject.TryGetComponent(out MovementScript movementScript))
        {
            speed = movementScript.MovementSpeed;
        }
        range = chargeAbility.range;

        if (modHandler != null)
        {
            maxPierce = modHandler.GetModAttributeByType(chargeAbility.ActivateOnHit, AbilityModTypes.MaxPierce);
        }
        Debug.Log($"Max pierce = {maxPierce}");

        originTransform = abilityHandler.abilityDirection.transform;
        rb = user.GetComponentInParent<Rigidbody2D>();
        chargeCoroutine = StartCoroutine(ChargeWhileLogical());
    }
    private IEnumerator ChargeWhileLogical()
    {
        if (chargeAbility.chargeTrail != null)
        {
            trailEffect = Instantiate(chargeAbility.chargeTrail, transform.position, originTransform.rotation, transform);
        }
        if (modHandler != null)
        {
            speed += modHandler.GetModAttributeByType(chargeAbility, AbilityModTypes.Speed);
            range += modHandler.GetModAttributeByType(chargeAbility, AbilityModTypes.Range);
        }
        bool stillCharging = true;
        startLoc = transform.position;
        while (stillCharging && maxChargeTime > 0)
        {
            rb.linearVelocity = originTransform.right * speed * 35;
            if (((Vector2)gameObject.transform.position - startLoc).magnitude > range)
            {
                EndCharge();
            }
            maxChargeTime -= .01;
            yield return chargeTickRate;
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        Ability abilityToCast = chargeAbility.ActivateOnHit;
        bool success = abilityToCast.Activate(transform.parent.gameObject);
        if (success)
        {
            if (chargeAbility.HitEffect != null)
            {
                Instantiate(chargeAbility.HitEffect, collision.transform.position, quaternion.identity, collision.transform);
                alreadypierced++;
            }
            if (alreadypierced > maxPierce)
                EndCharge();
        }
    }

    private void EndCharge()
    {
        if (trailEffect != null) Destroy(trailEffect);
        if (chargeCoroutine != null) StopCoroutine(chargeCoroutine);
        abilityHandler.SetCharging(false);
        ToggleStuckInAbilityAnimation(transform.parent.gameObject, false);
        Destroy(gameObject);
    }



    private void ToggleStuckInAbilityAnimation(GameObject user, bool stuck)
    {
        if (user.TryGetComponent<AI_CC_State>(out AI_CC_State ccState))
        {
            if (stuck) ccState.RecieveCC(attackAnimationCC, caster);
            else if (!stuck) ccState.RemoveCC(StatusEffectType.AttackAnimation);
        }

        else if (user.TryGetComponent<PlayerMovement>(out PlayerMovement playerMovementScript))
            playerMovementScript.SetStuckInAbilityAnimation(stuck);
        else Debug.Log($"No CcState script or playermovement script found on the sought gameObject: {user}");
    }

}
