using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/Target Mouse Ability")]
public class TargetMouseAbility : Ability
{
    //[field: Header("Projectile settings")]

    public override void Activate(GameObject user)
    {
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);

        if (hit.collider != null)
        {
            if (hit.collider.TryGetComponent<LivingBeing>(out var target))
            {
                Debug.Log("Hit LivingBeing: " + target.Name);
                Activate(user, target.gameObject);
            }
        }
    }
    public void Activate(GameObject user, GameObject target)
    {
        Logging.Info($"Activated Ability {Name} on {target.name}");
    }
}