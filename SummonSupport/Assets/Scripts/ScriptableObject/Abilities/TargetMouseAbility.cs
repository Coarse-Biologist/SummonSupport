using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[CreateAssetMenu(menuName = "Abilities/Target Mouse Ability")]
public class TargetMouseAbility : Ability
{
    [field: Header("settings")]
    [field: SerializeField] public List<OnEventDo> ListOnCastDo { get; protected set; }
    public override bool Activate(GameObject user)
    {
        bool usedAbility = false;
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        int layerMask = ~LayerMask.GetMask("TriggerOnly"); // Alle außer "TriggerOnly"
        RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero, Mathf.Infinity, layerMask);

        if (hit.collider != null)
        {
            if (hit.collider.TryGetComponent<LivingBeing>(out var target))
            {
                ActivateAbility(target.gameObject, mousePos);
                usedAbility = true;
            }
        }
        return usedAbility;
    }
    public void ActivateAbility(GameObject target, Vector2 mousePos)
    {
        Logging.Info($"Activated Ability {Name} on {target.name}");
        Logging.Verbose($"number of events to do: {ListOnCastDo.Count}");

        foreach (OnEventDo onEventDo in ListOnCastDo)
        {
            Logging.Verbose($"{onEventDo}");
            switch (onEventDo) //TODO: This has to be a class, just for testing purposes
            {
                case OnEventDo.Damage:
                    Logging.Verbose($"Do Damage to {target.gameObject.name}");
                    if (Attribute != AttributeType.None && Value != 0)
                    {

                    }
                    break;
                case OnEventDo.Heal:
                    Logging.Verbose($"Heal {target.gameObject.name}!!!!");
                    if (Attribute != AttributeType.None && Value != 0)
                    {
                        LivingBeing livingBeing = target.GetComponent<LivingBeing>();
                        livingBeing.ChangeAttribute(AttributeType.CurrentHitpoints, Value);
                    }
                    else Logging.Error($"Attribute is : ({Attribute}) and Value = ({Value})");
                    break;
                case OnEventDo.StatusEffect:
                    foreach (StatusEffect statusEffect in StatusEffects)
                    {
                        Logging.Verbose($"Apply {statusEffect.Name} to {target.gameObject.name}");
                        if (statusEffect != null)
                            statusEffect.ApplyStatusEffect(target.gameObject, mousePos);
                    }
                    break;
            }
        }
    }
}