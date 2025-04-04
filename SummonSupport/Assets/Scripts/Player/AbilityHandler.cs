using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using UnityEngine.Events;

using NUnit.Framework.Interfaces;
using UnityEngine.UIElements;
using UnityEditor.Callbacks;

public class AbilityHandler : MonoBehaviour
{
    [SerializeField] List<Ability> abilities;
    PlayerInputActions inputActions;
    public UnityEvent<GameObject> hpChanged;


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
        if (register)
        {
            foreach (var action in inputActions.Player.Get().actions)
            {
                if (action.name.StartsWith("Ability"))
                {
                    action.performed += OnAbility;
                }
            }
        }
        else
        {
            inputActions.Player.Ability1.performed -= OnAbility;
            inputActions.Player.Ability2.performed -= OnAbility;
        }
    }
    private void OnAbility(InputAction.CallbackContext context)
    {

        if (abilities.Count > 0)
        {
            CastAbility(abilities[0]);
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

