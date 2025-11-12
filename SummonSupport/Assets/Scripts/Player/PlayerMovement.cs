using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Entities;
using Unity.Collections;
using SummonSupportEvents;
using Unity.Mathematics;
using UnityEngine.SceneManagement;


public class PlayerMovement : MovementScript
{
    #region class Variables
    private Vector2 moveInput;
    public Camera mainCamera;
    private LivingBeing playerStats;
    [SerializeField] private PlayerInputActions inputActions;
    [SerializeField] public GameObject AbilityRotation;
    [field: SerializeField] public bool CameraFreeRotate = false;
    private Vector2 lookInput;
    private Vector2 worldPosition;
    private Vector2 direction;
    Vector3 moveDirection;
    [field: SerializeField] public GameObject DashDust { private set; get; }

    private Rigidbody rb;
    private bool dashing = false;
    private bool canDash = true;
    GameObject DashDustInstance = null;
    private bool lockedInUI = false;
    private bool paused = false;
    private bool StuckInAbilityAnimation = false;


    #endregion

    private void Awake()
    {
        Cursor.lockState = CursorLockMode.Locked;   // Locks the cursor to the center of the screen
        Cursor.visible = false;
        rb = GetComponent<Rigidbody>();
        mainCamera = Camera.main;
        inputActions = new PlayerInputActions();
        playerStats = GetComponent<LivingBeing>();

    }

    #region Enable and Disable event subscriptions
    private void OnEnable()
    {
        //AlchemyBenchUI.Instance.playerUsingUI.AddListener(ToggleLockedInUI);
        inputActions ??= new PlayerInputActions();
        EventDeclarer.SpeedAttributeChanged?.AddListener(SetMovementAttribute);
        inputActions.Player.Enable();
        inputActions.Player.Move.performed += OnWASD;
        inputActions.Player.Move.canceled += OnWASD;
        inputActions.Player.Dash.performed += OnUseDash;
        inputActions.Player.LookDirection.performed += OnLook;
        inputActions.Player.CommandMinion.performed += SendMinionCommandContext;
        inputActions.Player.TogglePauseGame.performed += ToggleGamePause;

    }

    private void OnDisable()
    {
        //AlchemyBenchUI.Instance.playerUsingUI.RemoveListener(ToggleLockedInUI);

        EventDeclarer.SpeedAttributeChanged?.RemoveListener(SetMovementAttribute);
        inputActions.Player.Move.performed -= OnWASD;
        inputActions.Player.Move.canceled -= OnWASD;
        inputActions.Player.Dash.performed -= OnUseDash;
        inputActions.Player.LookDirection.performed -= OnLook;
        inputActions.Player.CommandMinion.performed -= SendMinionCommandContext;
        inputActions.Player.TogglePauseGame.performed -= ToggleGamePause;

        inputActions.Player.Disable();
    }
    #endregion

    #region Dash logic
    private void OnUseDash(InputAction.CallbackContext context)
    {
        if (playerStats.Dead) return;

        if (canDash)
        {
            canDash = false;
            dashing = true;
            Invoke("ReturnToNormalSpeed", DashDuration);
            Invoke("ReadyDash", DashCoolDown);
            if (PlayerAbilityHandler.DashAbility != null) PlayerAbilityHandler.DashAbility.Activate(gameObject);
            else
            {
                DashDustInstance = Instantiate(DashDust, transform.position, Quaternion.identity, transform);
                float angle = Mathf.Atan2(-moveDirection.y, -moveDirection.x) * Mathf.Rad2Deg;
                DashDustInstance.transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle));
                Destroy(DashDustInstance, DashDuration);
            }
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

    public void SetStuckInAbilityAnimation(bool stuck)
    {
        StuckInAbilityAnimation = stuck;
    }
    #endregion

    #region WASD logic
    private void OnWASD(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }
    private void HandleMove()
    {
        float calculatedSpeed;
        if (dashing)
            calculatedSpeed = MovementSpeed + DashBoost;
        else
            calculatedSpeed = MovementSpeed;

        Vector3 moveDirection = transform.TransformDirection(new Vector3(moveInput.x, 0, moveInput.y)).normalized;

        rb.AddForce(moveDirection * calculatedSpeed * 100f, ForceMode.Acceleration);
    }


    #endregion

    #region Look Direction
    public void OnLook(InputAction.CallbackContext context)
    {
        lookInput = context.ReadValue<Vector2>();
    }
    private void HandleLook()
    {
        transform.rotation = Quaternion.Euler(0f, mainCamera.transform.eulerAngles.y, 0f);
    }

    #endregion

    private void SendMinionCommandContext(InputAction.CallbackContext context)
    {
        if (playerStats.Dead) return;

        worldPosition = mainCamera.ScreenToWorldPoint(lookInput);
        Debug.DrawLine(new Vector3(0, 0, 0), worldPosition, Color.green);
        CommandMinion.HandleCommand(worldPosition);
    }

    private void FixedUpdate()
    {
        if (playerStats.Dead) return;

        if (!lockedInUI && !StuckInAbilityAnimation)
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

    private void ToggleGamePause(InputAction.CallbackContext context)
    {
        if (!lockedInUI)
        {
            paused = !paused;
            EventDeclarer.TogglePauseGame?.Invoke();

            if (paused)
            {
                Debug.Log("Pausing");
                Time.timeScale = 0f;
            }
            else
            {
                Debug.Log("Unpausing");
                Time.timeScale = 1f;
            }
        }
    }

    //private void SetMovementAttribute(MovementAttributes attribute, float newValue)
    //{
    //    switch (attribute)
    //    {
    //        case MovementAttributes.MovementSpeed:
    //            movementSpeed = newValue;
    //            break;
    //        case MovementAttributes.DashBoost:
    //            dashBoost = newValue;
    //            break;
    //        case MovementAttributes.DashCooldown:
    //            dashCoolDown = newValue;
    //            break;
    //        case MovementAttributes.DashDuration:
    //            dashDuration = newValue;
    //            break;
    //    }
    //}
}
