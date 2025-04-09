using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using UnityEngine.Events;
public class AbilityHandler : MonoBehaviour
{
    [SerializeField] List<Ability> abilities;
    PlayerInputActions inputActions;
    public UnityEvent<GameObject> hpChanged;
    private Dictionary<string, int> actionToIndex = new()
    {
        { "Ability1", 0 },
        { "Ability2", 1 },
        { "Ability3", 2 },
        { "Ability4", 3 },
        { "Ability5", 4 },
        { "Ability6", 5 },
        
    };

    private void Awake()
    {
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
        if (actionToIndex.TryGetValue(context.action.name, out int index) && index < abilities.Count)
        {
            CastAbility(abilities[index]);
        }
    }
    void CastAbility(Ability ability)
    {
        switch (ability)
        {
            case ProjectileAbility projectile:
                Logging.Info("Cast Ability is a projectile: " + abilities[0].name);
                HandleProjectile(projectile);
                break;

            case ConjureAbility conjuration:
                break;
        }
    }
    void HandleProjectile(ProjectileAbility ability)
    {
        ability.Activate(gameObject, gameObject);
    }
}

