using UnityEngine;
using UnityEngine.InputSystem;

public class NewClimbing : MonoBehaviour
{
    [Header("References")]
    public Transform orientation;
    public Rigidbody rb;
    public NewThirdPlayerMovement tpm;
    public NewLedgeGrabbing lg;
    public LayerMask whatIsWall;

    [Header("Climbing")]
    public float climbSpeed = 6f;
    public float maxClimbTime = 1.25f;
    private float climbTimer;

    public bool climbing;

    [Header("ClimbJumping")]
    public float climbJumpUpForce = 6f;
    public float climbJumpBackForce = 4f;

    public int climbJumps = 1;
    private int climbJumpsLeft;

    [Header("Detection")]
    public float detectionLength = 1.0f;
    public float sphereCastRadius = 0.35f;
    public float maxWallLookAngle = 35f;
    private float wallLookAngle;

    private RaycastHit frontWallHit;
    private bool wallFront;

    private Transform lastWall;
    private Vector3 lastWallNormal;
    public float minWallNormalAngleChange = 5f;

    [Header("Exiting")]
    public bool exitingWall;
    public float exitWallTime = 0.2f;
    private float exitWallTimer;

    // ---- New Input System ----
    private PlayerControlsB controls;
    private Vector2 moveInput;
    private bool jumpPressedThisFrame;

    private void Awake()
    {
        controls = new PlayerControlsB();
    }

    private void Start()
    {
        if (rb == null) rb = GetComponent<Rigidbody>();
        if (tpm == null) tpm = GetComponent<NewThirdPlayerMovement>();
        if (lg == null) lg = GetComponent<NewLedgeGrabbing>();
        if (orientation == null && tpm != null) orientation = tpm.orientation;
        if (orientation == null) orientation = transform;

        climbTimer = maxClimbTime;
        climbJumpsLeft = climbJumps;
    }

    private void OnEnable()
    {
        controls.Player.Move.performed += OnMove;
        controls.Player.Move.canceled += OnMove;

        controls.Player.Jump.started += OnJumpStarted;

        controls.Player.Enable();
    }

    private void OnDisable()
    {
        controls.Player.Move.performed -= OnMove;
        controls.Player.Move.canceled -= OnMove;

        controls.Player.Jump.started -= OnJumpStarted;

        controls.Player.Disable();
    }

    private void OnMove(InputAction.CallbackContext ctx) => moveInput = ctx.ReadValue<Vector2>();
    private void OnJumpStarted(InputAction.CallbackContext _) => jumpPressedThisFrame = true;

    private void Update()
    {
        WallCheck();
        StateMachine();

        if (climbing && !exitingWall)
            ClimbingMovement();

        // one-frame press clear
        jumpPressedThisFrame = false;
    }

    private void StateMachine()
    {
        bool wantsForward = moveInput.y > 0.1f;

        // State 0 - Ledge Grabbing
        if (lg != null && lg.holding)
        {
            if (climbing) StopClimbing();
            return;
        }

        // State 2 - Exiting
        if (exitingWall)
        {
            if (climbing) StopClimbing();

            if (exitWallTimer > 0f) exitWallTimer -= Time.deltaTime;
            if (exitWallTimer <= 0f) exitingWall = false;
            return;
        }

        // State 1 - Climbing
        if (wallFront && wantsForward && wallLookAngle < maxWallLookAngle && !exitingWall)
        {
            if (!climbing && climbTimer > 0f)
                StartClimbing();

            if (climbTimer > 0f) climbTimer -= Time.deltaTime;
            if (climbTimer <= 0f) StopClimbing();
        }
        else
        {
            // State 3 - None
            if (climbing) StopClimbing();
        }

        // Climb jump
        if (wallFront && jumpPressedThisFrame && climbJumpsLeft > 0)
            ClimbJump();
    }

    private void WallCheck()
    {
        wallFront = Physics.SphereCast(
            transform.position,
            sphereCastRadius,
            orientation.forward,
            out frontWallHit,
            detectionLength,
            whatIsWall,
            QueryTriggerInteraction.Ignore
        );

        if (wallFront)
        {
            wallLookAngle = Vector3.Angle(orientation.forward, -frontWallHit.normal);

            bool newWall =
                frontWallHit.transform != lastWall ||
                Mathf.Abs(Vector3.Angle(lastWallNormal, frontWallHit.normal)) > minWallNormalAngleChange;

            if (newWall || (tpm != null && tpm.grounded))
            {
                climbTimer = maxClimbTime;
                climbJumpsLeft = climbJumps;
            }
        }
    }

    private void StartClimbing()
    {
        climbing = true;

        if (tpm != null)
        {
            tpm.climbing = true;

            // IMPORTANT: prevents NewThirdPlayerMovement from fighting climb velocity/forces
            tpm.restricted = true;
        }

        lastWall = frontWallHit.transform;
        lastWallNormal = frontWallHit.normal;
    }

    private void ClimbingMovement()
    {
        // Keep it simple: constant upward velocity while climbing
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, climbSpeed, rb.linearVelocity.z);
    }

    private void StopClimbing()
    {
        climbing = false;

        if (tpm != null)
        {
            tpm.climbing = false;
            tpm.restricted = false;
        }
    }

    private void ClimbJump()
    {
        if (tpm != null && tpm.grounded) return;
        if (lg != null && (lg.holding || lg.exitingLedge)) return;

        exitingWall = true;
        exitWallTimer = exitWallTime;

        Vector3 forceToApply = transform.up * climbJumpUpForce + frontWallHit.normal * climbJumpBackForce;

        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        rb.AddForce(forceToApply, ForceMode.Impulse);

        climbJumpsLeft--;
    }
}
