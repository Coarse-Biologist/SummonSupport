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


    new void Start()
    {
        base.Start();
        UpdateAbilities();
        SetAllAbilitySlots();
        //playerStats = GetComponent<LivingBeing>();
        //modHandler = AbilityModHandler.Instance;
        playerStats = PlayerStats.Instance;
        EquipAllPotions();
        //audioHandler = GetComponent<LivingBeingAudioHandler>();
        //Debug.Log($"{audioHandler} = audioHandler");
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


        inputActions.Enable();

        inputActions.Player.Ability1.performed += OnAbility1;
        inputActions.Player.Ability2.performed += OnAbility2;
        inputActions.Player.Ability3.performed += OnAbility3;
        inputActions.Player.AbilityQ.performed += OnAbilityQ;
        inputActions.Player.AbilityE.performed += OnAbilityE;
        inputActions.Player.AbilityF.performed += OnAbilityF;
        inputActions.Player.TogglePauseGame.performed += OnTogglePauseGame;

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
        inputActions.Player.TogglePauseGame.performed -= OnTogglePauseGame;
        inputActions.Player.UseSelectedAbility.performed -= OnSelectedAbility;
        EventDeclarer.SlotChanged?.RemoveListener(ChangeAbilitySlot);


        inputActions.Disable();
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
            EventDeclarer.NewAbilityUI?.Invoke(index, ability);
            index++;
        }
    }
    private void EquipAllPotions()
    {

        foreach (Ability ability in Abilities)
            PotionHandler.SpawnElementalPotionOnBelt(Abilities.IndexOf(ability), ability);

    }
    private void ChangeAbilitySlot(int index, Ability ability)
    {
        foreach (var kvp in SlottedAbilities)
        {
            if (kvp.Value == ability)
            {
                SlottedAbilities[kvp.Key] = null;
                break;
            }
        }
        SlottedAbilities[index] = ability;

        if (Abilities.Contains(ability))
            Abilities[Abilities.IndexOf(ability)] = null;
        while (Abilities.Count < index + 1)
            Abilities.Add(null);
        Abilities[index] = ability;
    }
    private void SetAllAbilitySlots()
    {
        for (int i = 0; i < Abilities.Count && i < 5; i++)
        {
            SlottedAbilities.Add(i, Abilities[i]);
        }
    }
    private bool CheckAbilityUsePossible(Ability selectedAbility)
    {
        if (selectedAbility == null) return false;
        if (IsOnCoolDown(selectedAbility)) return false;
        if (PauseGameHandler.RecentlyPaused) return false;
        if (playerStats.Dead) return false;
        if (playerStats.CurrentPower < selectedAbility.Cost) return false;
        return true;
    }
    private void OnTogglePauseGame(InputAction.CallbackContext context)
    {
        //EventDeclarer.PauseGame?.Invoke();
    }

    #region ability used
    public void OnAbility1(InputAction.CallbackContext context)
    {
        Ability ability = GetAbilityOfIndex(0);

        if (!CheckAbilityUsePossible(ability)) return;

        SetSelectedAbility(ability, 0);

        OnAbility(ability, 0);

    }
    public void OnAbility2(InputAction.CallbackContext context)
    {

        Ability ability = GetAbilityOfIndex(1);

        if (!CheckAbilityUsePossible(ability)) return;

        SetSelectedAbility(ability, 1);
        OnAbility(ability, 1);

    }
    public void OnAbility3(InputAction.CallbackContext context)
    {

        Ability ability = GetAbilityOfIndex(2);

        if (!CheckAbilityUsePossible(ability)) return;

        SetSelectedAbility(ability, 2);


        OnAbility(ability, 2);

    }
    public void OnAbilityQ(InputAction.CallbackContext context)
    {

        Ability ability = GetAbilityOfIndex(3);
        SetSelectedAbility(ability, 3);

        if (!CheckAbilityUsePossible(ability)) return;

        OnAbility(ability, 3);

    }
    public void OnAbilityE(InputAction.CallbackContext context)
    {

        Ability ability = GetAbilityOfIndex(4);

        SetSelectedAbility(ability, 4);

        if (!CheckAbilityUsePossible(ability)) return;

        OnAbility(ability, 4);

    }
    public void OnAbilityF(InputAction.CallbackContext context)
    {

        Ability ability = GetAbilityOfIndex(5);

        SetSelectedAbility(ability, 5);
        if (!CheckAbilityUsePossible(ability)) return;

        OnAbility(ability, 5);


    }
    public void OnSelectedAbility(InputAction.CallbackContext context)
    {
        if (!CheckAbilityUsePossible(selectedAbility)) return;

        OnAbility(selectedAbility, currentAbilityIndex);
    }

    private void OnAbility(Ability ability, int index)
    {
        CastAbility(ability, GetCenterOfScreen(100f), transform.rotation);
        EventDeclarer.AbilityUsed?.Invoke(index);
    }

    #endregion

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
        bool onCooldown = abilitiesOnCooldown[ability];
        return onCooldown;
    }
}
