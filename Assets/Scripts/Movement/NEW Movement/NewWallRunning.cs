using UnityEngine;
using UnityEngine.InputSystem;

public class NewWallRunning : MonoBehaviour
{
    [Header("Wallrunning")]
    public LayerMask whatIsWall;
    public LayerMask whatIsGround;
    public float wallRunForce;
    public float wallJumpUpForce;
    public float wallJumpSideForce;
    public float wallClimbSpeed;
    public float maxWallRunTime;
    private float wallRunTimer;

    [Header("Detection")]
    public float wallCheckDistance;
    public float minJumpHeight;
    private RaycastHit leftWallhit;
    private RaycastHit rightWallhit;
    private bool wallLeft;
    private bool wallRight;

    [Header("Exiting")]
    private bool exitingWall;
    public float exitWallTime = 0.2f;
    private float exitWallTimer;

    [Header("Gravity")]
    public bool useGravity = true;
    public float gravityCounterForce = 10f;

    [Header("References")]
    public Transform orientation;
    public NewPlayerCam cam;
    private NewThirdPlayerMovement tpm;
    private NewLedgeGrabbing lg;
    private Rigidbody rb;

    // --- New Input System ---
    private PlayerControlsB controls;
    private Vector2 moveInput;
    private bool upHeld;     // Sprint
    private bool downHeld;   // Crouch
    private bool jumpPressedThisFrame;

    private void Awake()
    {
        controls = new PlayerControlsB();
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        tpm = GetComponent<NewThirdPlayerMovement>();
        lg = GetComponent<NewLedgeGrabbing>();

        if (orientation == null && tpm != null) orientation = tpm.orientation;
        if (orientation == null) orientation = transform;
    }

    private void OnEnable()
    {
        controls.Player.Move.performed += OnMove;
        controls.Player.Move.canceled += OnMove;

        controls.Player.Sprint.started += OnUpStarted;
        controls.Player.Sprint.canceled += OnUpCanceled;

        controls.Player.Crouch.started += OnDownStarted;
        controls.Player.Crouch.canceled += OnDownCanceled;

        controls.Player.Jump.started += OnJumpStarted;

        controls.Player.Enable();
    }

    private void OnDisable()
    {
        controls.Player.Move.performed -= OnMove;
        controls.Player.Move.canceled -= OnMove;

        controls.Player.Sprint.started -= OnUpStarted;
        controls.Player.Sprint.canceled -= OnUpCanceled;

        controls.Player.Crouch.started -= OnDownStarted;
        controls.Player.Crouch.canceled -= OnDownCanceled;

        controls.Player.Jump.started -= OnJumpStarted;

        controls.Player.Disable();
    }

    private void OnMove(InputAction.CallbackContext ctx) => moveInput = ctx.ReadValue<Vector2>();
    private void OnUpStarted(InputAction.CallbackContext _) => upHeld = true;
    private void OnUpCanceled(InputAction.CallbackContext _) => upHeld = false;
    private void OnDownStarted(InputAction.CallbackContext _) => downHeld = true;
    private void OnDownCanceled(InputAction.CallbackContext _) => downHeld = false;
    private void OnJumpStarted(InputAction.CallbackContext _) => jumpPressedThisFrame = true;

    private void Update()
    {
        CheckForWall();
        StateMachine();

        // clear one-frame press
        jumpPressedThisFrame = false;
    }

    private void FixedUpdate()
    {
        if (tpm != null && tpm.wallrunning)
            WallRunningMovement();
    }

    private void CheckForWall()
    {
        wallRight = Physics.Raycast(transform.position, orientation.right, out rightWallhit, wallCheckDistance, whatIsWall);
        wallLeft  = Physics.Raycast(transform.position, -orientation.right, out leftWallhit, wallCheckDistance, whatIsWall);
    }

    private bool AboveGround()
    {
        return !Physics.Raycast(transform.position, Vector3.down, minJumpHeight, whatIsGround);
    }

    private void StateMachine()
    {
        float horizontalInput = moveInput.x;
        float verticalInput   = moveInput.y;

        // State 1 - Wallrunning
        if ((wallLeft || wallRight) && verticalInput > 0.1f && AboveGround() && !exitingWall)
        {
            if (!tpm.wallrunning)
                StartWallRun();

            if (wallRunTimer > 0f)
                wallRunTimer -= Time.deltaTime;

            if (wallRunTimer <= 0f && tpm.wallrunning)
            {
                exitingWall = true;
                exitWallTimer = exitWallTime;
            }

            if (jumpPressedThisFrame)
                WallJump();
        }
        // State 2 - Exiting
        else if (exitingWall)
        {
            if (tpm.wallrunning)
                StopWallRun();

            if (exitWallTimer > 0f)
                exitWallTimer -= Time.deltaTime;

            if (exitWallTimer <= 0f)
                exitingWall = false;
        }
        // State 3 - None
        else
        {
            if (tpm.wallrunning)
                StopWallRun();
        }
    }

    private void StartWallRun()
    {
        tpm.wallrunning = true;
        wallRunTimer = maxWallRunTime;

        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

        if (cam != null)
        {
            cam.DoFov(90f);
            if (wallLeft) cam.DoTilt(-5f);
            if (wallRight) cam.DoTilt(5f);
        }
    }

    private void WallRunningMovement()
    {
        rb.useGravity = useGravity;

        float horizontalInput = moveInput.x;

        Vector3 wallNormal = wallRight ? rightWallhit.normal : leftWallhit.normal;
        Vector3 wallForward = Vector3.Cross(wallNormal, transform.up);

        if ((orientation.forward - wallForward).sqrMagnitude > (orientation.forward + wallForward).sqrMagnitude)
            wallForward = -wallForward;

        rb.AddForce(wallForward * wallRunForce, ForceMode.Force);

        if (upHeld)
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, wallClimbSpeed, rb.linearVelocity.z);
        else if (downHeld)
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, -wallClimbSpeed, rb.linearVelocity.z);

        if (!(wallLeft && horizontalInput > 0f) && !(wallRight && horizontalInput < 0f))
            rb.AddForce(-wallNormal * 100f, ForceMode.Force);

        if (useGravity)
            rb.AddForce(transform.up * gravityCounterForce, ForceMode.Force);
    }

    private void StopWallRun()
    {
        tpm.wallrunning = false;

        if (cam != null)
        {
            cam.DoFov(80f);
            cam.DoTilt(0f);
        }
    }

    private void WallJump()
    {
        if (lg != null && (lg.holding || lg.exitingLedge)) return;

        exitingWall = true;
        exitWallTimer = exitWallTime;

        Vector3 wallNormal = wallRight ? rightWallhit.normal : leftWallhit.normal;
        Vector3 forceToApply = transform.up * wallJumpUpForce + wallNormal * wallJumpSideForce;

        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        rb.AddForce(forceToApply, ForceMode.Impulse);
    }
}
