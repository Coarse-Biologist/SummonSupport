using UnityEngine;
using System.Collections.Generic;
using System;

[CreateAssetMenu(menuName = "Abilities/Melee Ability")]

public class MeleeAbility : Ability
{

    [field: SerializeField] public float Range { get; private set; }
    [field: SerializeField] public float Angle { get; private set; }
    [field: SerializeField] public float Width { get; private set; }

    [field: SerializeField] public AreaOfEffectShape Shape { get; private set; }
    [field: SerializeField] public GameObject SpawnEffectOnHit { get; private set; }




    public override bool Activate(GameObject user)
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(user.transform.position, Range * 10, LayerMask.GetMask("Enemy"));
        bool activated = false;
        foreach (Collider2D collider in hits)
        {
            if (VerifyActivate(collider, user))
            {
                Logging.Info("verified and rarified");
                collider.GetComponent<LivingBeing>().ChangeAttribute(Attribute, Value);
                SpawnEffect(collider.GetComponent<LivingBeing>());
                activated = true;
            }
            Logging.Info("oof, return to the check stand for deportation");
        }
        return activated;

    }

    private bool VerifyActivate(Collider2D collider, GameObject user)
    {
        if (collider == null)
            return false;
        if (!IsUsableOn(user.GetComponent<LivingBeing>().CharacterTag, collider.GetComponent<LivingBeing>().CharacterTag))
            return false;
        if (VerifyWithinShape(user, collider))
            return true;
        else
            return false;
    }

    private bool VerifyWithinShape(GameObject user, Collider2D collider)
    {

        Transform originTransform = user.GetComponent<PlayerMovement>().AbilityRotation.transform;
        //DebugAbilityShape(originTransform);
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
            bool isInside = forwardDistance >= 0 && forwardDistance <= Range &&
                            (Mathf.Abs(sideDistance) <= Width / 2f);

            if (isInside)
                return true;

            else return false;

        }
        else return false;
    }


    private void DebugAbilityShape(Transform AbilityRotation)

    {

        Transform AR = AbilityRotation.transform;
        Debug.DrawRay(AR.position, AR.up * Width, Color.black, .8f); // char to right corner

        Debug.DrawRay(AR.position, -AR.up * Width, Color.black, .8f); // char to leftcorner

        Debug.DrawRay(AR.position, AR.right * Range, Color.black, .8f); // char to top center

        Debug.DrawRay(AR.right * Width + AR.position, AR.up * Range, Color.black, .8f); // top center to top right corner

        Debug.DrawRay(AR.right * Width + AR.position, -AR.up * Range, Color.black, .8f); // top center to top left corner

        Debug.DrawRay(AR.up * Width + AR.position, AR.right * Range, Color.black, .8f); //right corner to top right corner

        Debug.DrawRay(-AR.up * Width + AR.position, AR.right * Range, Color.black, .8f); //left corner to top left corner
    }

    private void SpawnEffect(LivingBeing targetLivingBeing)
    {
        GameObject instance;
        if (SpawnEffectOnHit != null)
        {
            instance = Instantiate(SpawnEffectOnHit, targetLivingBeing.transform.position, Quaternion.identity, targetLivingBeing.transform.transform);
            Destroy(instance, 3f);
        }

    }

    // make perpendicular line to players left-to-right line -- check length
    //make perpendicular line to playes bottom to top line -- check length


}



// Vector2 centralLine = originTransform.forward;
// Vector2 perpLine = new Vector2(-centralLine.y, centralLine.x);
// Vector2 PerpLineToHit = hitLocation + centralLine;
// Vector2 nearestPoint = GetIntercept(centralLine, PerpLineToHit);




//Vector2 forwardLine = originTransform.right.normalized;
//Vector2 targetPoint = collider.transform.position;
//float t = Vector2.Dot(targetPoint, forwardLine) / Vector2.Dot(forwardLine, forwardLine);
//Vector2 bottomLine = originTransform.right.normalized;
//float t2 = Vector2.Dot(targetPoint, bottomLine) / Vector2.Dot(forwardLine, forwardLine);


//new Vector2(originTransform.up.x + Width / 2, originTransform.up.y + Width / 2)


// Vector2 AP = hitLocation - (Vector2)originTransform.position;
// float t = Vector2.Dot(AP, originTransform.right) / Vector2.Dot(originTransform.right, originTransform.right);
// Vector2 L = originTransform.position + originTransform.right * t;
// float Distance1 = (hitLocation - L).magnitude;
//
// Vector2 centralHorizontalLine = (Vector2)originTransform.up + new Vector2(originTransform.right.x * Range / 2, originTransform.right.y * Range / 2);
// float t2 = Vector2.Dot(AP, centralHorizontalLine) / Vector2.Dot(centralHorizontalLine, centralHorizontalLine);
// Vector2 L2 = originTransform.position + originTransform.up * t2;
// float Distance2 = (hitLocation - L2).magnitude;

//Vector2 toTarget = (Vector2)hitLocation - (Vector2)originTransform.up * Width + (Vector2)originTransform.position;

//Vector2 offsetOrigin = originTransform.position + originTransform.up * Width;
//Vector2 toTarget = hitLocation - offsetOrigin;
//
//float x1 = Vector2.Dot(toTarget, originTransform.right);
//float y1 = Vector2.Dot(toTarget, originTransform.up);
//double angle1 = Mathf.Atan2(y1, x1) * Mathf.Rad2Deg;
//
//Vector2 toTarget2 = (Vector2)hitLocation - (Vector2)originTransform.right * Range + (Vector2)originTransform.position;
//
//float x2 = Vector2.Dot(toTarget2, originTransform.right);
//float y2 = Vector2.Dot(toTarget2, originTransform.up);
//double angle2 = Mathf.Atan2(y2, x2) * Mathf.Rad2Deg;