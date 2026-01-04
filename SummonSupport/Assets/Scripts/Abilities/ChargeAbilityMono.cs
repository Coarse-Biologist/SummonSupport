using System.Collections;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;

public class ChargeAbilityMono : MonoBehaviour
{
    private ChargeAbility chargeAbility;

    private Coroutine chargeCoroutine;
    private Rigidbody rb;
    private Transform originTransform;
    private Vector3 startLoc;
    private AbilityHandler abilityHandler;
    private LivingBeing caster;
    private WaitForSeconds chargeTickRate = new WaitForSeconds(.01f);
    public GameObject trailEffect { private set; get; }
    private AbilityModHandler modHandler;
    private float speedBoost = 35;
    private float range = 0;
    private int alreadypierced = 0;
    private int maxPierce = 0;


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
            speedBoost += movementScript.MovementSpeed;
        }
        range = chargeAbility.Range;

        if (modHandler != null)
        {
            maxPierce = modHandler.GetModAttributeByType(chargeAbility.ActivateOnHit, AbilityModTypes.MaxPierce);
            range += modHandler.GetModAttributeByType(chargeAbility.ActivateOnHit, AbilityModTypes.Range);
            speedBoost += modHandler.GetModAttributeByType(chargeAbility, AbilityModTypes.Speed);

        }
        Debug.Log($"Max pierce = {maxPierce}");

        originTransform = user.transform;
        rb = user.GetComponentInParent<Rigidbody>();
        chargeCoroutine = StartCoroutine(ChargeWhileLogical());
    }
    private IEnumerator ChargeWhileLogical()
    {
        if (chargeAbility.chargeTrail != null)
        {
            trailEffect = Instantiate(chargeAbility.chargeTrail, transform.position, originTransform.rotation, transform);
        }

        bool stillCharging = true;
        startLoc = transform.position;
        while (stillCharging)
        {
            rb.linearVelocity = originTransform.forward * speedBoost;
            if ((gameObject.transform.position - startLoc).magnitude > chargeAbility.Range)
            {
                EndCharge();
            }
            yield return chargeTickRate;
        }
    }

    void OnTriggerEnter(Collider collision)
    {
        //if (collision is BoxCollider wall) EndCharge();
        Ability abilityToCast = chargeAbility.ActivateOnHit;
        bool success = abilityToCast.Activate(transform.parent.gameObject);
        if (success)
        {
            if (chargeAbility.OnHitEffect != null)
            {
                Instantiate(chargeAbility.OnHitEffect, collision.transform.position, quaternion.identity, collision.transform);
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
