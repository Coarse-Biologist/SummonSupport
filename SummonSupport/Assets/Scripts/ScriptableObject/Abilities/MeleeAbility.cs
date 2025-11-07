using UnityEngine;
using System.Collections.Generic;
using System;
using System.IO;
using System.Collections;
using UnityEngine.UIElements;
using UnityEngine.Splines;

[CreateAssetMenu(menuName = "Abilities/Melee Ability")]

public class MeleeAbility : Ability
{
    [Header("Melee Ability settings")]


    [field: SerializeField] public float Angle { get; private set; }
    [field: SerializeField] public float Width { get; private set; }
    [field: SerializeField] public AreaOfEffectShape Shape { get; private set; }
    [field: SerializeField] public GameObject SpawnEffectOnHit { get; private set; }
    [field: SerializeField] public GameObject MeleeParticleSystem { get; protected set; }

    private Transform originTransform;
    private LivingBeing Caster;
    private LivingBeing Target;
    private AbilityHandler abilityHandler;
    private AbilityModHandler modHandler;

    private WaitForSeconds movementWait = new WaitForSeconds(.1f);






    public override bool Activate(GameObject user)
    {
        Caster = user.GetComponent<LivingBeing>();
        abilityHandler = user.GetComponent<AbilityHandler>();
        modHandler = user.GetComponent<AbilityModHandler>();
        if (originTransform == null)
        {
            //Debug.Log($"the user: {user}.");
            originTransform = abilityHandler.abilityDirection.transform;
        }

        return AttemptActivation(user);

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

                    SetEffects(Caster, Target);
                    CombatStatHandler.HandleEffectPackage(this, Caster, Target, this.TargetEffects);
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
        float totalWidth = Width;
        if (modHandler != null)
        {
            totalWidth += modHandler.GetModAttributeByType(this, AbilityModTypes.Size);
        }
        DebugAbilityShape(originTransform, totalWidth);
        Vector2 hitLocation = collider.transform.position;
        if ((hitLocation - (Vector2)originTransform.position).magnitude <= .2)
            return true;
        if (Shape == AreaOfEffectShape.Sphere)
        {
            if ((user.transform.position - collider.transform.position).magnitude <= Range)
                return true;
        }
        if (Shape == AreaOfEffectShape.Cone)
        {
            if (Vector2.Angle(hitLocation, originTransform.position) <= Angle)
                return true;

            else return false;
        }
        if (Shape == AreaOfEffectShape.Rectangle)
        {
            DebugAbilityShape(originTransform, totalWidth);
            Vector2 origin = originTransform.position;
            Vector2 forward = originTransform.right.normalized; // direction player is facing
            Vector2 side = originTransform.up.normalized;       // perpendicular to forward

            Vector2 toHit = hitLocation - origin;

            // Project hit position into local space of the rectangle
            float forwardDistance = Vector2.Dot(toHit, forward); // Distance in front of player
            float sideDistance = Vector2.Dot(toHit, side);       // Side offset (left/right)

            // Check if point is inside the rectangle
            bool isInside = forwardDistance <= Range && (Mathf.Abs(sideDistance) <= totalWidth / 2f); //forwardDistance >= 0 &&

            //Logging.Info($"isInside? = {isInside}");
            if (isInside)
                return true;
            else return false;
        }
        else return false;
    }


    private void DebugAbilityShape(Transform AbilityRotation, float totalWidth)

    {

        Transform AR = AbilityRotation.transform;
        Debug.DrawRay(AR.position, AR.up * totalWidth / 2, Color.black, .8f); // char to right corner

        Debug.DrawRay(AR.position, -AR.up * totalWidth / 2, Color.black, .8f); // char to leftcorner

        Debug.DrawRay(AR.position, AR.right * Range / 2, Color.black, .8f); // char to top center

        Debug.DrawRay(AR.right * totalWidth / 2 + AR.position, AR.up * Range, Color.black, .8f); // top center to top right corner

        Debug.DrawRay(AR.right * totalWidth / 2 + AR.position, -AR.up * Range, Color.black, .8f); // top center to top left corner

        Debug.DrawRay(AR.up * totalWidth / 2 + AR.position, AR.right * Range, Color.black, .8f); //right corner to top right corner

        Debug.DrawRay(-AR.up * totalWidth / 2 + AR.position, AR.right * Range, Color.black, .8f); //left corner to top left corner
    }

    private void SpawnHitEffect(LivingBeing targetLivingBeing)
    {
        GameObject instance;

        if (SpawnEffectOnHit != null)
        {
            instance = Instantiate(SpawnEffectOnHit, targetLivingBeing.transform.position, Quaternion.identity, targetLivingBeing.transform.transform);
            //EffectColorChanger.SetImmersiveBleedEffect(instance.GetComponent<ParticleSystem>(), targetLivingBeing);
            Destroy(instance, 3f);
        }

    }
    private void SetEffects(LivingBeing caster, LivingBeing target)
    {
        GameObject particleSystem;
        abilityHandler = caster.GetComponent<AbilityHandler>();
        originTransform = abilityHandler.abilityDirection.transform;

        if (MeleeParticleSystem != null)
        {
            float angle = Mathf.Atan2(-originTransform.transform.up.y, -originTransform.transform.up.x) * Mathf.Rad2Deg;

            if (abilityHandler.WeaponInfo == null)
            {
                particleSystem = Instantiate(MeleeParticleSystem, target.transform.position, Quaternion.identity);
                Destroy(particleSystem, 2); //particleSystem.GetComponent<ParticleSystem>().main.duration);

            }
            else
            {
                particleSystem = Instantiate(abilityHandler.WeaponInfo.animationSplineObject, caster.transform.position, Quaternion.identity);

                SpriteRenderer spriteRenderer = particleSystem.GetComponentInChildren<SpriteRenderer>();
                if (spriteRenderer != null)
                {
                    spriteRenderer.sprite = abilityHandler.WeaponInfo.WeaponSprite;
                }
                else { Debug.Log("There must be an error here"); }
                SplineAnimate animator = abilityHandler.WeaponInfo.animationSplineObject.GetComponentInChildren<SplineAnimate>();

                animator.Play();

                particleSystem.transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle));

                Destroy(particleSystem, animator.Duration);

            }

            if (Shape != AreaOfEffectShape.Sphere)
            {
                particleSystem.transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle));
            }
        }
    }

}

