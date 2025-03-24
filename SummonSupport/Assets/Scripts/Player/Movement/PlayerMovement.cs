using UnityEngine;
using UnityEngine.InputSystem;


public class PlayerMovement : MonoBehaviour
{
    #region class Variables
    private Vector2 moveInput;
    public Camera mainCamera;
    [SerializeField] private PlayerInputActions inputActions;
    private Vector2 lookInput;
    [SerializeField] float speed = 5f;
    [SerializeField] float dashBoost = 5f;
    [SerializeField] float dashCoolDown = 1f;
    [SerializeField] float dashDuration = .5f;

    private bool dashing = false;
    private bool canDash = true;

    #endregion

    private void Awake()
    {
        mainCamera = Camera.main;
        inputActions = new PlayerInputActions();
    }
    #region Enable and Disable event subscriptions
    private void OnEnable()
    {
        inputActions.Player.Enable();

        inputActions.Player.Move.performed += OnMove;
        inputActions.Player.Move.canceled += OnMove;

        inputActions.Player.Dash.performed += OnDash;

        inputActions.Player.LookDirection.performed += OnLook;
    }

    private void OnDisable()
    {
        inputActions.Player.Move.performed -= OnMove;
        inputActions.Player.Move.canceled -= OnMove;

        inputActions.Player.Dash.performed -= OnDash;

        inputActions.Player.LookDirection.performed -= OnLook;


        inputActions.Player.Disable();
    }
    #endregion

    #region Dash logic
    private void OnDash(InputAction.CallbackContext context)
    {
        if (canDash) dashing = true;
        Invoke("ReturnToNormalSpeed", dashDuration);
        Invoke("ReadyDash", dashCoolDown);
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
        float calculatedSpeed = 0f;
        if (dashing) calculatedSpeed = speed + dashBoost;
        else calculatedSpeed = speed;
        Vector3 moveDirection = new Vector3(moveInput.x, moveInput.y, 0).normalized;
        transform.position += moveDirection * calculatedSpeed * Time.deltaTime;
    }
    #endregion

    #region Look Direction
    public void OnLook(InputAction.CallbackContext context)
    {
        lookInput = context.ReadValue<Vector2>();
    }
    private Quaternion HandleLook()
    {
        Vector2 worldPosition = mainCamera.ScreenToWorldPoint(lookInput);
        Vector2 direction = (worldPosition - (Vector2)transform.position).normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);

        return transform.rotation;
    }
    #endregion

    private void Update()
    {
        HandleMove();

        HandleLook();


    }
}
