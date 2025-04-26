using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[CreateAssetMenu(menuName = "Abilities/Target Mouse Ability")]
public class TargetMouseAbility : Ability
{
    [field: Header("settings")]
    [field: SerializeField] public List<OnEventDo> ListOnCastDo { get; protected set; }
    public override void Activate(GameObject user)
    {
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);

        if (hit.collider != null)
        {
            if (hit.collider.TryGetComponent<LivingBeing>(out var target))
            {
                Debug.Log("Hit LivingBeing: " + target.Name);
                ActivateAbility(target.gameObject, mousePos);
            }
            else Logging.Verbose($"{hit.collider.name} does not have the component living being!!!!");
        }
        else Logging.Verbose("Collider was null");
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
                // case OnEventDo.CC:
                    // AI_CC_State ccState = target.GetComponent<AI_CC_State>();
                    // Logging.Info($"A CC ability has hit the target {target.name}");
                    // if(ccState != null) ccState.RecieveCC(StatusEffectType.KnockInTheAir, user);
                    // break;
            }
        }
    }
}