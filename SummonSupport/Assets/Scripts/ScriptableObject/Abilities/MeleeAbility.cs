using UnityEngine;
using System.Collections.Generic;
using System;
using System.IO;
using System.Collections;
using UnityEngine.UIElements;

[CreateAssetMenu(menuName = "Abilities/Melee Ability")]

public class MeleeAbility : Ability
{
    [Header("Melee Ability settings")]


    [field: SerializeField] public float Range { get; private set; }
    [field: SerializeField] public float Angle { get; private set; }
    [field: SerializeField] public float Width { get; private set; }
    [field: SerializeField] public MeleeMovementType MovementType { get; private set; } // will use the range specified above
    [field: SerializeField] public AreaOfEffectShape Shape { get; private set; }
    [field: SerializeField] public GameObject SpawnEffectOnHit { get; private set; }
    [field: SerializeField] public GameObject MeleeParticleSystem { get; protected set; }

    private Transform originTransform;
    private LivingBeing Caster;
    private LivingBeing Target;

    private WaitForSeconds movementWait = new WaitForSeconds(.1f);






    public override bool Activate(GameObject user)
    {
        Caster = user.GetComponent<LivingBeing>();
        if (originTransform == null)
        {
            originTransform = user.GetComponent<AbilityHandler>().abilityDirection.transform;
        }
        if (MovementType != MeleeMovementType.None)
        {
            CoroutineManager.Instance.StartCustomCoroutine(HandleSpecialMovement(user, originTransform));
            return true;
        }
        else
            return AttemptActivation(user);

    }
    private void UseWeaponOrSetEffect(GameObject user)
    {
        if (Caster.TryGetComponent<AbilityHandler>(out AbilityHandler abilityHandler) && abilityHandler.weaponSlot != null)
            abilityHandler.weaponSlot.GetComponent<WeaponMono>().UseWeapon();

        else SetEffects(user);

    }

    private IEnumerator HandleSpecialMovement(GameObject user, Transform originTransform)
    {
        Rigidbody2D rb = user.GetComponent<Rigidbody2D>();

        if (MovementType == MeleeMovementType.Charge)
        {
            ToggleStuckInAbilityAnimation(user, true);
        }
        Vector2 startLoc = user.transform.position;

        while ((startLoc - (Vector2)user.transform.position).magnitude < Range) // while the distance between current loc and startLoc is less than Range
        {
            Debug.Log("handling special movement?");
            rb.linearVelocity = originTransform.right * 6;
            yield return movementWait;
        }

        ToggleStuckInAbilityAnimation(user, false);
        AttemptActivation(user);
    }
    private void ToggleStuckInAbilityAnimation(GameObject user, bool stuck)
    {
        if (Caster.CharacterTag != CharacterTag.Player)
        {
            AIStateHandler stateHandler = user.GetComponent<AIStateHandler>();
            stateHandler.SetStuckInAbilityAnimation(stuck);
        }
        else
        {
            user.TryGetComponent<PlayerMovement>(out PlayerMovement playerMovemeentScript);
            playerMovemeentScript.SetStuckInAbilityAnimation(stuck);
        }
    }

    private bool AttemptActivation(GameObject user)
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(user.transform.position, Range);
        bool activated = false;
        foreach (Collider2D collider in hits)
        {
            if (collider != null && collider.gameObject != user)
            {
                if (VerifyActivate(collider, user))
                {
                    Target = collider.GetComponent<LivingBeing>();

                    UseWeaponOrSetEffect(user);
                    //                    Logging.Info("verified and rarified");
                    CombatStatHandler.HandleEffectPackages(this, Caster, Target);
                    SpawnHitEffect(Target);
                    activated = true;
                }
            }

        }
        return activated;
    }


    private bool VerifyActivate(Collider2D collider, GameObject user)
    {

        if (!collider.TryGetComponent<LivingBeing>(out LivingBeing targetStats))
        {
            //Logging.Info("oof, 1");

            return false;
        }
        Target = targetStats;

        //Logging.Info($"User = {user} collider = {collider.gameObject}");

        if (!IsUsableOn(Caster.CharacterTag, Target.CharacterTag))
        {
            if (!HasElementalSynergy(this, Target) || !user.TryGetComponent<AI_CC_State>(out AI_CC_State ccState) || !ccState.isCharmed)
                return false;
        }
        //else Debug.Log($"usable on {collider.gameObject}");
        if (VerifyWithinShape(user, collider))
        {
            //Logging.Info("yay, 3");

            return true;
        }
        else
        {
            //Logging.Info("oof, 4");

            return false;
        }
    }

    private bool VerifyWithinShape(GameObject user, Collider2D collider)
    {

        DebugAbilityShape(originTransform);
        Vector2 hitLocation = collider.transform.position;
        if ((hitLocation - (Vector2)originTransform.position).magnitude <= .2)
            return true;
        if (Shape == AreaOfEffectShape.Sphere)
            return true;
        if (Shape == AreaOfEffectShape.Cone)
        {
            if (Vector2.Angle(hitLocation, originTransform.position) <= Angle)
                return true;

            else return false;
        }
        if (Shape == AreaOfEffectShape.Rectangle)
        {
            DebugAbilityShape(originTransform);
            Vector2 origin = originTransform.position;
            Vector2 forward = originTransform.right.normalized; // direction player is facing
            Vector2 side = originTransform.up.normalized;       // perpendicular to forward

            Vector2 toHit = hitLocation - origin;

            // Project hit position into local space of the rectangle
            float forwardDistance = Vector2.Dot(toHit, forward); // Distance in front of player
            float sideDistance = Vector2.Dot(toHit, side);       // Side offset (left/right)

            // Check if point is inside the rectangle
            bool isInside = forwardDistance <= Range && (Mathf.Abs(sideDistance) <= Width / 2f); //forwardDistance >= 0 &&

            //Logging.Info($"isInside? = {isInside}");
            if (isInside)
                return true;
            else return false;
        }
        else return false;
    }


    private void DebugAbilityShape(Transform AbilityRotation)

    {

        Transform AR = AbilityRotation.transform;
        Debug.DrawRay(AR.position, AR.up * Width / 2, Color.black, .8f); // char to right corner

        Debug.DrawRay(AR.position, -AR.up * Width / 2, Color.black, .8f); // char to leftcorner

        Debug.DrawRay(AR.position, AR.right * Range / 2, Color.black, .8f); // char to top center

        Debug.DrawRay(AR.right * Width / 2 + AR.position, AR.up * Range, Color.black, .8f); // top center to top right corner

        Debug.DrawRay(AR.right * Width / 2 + AR.position, -AR.up * Range, Color.black, .8f); // top center to top left corner

        Debug.DrawRay(AR.up * Width / 2 + AR.position, AR.right * Range, Color.black, .8f); //right corner to top right corner

        Debug.DrawRay(-AR.up * Width / 2 + AR.position, AR.right * Range, Color.black, .8f); //left corner to top left corner
    }

    private void SpawnHitEffect(LivingBeing targetLivingBeing)
    {
        GameObject instance;
        if (SpawnEffectOnHit != null)
        {
            instance = Instantiate(SpawnEffectOnHit, targetLivingBeing.transform.position, Quaternion.identity, targetLivingBeing.transform.transform);
            Destroy(instance, 3f);
        }

    }
    private void SetEffects(GameObject caster)
    {
        GameObject particleSystem;
        originTransform = caster.GetComponent<AbilityHandler>().abilityDirection.transform;

        if (MeleeParticleSystem != null)
        {
            particleSystem = Instantiate(MeleeParticleSystem, caster.transform.position, Quaternion.identity);
            float angle = Mathf.Atan2(-originTransform.transform.up.y, -originTransform.transform.up.x) * Mathf.Rad2Deg;

            particleSystem.transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle));
            Destroy(particleSystem, 2f);

        }
    }

}

