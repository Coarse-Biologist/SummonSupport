using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using NUnit.Framework.Interfaces;
using UnityEngine.UIElements;
// using UnityEngine.Events;

public class AbilityHandler : MonoBehaviour
{
    [SerializeField] private GameObject iceWallPrefab;
    [SerializeField] private GameObject healingSporesPrefab;
    enum AbilitySlot { Ability1, Ability2, Ability3, Ability4, Ability5, Ability6 }
    PlayerInputActions inputActions;
    Dictionary<AbilitySlot, Ability> abilities;
    Dictionary<string, GameObject> abilityPrefabs;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    #region Init methods
    void Awake()
    {
        inputActions    = new PlayerInputActions();
        abilities       = new Dictionary<AbilitySlot, Ability>();
        abilityPrefabs  = new Dictionary<string, GameObject>();
    }
    void OnEnable()
    {
        inputActions.Player.Ability1.performed += OnAbility1;
        inputActions.Player.Ability2.performed += OnAbility2;
        inputActions.Player.Ability3.performed += OnAbility3;
        // inputActions.Player.Ability4.performed += OnAbility4;
        // inputActions.Player.Ability5.performed += OnAbility5;
        // inputActions.Player.Ability6.performed += OnAbility6;
        inputActions.Player.Enable();
    }
    void OnDisable()
    {
        inputActions.Player.Ability1.performed -= OnAbility1;
        inputActions.Player.Ability2.performed -= OnAbility2;
        inputActions.Player.Ability3.performed += OnAbility3;
        inputActions.Disable();
    }
    void Start()
    {
        abilityPrefabs.Add("Ice Wall",          iceWallPrefab);
        abilityPrefabs.Add("Healing Spores",    healingSporesPrefab);
    }
    #endregion
    #region Select abilities
    public void SelectAbilities()
    {
        abilities.Add(AbilitySlot.Ability1, new Renew());
        abilities.Add(AbilitySlot.Ability2, new IceWall());
        abilities.Add(AbilitySlot.Ability3, new HealingSpores());
    }
    #endregion
    #region Calling abilities
    void OnAbility1(InputAction.CallbackContext context)
    {
        CastAbility(AbilitySlot.Ability1);
    }
    void OnAbility2(InputAction.CallbackContext context)
    {
        CastAbility(AbilitySlot.Ability2);
    }
    void OnAbility3(InputAction.CallbackContext context)
    {
        CastAbility(AbilitySlot.Ability3);
    }
    #endregion

    #region Ability Handling
    void CastAbility(AbilitySlot abilitySlot)
    {
        Ability ability = abilities[abilitySlot];
        HandleTarget(ability);
    }

    void HandleTarget(Ability ability)
    {
        switch (ability.target)
        {
            case AbilityTarget.Self:
                break;
            case AbilityTarget.Mouse:
                Vector3 mousePosition = Input.mousePosition;
                Vector2 worldPos = Camera.main.ScreenToWorldPoint(mousePosition);
                HandleAbilityTypeOnLocation((ConjureAbility)ability, new Vector3(worldPos.x, worldPos.y, 0f));
                break;
            case AbilityTarget.Target:
            case AbilityTarget.Ally:
            case AbilityTarget.Enemy:
            case AbilityTarget.Summon:
                GameObject gameObject = GetGameOjectWithTag(ability.target.ToString());
                if (gameObject != null)
                {
                    HandleAbilityTypeOnTarget(ability, gameObject);
                }
                break;
        }
    }
    GameObject GetGameOjectWithTag(string tag)
    {
        Vector3 mousePosition = Input.mousePosition;
        Vector2 worldPos = Camera.main.ScreenToWorldPoint(mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(worldPos, Vector2.zero); 
        return hit.collider != null && hit.collider.CompareTag(tag)
            ? hit.collider.gameObject
            : null;
    }
    void HandleAbilityTypeOnTarget(Ability ability, GameObject target)
    {   
        switch (ability.type)
        {
            case AbilityType.Heal:
                HealTarget(ability, target);
                break;

            case AbilityType.Damage:
                break;

            case AbilityType.CrowdControl:
                break;
        }
    }

    void HandleAbilityTypeOnLocation(ConjureAbility ability, Vector3 pos)
    {
        switch (ability.type)
        {
            case AbilityType.Summon:
                break;
                
            case AbilityType.Conjure:
                Quaternion rotation = Quaternion.LookRotation(Vector3.forward, transform.up) * Quaternion.Euler(0f, 0f, (float)ability.conjureRotation);
                GameObject prefab;
                if (abilityPrefabs.TryGetValue(ability.name, out prefab))
                {
                    GameObject conjuredObject = Instantiate(prefab, pos, rotation);
                    if (ability.isDecaying)
                    {
                        SelfDestructTimer timer = conjuredObject.GetComponent<SelfDestructTimer>();
                        timer.timeToDestroy = ability.conjureDuration;
                        timer.StartTimer();
                    }
                }
                break;
        }
    }

    void HealTarget(Ability ability, GameObject target)
    {
        StatHandler statHandler = target.GetComponent<StatHandler>();
        if (statHandler != null)
        {
            statHandler.AddHealth(ability.value);
        }
    }
    #endregion
}
