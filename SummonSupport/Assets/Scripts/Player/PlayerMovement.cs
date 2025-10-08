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
    private CreatureSpriteController spriteController;
    private LivingBeing playerStats;
    [SerializeField] private PlayerInputActions inputActions;
    [SerializeField] public GameObject AbilityRotation;
    private Vector2 lookInput;
    private Vector2 worldPosition;
    private Vector2 direction;
    Vector3 moveDirection;
    [field: SerializeField] public GameObject DashDust { private set; get; }

    private Rigidbody2D rb;
    private bool dashing = false;
    private bool canDash = true;
    private bool lockedInUI = false;
    private bool StuckInAbilityAnimation = false;
    private bool paused = false;

    #endregion

    private void Awake()
    {
        spriteController = GetComponentInChildren<CreatureSpriteController>();
        rb = GetComponent<Rigidbody2D>();
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
        inputActions.Player.Move.performed += OnMove;
        inputActions.Player.Move.canceled += OnMove;
        inputActions.Player.Dash.performed += OnDash;
        inputActions.Player.LookDirection.performed += OnLook;
        inputActions.Player.CommandMinion.performed += SendMinionCommandContext;
        inputActions.Player.TogglePauseGame.performed += ToggleGamePause;

    }

    private void OnDisable()
    {
        //AlchemyBenchUI.Instance.playerUsingUI.RemoveListener(ToggleLockedInUI);

        EventDeclarer.SpeedAttributeChanged?.RemoveListener(SetMovementAttribute);
        inputActions.Player.Move.performed -= OnMove;
        inputActions.Player.Move.canceled -= OnMove;
        inputActions.Player.Dash.performed -= OnDash;
        inputActions.Player.LookDirection.performed -= OnLook;
        inputActions.Player.CommandMinion.performed -= SendMinionCommandContext;
        inputActions.Player.TogglePauseGame.performed -= ToggleGamePause;

        inputActions.Player.Disable();
    }
    #endregion

    #region Dash logic
    private void OnDash(InputAction.CallbackContext context)
    {
        if (playerStats.Dead) return;

        //EventDeclarer.SpawnEnemies?.Invoke(this.gameObject);
        if (canDash)
        {
            canDash = false;
            dashing = true;
            Invoke("ReturnToNormalSpeed", DashDuration);
            Invoke("ReadyDash", DashCoolDown);
            GameObject DashDustInstance = Instantiate(DashDust, transform.position, Quaternion.identity, transform);
            float angle = Mathf.Atan2(-moveDirection.y, -moveDirection.x) * Mathf.Rad2Deg;
            DashDustInstance.transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle));
            Destroy(DashDustInstance, DashDuration);
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
    private void OnMove(InputAction.CallbackContext context)
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
            //paused = !paused;
            EventDeclarer.TogglePauseGame?.Invoke();

            //if (paused)
            //{
            //    Debug.Log("Pausing");
            //    Time.timeScale = 0f;
            //}
            //else
            //{
            //    Debug.Log("Unpausing");
            //    Time.timeScale = 1f;
            //}
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
