using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using NUnit.Framework.Interfaces;
using UnityEngine.UIElements;
using UnityEditor.Callbacks;
// using UnityEngine.Events;

public class AbilityHandler : MonoBehaviour
{
    [SerializeField] List<Ability> abilities;
    PlayerInputActions inputActions;

    private void Awake()
    {
        inputActions = new PlayerInputActions();
        abilities ??= new List<Ability>();
        
    }
    
    private void OnEnable()
    {
        inputActions.Player.Ability1.performed += OnAbility1;
        inputActions.Player.Ability2.performed += OnAbility2;
        inputActions.Enable();
    }
    
    private void OnDisable()
    {
        inputActions.Player.Ability1.performed -= OnAbility1;
        inputActions.Player.Ability2.performed -= OnAbility2;
        inputActions.Disable();
    }
    
    private void OnAbility1(InputAction.CallbackContext context)
    {
        if (abilities.Count > 0)
        {
            CastAbility(abilities[0]);
        }
    }

    private void OnAbility2(InputAction.CallbackContext context)
    {
        if (abilities.Count > 1)
        {
            CastAbility(abilities[1]);
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
        ability.Activate(gameObject);
    }
}

