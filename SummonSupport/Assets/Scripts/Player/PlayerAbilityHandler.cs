using UnityEngine.InputSystem;
using System.Collections.Generic;
using UnityEngine;
using SummonSupportEvents;

public class PlayerAbilityHandler : AbilityHandler
{
    PlayerInputActions inputActions;
    private Dictionary<string, int> inputActionToIndex = new()
    {
        { "Ability1", 0 },
        { "Ability2", 1 },
        { "Ability3", 2 },
        { "Ability4", 3 },
        { "Ability5", 4 },
        { "Ability6", 5 },
    };

    public void UpdateAbilities()
    {
        int index = 0;
        foreach (Ability ability in abilities)
        {
            EventDeclarer.SlotChanged.Invoke(index, ability);
            index++;
        }
    }
    void Start()
    {
        UpdateAbilities();
        // abilityRotation = gameObject.GetComponentInChildren<RectTransform>();
    }

    new void Awake()
    {
        base.Awake();
        inputActions ??= new PlayerInputActions();
    }

    void OnEnable()
    {
        RegisterInputEvents(true);
        inputActions.Enable();
    }

    void OnDisable()
    {
        RegisterInputEvents(false);
        inputActions.Disable();
    }

    void RegisterInputEvents(bool register)
    {
        foreach (var action in inputActions.Player.Get().actions)
        {
            if (action.name.StartsWith("Ability"))
            {
                if (register)
                    action.performed += OnAbility;
                else
                    action.performed -= OnAbility;
            }
        }
    }
    void OnAbility(InputAction.CallbackContext context)
    {
        if (inputActionToIndex.TryGetValue(context.action.name, out int index) && index < abilities.Count)
        {
            if (!abilitiesOnCooldown[index])
            {
                if (CastAbility(index, GetMousePosition(), abilityDirection.transform.rotation))
                    EventDeclarer.AbilityUsed?.Invoke(index);

            }
                
        }
    }
    Vector2 GetMousePosition()
    {
        return Camera.main.ScreenToWorldPoint(Input.mousePosition);
    }
}
