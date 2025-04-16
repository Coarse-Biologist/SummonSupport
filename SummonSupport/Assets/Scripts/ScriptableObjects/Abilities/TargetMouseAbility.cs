using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/Target Mouse Ability")]
public class TargetMouseAbility : Ability
{
    [field: Header("Projectile settings")]
    [field: SerializeField] public List<OnEventDo>      ListOnCastDo         { get; protected set; }
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
        foreach (OnEventDo onEventDo in ListOnCastDo)
        {
            switch (onEventDo) //TODO: This has to be a class, just for testing purposes
            {
                case OnEventDo.Damage:
                    Logging.Verbose($"Do Damage to {target.gameObject.name}");
                    if (Attribute != AttributeType.None && Value != 0)
                    {
                        
                    }
                    break;
                case OnEventDo.Heal:
                    Logging.Verbose($"Heal {target.gameObject.name}");
                    if (Attribute != AttributeType.None && Value != 0)
                    {
                        LivingBeing livingBeing = target.GetComponent<LivingBeing>();
                        livingBeing.ChangeAttribute(AttributeType.CurrentHitpoints, Value);
                    }
                    break;
                case OnEventDo.StatusEffect:
                    Logging.Verbose($"Apply {StatusEffect.EffectName} to {target.gameObject.name}");    
                    if (StatusEffect != null)
                        StatusEffect.ApplyStatusEffect(target.gameObject);
                    break;
            }
        }
    }
}