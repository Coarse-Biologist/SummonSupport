using System.Collections;
using UnityEngine;

public class ChargeAbilityMono : MonoBehaviour
{
    private ChargeAbility chargeAbility;
    private float distanceTraveled = 0f;
    private float maxDistance;
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
            rb.linearVelocity = originTransform.right * 10;

            if ((startLoc - (Vector2)gameObject.transform.position).magnitude > chargeAbility.range)
            {
                Debug.Log($"traveled {(startLoc - (Vector2)gameObject.transform.position).magnitude}. max is {chargeAbility.range}. Ending Charge");
                EndCharge();
            }
            yield return chargeTickRate;
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("Gotcha!");
        bool success = chargeAbility.ability.Activate(transform.parent.gameObject);
        if (success)
        {
            Debug.Log($"ability was activated and therefor the charge is ending");

            EndCharge();
        }
    }

    private void EndCharge()
    {
        StopCoroutine(chargeCoroutine);
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

        else if (user.TryGetComponent<PlayerMovement>(out PlayerMovement playerMovemeentScript))
            playerMovemeentScript.SetStuckInAbilityAnimation(stuck);
    }

}
