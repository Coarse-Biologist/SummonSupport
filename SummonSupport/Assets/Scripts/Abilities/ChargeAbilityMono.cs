using System.Collections;
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
    private WaitForSeconds chargeTickRate = new WaitForSeconds(.1f);
    [field: SerializeField] public StatusEffects attackAnimationCC;
    private void Start()
    {
        startLoc = transform.position;
    }

    public void Charge(ChargeAbility assignedAbility)
    {
        ToggleStuckInAbilityAnimation(gameObject, true);
        chargeAbility = assignedAbility;
        abilityHandler = gameObject.GetComponentInParent<AbilityHandler>();
        abilityHandler.SetCharging(true);
        caster = gameObject.GetComponentInParent<LivingBeing>();
        originTransform = abilityHandler.abilityDirection.transform;
        rb = gameObject.GetComponentInParent<Rigidbody2D>();
        chargeCoroutine = StartCoroutine(ChargeWhileLogical());
    }
    private IEnumerator ChargeWhileLogical()
    {
        bool stillCharging = true;

        while (stillCharging)
        {
            rb.linearVelocity = originTransform.right * caster.GetAttribute(AttributeType.MovementSpeed) * 20;
            if ((startLoc - (Vector2)gameObject.transform.position).magnitude > chargeAbility.range)
            {
                EndCharge();
            }
            yield return chargeTickRate;
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        Ability abilityToCast = chargeAbility.ability;
        bool success = abilityToCast.Activate(transform.parent.gameObject);
        if (success)
        {

            EndCharge();
        }
    }

    private void EndCharge()
    {
        if (chargeCoroutine != null) StopCoroutine(chargeCoroutine);
        abilityHandler.SetCharging(false);
        ToggleStuckInAbilityAnimation(gameObject, false);
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
    }

}
