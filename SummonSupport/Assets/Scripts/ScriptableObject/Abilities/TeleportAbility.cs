using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/Teleport Ability")]
public class TeleportAbility : Ability
{

    [field: SerializeField] public float Range { get; private set; }
    [field: SerializeField] public List<Ability> ActivateOnUse { get; private set; }
    [field: SerializeField] public List<Ability> ActivateOnArrive { get; private set; }


    public bool Activate(GameObject user, Vector2 targetLocation)
    {
        //Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(targetLocation, Vector2.zero);
        GameObject target = null;
        if (hit.collider != null && hit.collider.gameObject.GetComponent<LivingBeing>() != null)
        {
            target = hit.collider.gameObject;

            //Logging.Info($"there was a hit collider");
            if (IsUsableOn(user.GetComponent<LivingBeing>().CharacterTag, target.GetComponent<LivingBeing>().CharacterTag))
            {
                TeleportToBeing(user, target);
                return true;
            }
            // else Logging.Info($"the ability wasnt useable on sucha  being");

        }
        //else Logging.Info($"there was NO hit collider or it wasnt a living being at location {hit.point}");

        return false;
    }

    public override bool Activate(GameObject user)
    {
        throw new System.NotImplementedException();
    }

    private void TeleportToBeing(GameObject user, GameObject target)
    {
        user.transform.position = target.transform.position;

        foreach (Ability ability in ActivateOnArrive)
        {
            ability.Activate(user);
        }
        foreach (StatusEffect statusEffect in StatusEffects)
        {
            statusEffect.ApplyStatusEffect(target.GetComponent<LivingBeing>());
            //Logging.Info($"Adding {statusEffect} to {target.name}");
        }

    }


}
