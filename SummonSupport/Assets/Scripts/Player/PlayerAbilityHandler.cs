using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class PlayerAbilityHandler : AbilityHandler
{
    [SerializeField] List<Ability> abilities;
    PlayerInputActions inputActions;
    private Dictionary<string, int> actionToIndex = new()
    {
        { "Ability1", 0 },
        { "Ability2", 1 },
        { "Ability3", 2 },
        { "Ability4", 3 },
        { "Ability5", 4 },
        { "Ability6", 5 },
        
    };

    private new void Awake()
    {
        base.Awake(); 
        inputActions ??= new PlayerInputActions();
    }

    private void OnEnable()
    {
        RegisterInputEvents(true);
        inputActions.Enable();
    }

    private void OnDisable()
    {
        RegisterInputEvents(false);
        inputActions.Disable();
    }

    private void RegisterInputEvents(bool register)
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
    private void OnAbility(InputAction.CallbackContext context)
    {
        Logging.Verbose($"{gameObject.name} used ability");
        if (actionToIndex.TryGetValue(context.action.name, out int index) && index < abilities.Count)
        {
            Ability ability = abilities[index];
            Logging.Verbose($"{gameObject.name} used ability with index: {index}");
            CastAbility(ability);
        }
    }
}
