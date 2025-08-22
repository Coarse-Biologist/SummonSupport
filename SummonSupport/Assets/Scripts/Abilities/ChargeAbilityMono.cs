using System.Collections;
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
    [field: SerializeField] public StatusEffects attackAnimationCC;
    void Start()
    {
        //startLoc = transform.position;
        //Debug.Log($"start loc = {startLoc}");
    }

    public void Charge(ChargeAbility assignedAbility)
    {
        GameObject user = transform.parent.gameObject;
        ToggleStuckInAbilityAnimation(user, true);
        chargeAbility = assignedAbility;
        abilityHandler = user.GetComponentInParent<AbilityHandler>();
        abilityHandler.SetCharging(true);
        caster = user.GetComponentInParent<LivingBeing>();
        originTransform = abilityHandler.abilityDirection.transform;
        rb = user.GetComponentInParent<Rigidbody2D>();
        chargeCoroutine = StartCoroutine(ChargeWhileLogical());
    }
    private IEnumerator ChargeWhileLogical()
    {
        bool stillCharging = true;
        startLoc = transform.position;
        Debug.Log($"start loc = {startLoc}");
        while (stillCharging)
        {
            rb.linearVelocity = originTransform.right * caster.GetAttribute(AttributeType.MovementSpeed) * 20;
            if (((Vector2)gameObject.transform.position - startLoc).magnitude > chargeAbility.range)
            {
                Debug.Log($"distance travelled: {((Vector2)gameObject.transform.position - startLoc).magnitude} Charge ability is being ended");

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
        Debug.Log("Ending charge");

        if (chargeCoroutine != null) StopCoroutine(chargeCoroutine);
        abilityHandler.SetCharging(false);
        ToggleStuckInAbilityAnimation(transform.parent.gameObject, false);
        Destroy(gameObject);
    }



    private void ToggleStuckInAbilityAnimation(GameObject user, bool stuck)
    {
        if (user.TryGetComponent<AI_CC_State>(out AI_CC_State ccState))
        {
            Debug.Log($"going to give cc {attackAnimationCC} to {user}");

            if (stuck) ccState.RecieveCC(attackAnimationCC, caster);
            else if (!stuck) ccState.RemoveCC(StatusEffectType.AttackAnimation);
        }

        else if (user.TryGetComponent<PlayerMovement>(out PlayerMovement playerMovementScript))
            playerMovementScript.SetStuckInAbilityAnimation(stuck);
    }

}
