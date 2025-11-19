using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Entities;
using Unity.Collections;
using SummonSupportEvents;
using Unity.Mathematics;
using UnityEngine.SceneManagement;
using Unity.VisualScripting;
using System;
using System.Collections.Generic;


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
    private Vector3 worldPosition;
    Vector3 moveDirection;
    [field: SerializeField] public GameObject DashDust { private set; get; }

    private Rigidbody rb;
    private bool dashing = false;
    private bool canDash = true;
    GameObject DashDustInstance = null;
    private bool lockedInUI = false;
    private bool paused = false;
    private bool StuckInAbilityAnimation = false;
    private AnimationControllerScript anim;





    #endregion

    private void Awake()
    {
        Cursor.lockState = CursorLockMode.Locked;   // Locks the cursor to the center of the screen
        Cursor.visible = false;
        rb = GetComponent<Rigidbody>();
        mainCamera = Camera.main;
        inputActions = new PlayerInputActions();
        playerStats = GetComponent<LivingBeing>();
        anim = GetComponent<AnimationControllerScript>();
        if (anim == null) throw new System.Exception($"Animation controller is null. it was not found among children objects.");


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
                DashDustInstance = Instantiate(DashDust, transform.position, Quaternion.identity);
                Vector3 moveDirection = new Vector3(moveInput.x, 0, moveInput.y);
                // make dash dust instance face the opposite direction of player movement. 
                DashDustInstance.transform.rotation = Quaternion.LookRotation(-moveDirection, Vector3.right);
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
        if (Math.Abs(moveInput.x) > .01 || Math.Abs(moveInput.y) > .01)
        {
            float calculatedSpeed;
            if (dashing)
                calculatedSpeed = MovementSpeed + DashBoost;
            else
                calculatedSpeed = MovementSpeed;

            Vector3 moveDirection = transform.TransformDirection(new Vector3(moveInput.x, 0, moveInput.y)).normalized;
            anim.ChangeMovementAnimation(moveInput.x, moveInput.y);
            rb.AddForce(moveDirection * calculatedSpeed * 100f, ForceMode.Acceleration);
        }
        else anim.ChangeAnimation("Idle", .2f);

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

        Vector3 screenCenter = new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0f);
        Ray ray = Camera.main.ScreenPointToRay(screenCenter);
        Debug.DrawRay(ray.origin, ray.direction * 300, Color.cyan, 0.1f);

        if (Physics.Raycast(ray, out RaycastHit hit, 300))
        {
            CommandMinion.HandleCommand(hit);
        }
    }

    private void FixedUpdate()
    {
        if (playerStats.Dead)
        {
            anim.ChangeAnimation("Death", 1f);
            return;
        }

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

}
