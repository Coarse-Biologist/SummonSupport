using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/Teleport Ability")]
public class TeleportAbility : Ability
{

    [field: SerializeField] public float Range { get; private set; }
    [field: SerializeField] public List<Ability> ActivateOnUse { get; private set; }
    [field: SerializeField] public List<Ability> ActivateOnArrive { get; private set; }


    public override bool Activate(GameObject user)
    {
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);
        GameObject target = null;
        if (hit.collider != null)
        {
            Logging.Info($"there was a hit collider");
            if ((ListUsableOn.Contains(RelationshipType.Friendly) && hit.collider.GetComponent<MinionStats>() != null) || (ListUsableOn.Contains(RelationshipType.Hostile) && hit.collider.GetComponent<EnemyStats>() != null))
            {
                target = hit.collider.gameObject;
                Logging.Info($"using teleport ability inside of teleport activate on {target.name}");
                TeleportToBeing(user, target);
            }

        }
        else Logging.Info($"there was NO hit collider");

        return true;
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
            Logging.Info($"Adding {statusEffect} to {target.name}");
        }

    }


}
