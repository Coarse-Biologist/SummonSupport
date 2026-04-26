using UnityEngine.InputSystem;
using System.Collections.Generic;
using UnityEngine;
using SummonSupportEvents;
using Unity.VisualScripting;

public class PlayerAbilityHandler : AbilityHandler
{
    public PlayerAbilityHandler Instance { private set; get; }
    PlayerInputActions inputActions;
    public static Ability DashAbility { private set; get; }
    public Ability selectedAbility { private set; get; }
    private LivingBeing playerStats;
    public AnimationControllerScript anim { get; private set; }
    public string currentAnimation;
    private int currentAbilityIndex;
    private bool paused = false;
    private Dictionary<string, int> inputActionToIndex = new()
    {
        { "Ability1", 0 },
        { "Ability2", 1 },
        { "Ability3", 2 },
        { "Ability4", 3 },
        { "Ability5", 4 },
        { "Ability6", 5 },
    };


    void Start()
    {
        UpdateAbilities();
        playerStats = GetComponent<LivingBeing>();
        modHandler = AbilityModHandler.Instance;
        if (Instance != null) Destroy(this);
        else Instance = this;

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
        EventDeclarer.UnpauseGame?.AddListener(UnpauseGame);
        EventDeclarer.PauseGame?.AddListener(PauseGame);


        inputActions.Enable();

        inputActions.Player.Ability1.performed += OnAbility1;
        inputActions.Player.Ability2.performed += OnAbility2;
        inputActions.Player.Ability3.performed += OnAbility3;
        inputActions.Player.AbilityQ.performed += OnAbilityQ;
        inputActions.Player.AbilityE.performed += OnAbilityE;
        inputActions.Player.AbilityF.performed += OnAbilityF;
        inputActions.Player.UseSelectedAbility.performed += OnSelectedAbility;
        EventDeclarer.SlotChanged?.AddListener(ChangeAbilitySlot);

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
        EventDeclarer.SlotChanged?.RemoveListener(ChangeAbilitySlot);
        EventDeclarer.PauseGame?.RemoveListener(PauseGame);
        EventDeclarer.UnpauseGame?.AddListener(UnpauseGame);


        inputActions.Disable();
    }
    private void PauseGame()
    {
        paused = true;
    }
    private void UnpauseGame()
    {
        paused = false;
    }
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
            if (ability == null) continue;
            EventDeclarer.SetSlot?.Invoke(index, ability);
            index++;
        }
    }
    private void ChangeAbilitySlot(int index, Ability ability)
    {
        if (Abilities.Contains(ability))
            Abilities[Abilities.IndexOf(ability)] = null;
        while (Abilities.Count < index + 1)
            Abilities.Add(null);
        Abilities[index] = ability;
    }

    #region ability used
    public void OnAbility1(InputAction.CallbackContext context)
    {
        if (playerStats.Dead || paused) return;

        Ability ability = GetAbilityOfIndex(0);

        if (ability == null) return;

        SetSelectedAbility(ability, 0);

        if (!IsOnCoolDown(ability)) // Checks if the value is true. Abilities[index] is the ability at the InputActions context index.
        {
            CastAbility(ability, GetCenterOfScreen(100f), transform.rotation);
            EventDeclarer.AbilityUsed?.Invoke(0);

        }
    }
    public void OnAbility2(InputAction.CallbackContext context)
    {
        if (playerStats.Dead || paused) return;

        Ability ability = GetAbilityOfIndex(1);

        if (ability == null) return;

        SetSelectedAbility(ability, 1);

        if (!IsOnCoolDown(ability)) // Checks if the value is true. Abilities[index] is the ability at the InputActions context index.
        {
            CastAbility(ability, GetCenterOfScreen(100f), transform.rotation);
            EventDeclarer.AbilityUsed?.Invoke(1);
        }
    }
    public void OnAbility3(InputAction.CallbackContext context)
    {
        if (playerStats.Dead || paused) return;

        Ability ability = GetAbilityOfIndex(2);

        if (ability == null) return;

        SetSelectedAbility(ability, 2);

        if (!IsOnCoolDown(ability)) // Checks if the value is true. Abilities[index] is the ability at the InputActions context index.
        {
            CastAbility(ability, GetCenterOfScreen(100f), transform.rotation);
            EventDeclarer.AbilityUsed?.Invoke(2);

        }
    }
    public void OnAbilityQ(InputAction.CallbackContext context)
    {
        if (playerStats.Dead || paused) return;

        Ability ability = GetAbilityOfIndex(3);
        if (ability == null) return;

        SetSelectedAbility(ability, 3);

        if (!IsOnCoolDown(ability)) // Checks if the value is true. Abilities[index] is the ability at the InputActions context index.
        {
            CastAbility(ability, GetCenterOfScreen(100f), transform.rotation);
            EventDeclarer.AbilityUsed?.Invoke(3);

        }
    }
    public void OnAbilityE(InputAction.CallbackContext context)
    {
        if (playerStats.Dead || paused) return;

        Ability ability = GetAbilityOfIndex(4);

        if (ability == null) return;

        SetSelectedAbility(ability, 4);

        if (!IsOnCoolDown(ability)) // Checks if the value is true. Abilities[index] is the ability at the InputActions context index.
        {
            CastAbility(ability, GetCenterOfScreen(100f), transform.rotation);
            EventDeclarer.AbilityUsed?.Invoke(4);
        }
    }
    public void OnAbilityF(InputAction.CallbackContext context)
    {
        if (playerStats.Dead || paused) return;

        Ability ability = GetAbilityOfIndex(5);

        if (ability == null) return;

        SetSelectedAbility(ability, 5);

        if (!IsOnCoolDown(ability)) // Checks if the value is true. Abilities[index] is the ability at the InputActions context index.
        {
            CastAbility(ability, GetCenterOfScreen(100f), transform.rotation);
            EventDeclarer.AbilityUsed?.Invoke(5);

        }
    }

    #endregion
    public void OnSelectedAbility(InputAction.CallbackContext context)
    {
        if (selectedAbility == null || playerStats.Dead || paused) return;

        if (!IsOnCoolDown(selectedAbility)) // Checks if the value is true. Abilities[index] is the ability at the InputActions context index.
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
    protected override bool IsOnCoolDown(Ability ability)
    {
        if (ability is BeamAbility beamAbility)
        {
            if (toggledAbilitiesDict.ContainsKey(beamAbility))
                return false;
        }
        bool onCooldown = abilitiesOnCooldownCrew[ability];
        return onCooldown;
    }
}
