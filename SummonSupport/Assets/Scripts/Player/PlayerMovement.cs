using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Entities;
using Unity.Collections;
using SummonSupportEvents;
using Unity.Mathematics;


public class PlayerMovement : MonoBehaviour
{
    #region class Variables
    private Vector2 moveInput;
    public Camera mainCamera;
    private CreatureSpriteController spriteController;
    [SerializeField] private PlayerInputActions inputActions;
    [SerializeField] public GameObject AbilityRotation;
    private Vector2 lookInput;
    private Vector2 worldPosition;
    private Vector2 direction;
    Vector3 moveDirection;
    [field: SerializeField] public GameObject DashDust { private set; get; }

    [SerializeField] float movementSpeed;
    [SerializeField] float dashBoost = 10f;
    [SerializeField] float dashCoolDown = 1f;
    [SerializeField] float dashDuration = .1f;
    private Rigidbody2D rb;
    private bool dashing = false;
    private bool canDash = true;
    private bool lockedInUI = false;

    #endregion

    private void Awake()
    {
        spriteController = GetComponentInChildren<CreatureSpriteController>();
        rb = GetComponent<Rigidbody2D>();
        mainCamera = Camera.main;
        inputActions = new PlayerInputActions();

    }

    void Start()
    {
        movementSpeed = gameObject.GetComponent<PlayerStats>().Speed;
    }
    #region Enable and Disable event subscriptions
    private void OnEnable()
    {
        //AlchemyBenchUI.Instance.playerUsingUI.AddListener(ToggleLockedInUI);
        inputActions ??= new PlayerInputActions();
        EventDeclarer.SpeedAttributeChanged.AddListener(SetMovementAttribute);
        inputActions.Player.Enable();
        inputActions.Player.Move.performed += OnMove;
        inputActions.Player.Move.canceled += OnMove;
        inputActions.Player.Dash.performed += OnDash;
        inputActions.Player.LookDirection.performed += OnLook;
        inputActions.Player.CommandMinion.performed += SendMinionCommandContext;
    }

    private void OnDisable()
    {
        //AlchemyBenchUI.Instance.playerUsingUI.RemoveListener(ToggleLockedInUI);

        EventDeclarer.SpeedAttributeChanged.RemoveListener(SetMovementAttribute);
        inputActions.Player.Move.performed -= OnMove;
        inputActions.Player.Move.canceled -= OnMove;
        inputActions.Player.Dash.performed -= OnDash;
        inputActions.Player.LookDirection.performed -= OnLook;
        inputActions.Player.CommandMinion.performed -= SendMinionCommandContext;
        inputActions.Player.Disable();
    }
    #endregion

    #region Dash logic
    private void OnDash(InputAction.CallbackContext context)
    {
        EventDeclarer.SpawnEnemies?.Invoke(this.gameObject);
        if (canDash)
        {
            canDash = false;
            dashing = true;
            Invoke("ReturnToNormalSpeed", dashDuration);
            Invoke("ReadyDash", dashCoolDown);
            GameObject DashDustInstance = Instantiate(DashDust, transform.position, Quaternion.identity, transform);
            float angle = Mathf.Atan2(-moveDirection.y, -moveDirection.x) * Mathf.Rad2Deg;
            DashDustInstance.transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle));
            Destroy(DashDustInstance, dashDuration);
        }
    }

    private void ReadyDash()
    {
        canDash = true;
    }
    private void ReturnToNormalSpeed()
    {
        dashing = false;
    }
    #endregion

    #region WASD logic
    private void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }
    private void HandleMove()
    {
        float calculatedSpeed;
        if (dashing)
            calculatedSpeed = movementSpeed + dashBoost;
        else
            calculatedSpeed = movementSpeed;

        moveDirection = new Vector3(moveInput.x, moveInput.y, 0).normalized;
        rb.linearVelocity = moveDirection * calculatedSpeed * 10;
    }


    #endregion

    #region Look Direction
    public void OnLook(InputAction.CallbackContext context)
    {
        lookInput = context.ReadValue<Vector2>();
    }
    private Quaternion HandleLook()
    {
        worldPosition = mainCamera.ScreenToWorldPoint(lookInput);
        direction = (worldPosition - (Vector2)transform.position).normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        AbilityRotation.transform.rotation = Quaternion.Euler(0, 0, angle);
        spriteController.SetSpriteDirection(angle);

        //DebugAbilityShape();

        return transform.rotation;
    }
    private void DebugAbilityShape()
    {
        Transform AR = AbilityRotation.transform;
        Debug.DrawRay(AR.position, AR.up * 2, Color.black, .1f);

        Debug.DrawRay(AR.position, -AR.up * 2, Color.black, .1f);

        Debug.DrawRay(AR.position, AR.right * 2, Color.black, .1f);

        Debug.DrawRay(AR.right * 2 + AR.position, AR.up * 2, Color.black, .1f);

        Debug.DrawRay(AR.right * 2 + AR.position, -AR.up * 2, Color.black, .1f);
        Debug.DrawRay(AR.up * 2 + AR.position, AR.right * 2, Color.black, .1f);

        Debug.DrawRay(-AR.up * 2 + AR.position, AR.right * 2, Color.black, .1f);
    }

    #endregion

    private void SendMinionCommandContext(InputAction.CallbackContext context)
    {
        worldPosition = mainCamera.ScreenToWorldPoint(lookInput);
        Debug.DrawLine(new Vector3(0, 0, 0), worldPosition, Color.green);
        CommandMinion.HandleCommand(worldPosition);


    }

    private void Update()
    {
        if (!lockedInUI)
        {
            HandleMove();

            HandleLook();
        }
    }

    private void UpdatePositionForEntities()
    {
        EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        EntityQuery entityQuery = new EntityQueryBuilder(Allocator.Temp).WithAll<SeesTargetComponent>().Build(entityManager);
        NativeArray<Entity> entityArray = entityQuery.ToEntityArray(Allocator.Temp);
        NativeArray<SeesTargetComponent> seesTargetCompArray = entityQuery.ToComponentDataArray<SeesTargetComponent>(Allocator.Temp);
        for (int i = 0; i < seesTargetCompArray.Length; i++)
        {
            SeesTargetComponent seesTargetComponent = seesTargetCompArray[i];
            seesTargetComponent.targetLocation = transform.position;
            seesTargetCompArray[i] = seesTargetComponent;
        }
        entityQuery.CopyFromComponentDataArray(seesTargetCompArray);
    }

    private void SetMovementAttribute(AttributeType attribute, float newValue)
    {
        switch (attribute)
        {
            case AttributeType.MovementSpeed:
                movementSpeed = newValue;
                break;
            case AttributeType.DashBoost:
                dashBoost = newValue;
                break;
            case AttributeType.DashCooldown:
                dashCoolDown = newValue;
                break;
            case AttributeType.DashDuration:
                dashDuration = newValue;
                break;
        }
    }
}
