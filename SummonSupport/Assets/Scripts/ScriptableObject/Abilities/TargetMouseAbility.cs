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
        int layerMask = LayerMask.GetMask("Minion", "Player", "Enemy");
        RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero, Mathf.Infinity, layerMask);

        if (hit.collider != null)
            if (hit.collider.TryGetComponent<LivingBeing>(out var target))
                usedAbility = ActivateAbility(user, target, mousePos);

        return usedAbility;
    }
    public bool ActivateAbility(GameObject user, LivingBeing targetLivingBeing, Vector2 mousePos)
    {
        if (user.TryGetComponent<LivingBeing>(out var userLivingBeing))
            if (!IsUsableOn(userLivingBeing.CharacterTag, targetLivingBeing.CharacterTag))
                return false;

        foreach (OnEventDo onEventDo in ListOnCastDo)
        {
            HandleEventType(onEventDo, targetLivingBeing, mousePos);
        }
        return true;
    }

    void HandleEventType(OnEventDo onEventDo, LivingBeing targetLivingBeing, Vector2 mousePos)
    {
        switch (onEventDo) //TODO: This has to be a class, just for testing purposes
        {
            case OnEventDo.Damage:
                if (Attribute != AttributeType.None && Value != 0)
                    targetLivingBeing.ChangeAttribute(AttributeType.CurrentHitpoints, -Value);
                break;
            case OnEventDo.Heal:
                if (Attribute != AttributeType.None && Value != 0)
                    targetLivingBeing.ChangeAttribute(AttributeType.CurrentHitpoints, Value);
                break;
            case OnEventDo.StatusEffect:
                foreach (StatusEffect statusEffect in StatusEffects)
                {
                    if (statusEffect != null)
                        statusEffect.ApplyStatusEffect(targetLivingBeing.gameObject, mousePos);
                }
                break;
        }
    }
}