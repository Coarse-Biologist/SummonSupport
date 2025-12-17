using UnityEngine.InputSystem;
using System.Collections.Generic;
using UnityEngine;
using SummonSupportEvents;

public class PlayerAbilityHandler : AbilityHandler
{
    PlayerInputActions inputActions;
    public static Ability DashAbility { private set; get; }
    public Ability selectedAbility { private set; get; }
    private LivingBeing playerStats;
    public AnimationControllerScript anim { get; private set; }
    public string currentAnimation;
    private int currentAbilityIndex;
    private Dictionary<string, int> inputActionToIndex = new()
    {
        { "Ability1", 0 },
        { "Ability2", 1 },
        { "Ability3", 2 },
        { "Ability4", 3 },
        { "Ability5", 4 },
        { "Ability6", 5 },
    };

    public static void SetDashAbility(DashAbility ability)
    {
        Debug.Log("Setting dash ability");
        DashAbility = ability;
    }
    public void UpdateAbilities()
    {
        int index = 0;
        foreach (Ability ability in Abilities)
        {
            EventDeclarer.SlotChanged.Invoke(index, ability);
            index++;
        }
    }
    void Start()
    {
        UpdateAbilities();
        playerStats = GetComponent<LivingBeing>();

    }

    new void Awake()
    {
        base.Awake();
        inputActions ??= new PlayerInputActions();

    }

    void OnEnable()
    {
        inputActions ??= new PlayerInputActions();
        //RegisterInputEvents(true);
        EventDeclarer.PlayerLearnedAbility?.AddListener(LearnAbility);
        inputActions.Enable();

        inputActions.Player.Ability1.performed += OnAbility1;
        inputActions.Player.Ability2.performed += OnAbility2;
        inputActions.Player.Ability3.performed += OnAbility3;
        inputActions.Player.AbilityQ.performed += OnAbilityQ;
        inputActions.Player.AbilityE.performed += OnAbilityE;
        inputActions.Player.AbilityF.performed += OnAbilityF;
        inputActions.Player.UseSelectedAbility.performed += OnSelectedAbility;

    }

    void OnDisable()
    {
        //RegisterInputEvents(false);
        EventDeclarer.PlayerLearnedAbility?.RemoveListener(LearnAbility);

        inputActions.Player.Ability1.performed -= OnAbility1;
        inputActions.Player.Ability2.performed -= OnAbility2;
        inputActions.Player.Ability3.performed -= OnAbility3;
        inputActions.Player.AbilityQ.performed -= OnAbilityQ;
        inputActions.Player.AbilityE.performed -= OnAbilityE;
        inputActions.Player.AbilityF.performed -= OnAbilityF;
        inputActions.Player.UseSelectedAbility.performed -= OnSelectedAbility;

        inputActions.Disable();
    }

    #region ability used
    public void OnAbility1(InputAction.CallbackContext context)
    {
        if (playerStats.Dead) return;

        Ability ability = GetAbilityOfIndex(0);

        if (ability == null) return;

        SetSelectedAbility(ability, 0);

        if (!abilitiesOnCooldownCrew[ability]) // Checks if the value is true. Abilities[index] is the ability at the InputActions context index.
        {
            CastAbility(ability, GetCenterOfScreen(100f), transform.rotation);
            EventDeclarer.AbilityUsed?.Invoke(0);

        }
    }
    public void OnAbility2(InputAction.CallbackContext context)
    {
        if (playerStats.Dead) return;

        Ability ability = GetAbilityOfIndex(1);

        if (ability == null) return;

        SetSelectedAbility(ability, 1);

        if (!abilitiesOnCooldownCrew[ability]) // Checks if the value is true. Abilities[index] is the ability at the InputActions context index.
        {
            CastAbility(ability, GetCenterOfScreen(100f), transform.rotation);
            EventDeclarer.AbilityUsed?.Invoke(1);

        }
    }
    public void OnAbility3(InputAction.CallbackContext context)
    {
        if (playerStats.Dead) return;

        Ability ability = GetAbilityOfIndex(2);

        if (ability == null) return;

        SetSelectedAbility(ability, 2);

        if (!abilitiesOnCooldownCrew[ability]) // Checks if the value is true. Abilities[index] is the ability at the InputActions context index.
        {
            CastAbility(ability, GetCenterOfScreen(100f), transform.rotation);
            EventDeclarer.AbilityUsed?.Invoke(2);

        }
    }
    public void OnAbilityQ(InputAction.CallbackContext context)
    {
        if (playerStats.Dead) return;

        Ability ability = GetAbilityOfIndex(3);
        if (ability == null) return;

        SetSelectedAbility(ability, 3);

        if (!abilitiesOnCooldownCrew[ability]) // Checks if the value is true. Abilities[index] is the ability at the InputActions context index.
        {
            CastAbility(ability, GetCenterOfScreen(100f), transform.rotation);
            EventDeclarer.AbilityUsed?.Invoke(3);

        }
    }
    public void OnAbilityE(InputAction.CallbackContext context)
    {
        if (playerStats.Dead) return;

        Ability ability = GetAbilityOfIndex(4);

        if (ability == null) return;

        SetSelectedAbility(ability, 4);

        if (!abilitiesOnCooldownCrew[ability]) // Checks if the value is true. Abilities[index] is the ability at the InputActions context index.
        {
            CastAbility(ability, GetCenterOfScreen(100f), transform.rotation);
            EventDeclarer.AbilityUsed?.Invoke(4);
        }
    }
    public void OnAbilityF(InputAction.CallbackContext context)
    {
        if (playerStats.Dead) return;

        Ability ability = GetAbilityOfIndex(5);

        if (ability == null) return;

        SetSelectedAbility(ability, 5);

        if (!abilitiesOnCooldownCrew[ability]) // Checks if the value is true. Abilities[index] is the ability at the InputActions context index.
        {
            CastAbility(ability, GetCenterOfScreen(100f), transform.rotation);
            EventDeclarer.AbilityUsed?.Invoke(5);

        }
    }

    #endregion
    public void OnSelectedAbility(InputAction.CallbackContext context)
    {
        if (selectedAbility == null || playerStats.Dead) return;

        if (!abilitiesOnCooldownCrew[selectedAbility]) // Checks if the value is true. Abilities[index] is the ability at the InputActions context index.
        {
            CastAbility(selectedAbility, GetCenterOfScreen(100f), transform.rotation);
            EventDeclarer.AbilityUsed?.Invoke(currentAbilityIndex);
        }
    }
    private Ability GetAbilityOfIndex(int index)
    {
        if (Abilities.Count - 1 < index) return null; // index 3 requires count of 4
        else return Abilities[index];
    }

    private void SetSelectedAbility(Ability ability, int index)
    {
        currentAbilityIndex = index;
        if (ability != null)
            selectedAbility = ability;
    }

    public Vector3 GetCenterOfScreen(float distance)
    {
        Vector3 screenCenter = new Vector3(
                Screen.width * 0.5f,
                Screen.height * 0.5f,
                distance
            );

        return Camera.main.ScreenToWorldPoint(screenCenter);
    }
}
