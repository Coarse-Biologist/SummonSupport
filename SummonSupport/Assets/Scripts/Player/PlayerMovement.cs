using UnityEngine;
using UnityEngine.InputSystem;
using SummonSupportEvents;
using System;


public class PlayerMovement : MovementScript
{
    #region class Variables
    private Vector2 moveInput;
    public Camera mainCamera;
    private LivingBeing playerStats;
    [SerializeField] private PlayerInputActions inputActions;
    private Vector2 lookInput;
    private Vector3 worldPosition;
    Vector3 moveDirection;
    [field: SerializeField] public GameObject DashDust { private set; get; }

    private Rigidbody rb;
    private bool dashing = false;
    private bool canDash = true;
    GameObject DashDustInstance = null;
    private bool paused = false;
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
        EventDeclarer.UnpauseGame?.AddListener(UnpauseGame);

        EventDeclarer.PauseGame?.AddListener(PauseGame);
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
        EventDeclarer.UnpauseGame?.RemoveListener(UnpauseGame);

        EventDeclarer.PauseGame.RemoveListener(PauseGame);

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
            if (PlayerAbilityHandler.DashAbility != null) PlayerAbilityHandler.DashAbility.Activate(playerStats);
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


    public void UnpauseGame()
    {
        anim.SetUpdateMode(AnimatorUpdateMode.UnscaledTime);
        paused = false;
    }
    public void PauseGame()
    {
        anim.SetUpdateMode(AnimatorUpdateMode.Normal);

        paused = true;

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

            Vector3 force = moveDirection * calculatedSpeed * 100f;
            rb.AddForce(force, ForceMode.Acceleration);

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

        Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward.normalized);
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

        if (!paused)
        {
            HandleMove();

            HandleLook();
        }
    }

    private void ToggleGamePause(InputAction.CallbackContext context)
    {

        if (!paused)
        {
            paused = true;
            EventDeclarer.PauseGame?.Invoke();
        }
        else
        {
            EventDeclarer.UnpauseGame?.Invoke();
            paused = false;
        }
    }

}
