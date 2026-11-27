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
    private WaitForSeconds chargeTickRate = new WaitForSeconds(.1f);
    private GameObject trailEffect;
    [field: SerializeField] public float SpeedBoost { private set; get; } = 10;
    private float range = 0;
    private int alreadypierced = 0;
    private int maxPierce = 0;

    public void Charge(LivingBeing casterStats, ChargeAbility assignedAbility)
    {
        Debug.Log($"caster = {casterStats}. Assigned ability = {assignedAbility}");
        GameObject user = transform.parent.gameObject;
        chargeAbility = assignedAbility;
        abilityHandler = casterStats.abilityHandler;
        caster = casterStats;
        abilityHandler.SetCharging(true);
        ToggleStuckInAbilityAnimation(user, true);

        SetRunAnimation();
        range = chargeAbility.Range;
        SetAndApplyMods();


        //Debug.Log($"Max pierce = {maxPierce}");

        originTransform = user.transform;
        rb = user.GetComponentInParent<Rigidbody>();
        chargeCoroutine = StartCoroutine(ChargeWhileLogical());
    }
    private void SetAndApplyMods()
    {
        Ability onHitAbility = chargeAbility.ActivateOnHit;
        maxPierce = Mathf.Max(0, AbilityModHandler.GetPlayerModValue(onHitAbility, AbilityModTypes.MaxPierce, caster));
        range += Mathf.Max(0, AbilityModHandler.GetPlayerModValue(onHitAbility, AbilityModTypes.Size, caster));
        SpeedBoost += Mathf.Max(0, AbilityModHandler.GetPlayerModValue(chargeAbility, AbilityModTypes.Speed, caster));

        if (caster.gameObject.TryGetComponent(out MovementScript movementScript))
        {
            SpeedBoost += movementScript.MovementSpeed;
        }
    }

    private void SetRunAnimation()
    {
        if (abilityHandler.anim != null)
        {
            if (caster.CharacterTag == CharacterTag.Player)
                abilityHandler.anim.ChangeAnimation("Sprint", .1f);
            else abilityHandler.anim.ChangeAnimation("Run", .1f);
        }
    }

    private IEnumerator ChargeWhileLogical()
    {
        if (chargeAbility.chargeTrail != null)
        {
            trailEffect = Instantiate(chargeAbility.chargeTrail, transform.position, originTransform.rotation, transform);
        }

        bool stillCharging = true;
        startLoc = transform.position;
        // A charge that gets blocked, or a copy of the caster on another machine that is moved by the network
        // and stops short, would otherwise never end and leave the caster stuck charging.
        float endTime = Time.time + range / Mathf.Max(1f, SpeedBoost) * 1.5f + .5f;
        while (stillCharging)
        {
            // Copies of the caster on other machines are kinematic and moved by NetworkTransform.
            if (!rb.isKinematic) rb.linearVelocity = originTransform.forward * SpeedBoost;
            if ((gameObject.transform.position - startLoc).magnitude > range || Time.time > endTime)
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
        if (abilityToCast == null)
        {
            EndCharge();
            return;
        }
        bool success = abilityToCast.Activate(caster);
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
        Debug.Log($"user = {user} caster = {caster}");
        if (caster.CharacterTag != CharacterTag.Player)
        {
            AIStateHandler stateHandler = user.GetComponent<AIStateHandler>();
            stateHandler.SetStuckInAbilityAnimation(stuck);
        }

        else if (user.TryGetComponent(out PlayerMovement playerMovementScript))
        {
            if (stuck)
                playerMovementScript.PauseGame();
            else playerMovementScript.UnpauseGame();
        }

        else Debug.Log($"No CcState script or playermovement script found on the sought gameObject: {user}");
    }
}
