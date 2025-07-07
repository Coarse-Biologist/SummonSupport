using UnityEngine;
using System.Collections.Generic;
[CreateAssetMenu(menuName = "Abilities/Melee Ability")]

public class MeleeAbility : Ability
{

    [field: SerializeField] public float Range { get; private set; }
    [field: SerializeField] public AreaOfEffectShape Shape { get; private set; }




    public override bool Activate(GameObject user)
    {
        List<GameObject> targets = new();
        Collider2D[] hits = Physics2D.OverlapCircleAll(user.transform.position, Range, LayerMask.GetMask("Enemy"));

        foreach (Collider2D collider in hits)
            if (collider != null)
            {
                Debug.Log("Hit: " + collider.name + "with" + this);
            }
        return true;
    }
}


