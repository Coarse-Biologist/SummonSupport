using UnityEngine.InputSystem;
using System.Collections.Generic;
using UnityEngine;
using SummonSupportEvents;
using Unity.VisualScripting;

public class PlayerAbilityHandler : AbilityHandler
{
    PlayerInputActions inputActions;
    public static Ability DashAbility { private set; get; }
    private LivingBeing playerStats;
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
        //Logging.Info($"Number of abilities one may potentially like to add: {abilities.Count}");
        foreach (Ability ability in Abilities)
        {
            // Logging.Info($"Slotting {ability} with name {ability.Name} in UI");
            EventDeclarer.SlotChanged.Invoke(index, ability);
            index++;
        }
    }
    void Start()
    {
        UpdateAbilities();
        // abilityRotation = gameObject.GetComponentInChildren<RectTransform>();
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
        RegisterInputEvents(true);
        EventDeclarer.PlayerLearnedAbility?.AddListener(LearnAbility);
        inputActions.Enable();
    }

    void OnDisable()
    {
        RegisterInputEvents(false);
        EventDeclarer.PlayerLearnedAbility?.RemoveListener(LearnAbility);
        inputActions.Disable();
    }

    void RegisterInputEvents(bool register)
    {
        if (inputActions == null) throw new System.Exception("The players input actions is null.");
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
        if (playerStats.Dead) return;

        if (inputActionToIndex.TryGetValue(context.action.name, out int index) && index < Abilities.Count)
        {
            if (!abilitiesOnCooldown[index])
            {

                if (CastAbility(index, GetMousePosition(), transform.rotation))
                    EventDeclarer.AbilityUsed?.Invoke(index);

            }

        }
    }
    Vector2 GetMousePosition()
    {
        return Camera.main.ScreenToWorldPoint(Input.mousePosition);
    }
}
