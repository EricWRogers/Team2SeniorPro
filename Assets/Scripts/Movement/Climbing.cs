using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem; 

public class Climbing : MonoBehaviour
{
    [Header("References")]
    public Transform orientation;
    public Rigidbody rb;
    public ThirdPersonMovement tpm;
    public LedgeGrabbing lg;
    public LayerMask whatIsWall;

    [Header("Climbing")]
    public float climbSpeed;
    public float maxClimbTime;
    private float climbTimer;

    private bool climbing;

    [Header("ClimbJumping")]
    public float climbJumpUpForce;
    public float climbJumpBackForce;
    public int climbJumps;
    private int climbJumpsLeft;

    [Header("Detection")]
    public float detectionLength;
    public float sphereCastRadius;
    public float maxWallLookAngle;
    private float wallLookAngle;

    [Header("Height Integration")]
    public bool climbEnabled = true; 

    private RaycastHit frontWallHit;
    private bool wallFront;

    private Transform lastWall;
    private Vector3 lastWallNormal;
    public float minWallNormalAngleChange;

    [Header("Exiting")]
    public bool exitingWall;
    public float exitWallTime;
    private float exitWallTimer;

    [Header("Jump Grace Control")]
    public float jumpIgnoreTime = 0.2f;
    public float jumpIgnoreTimer;

    [Header("Wall Filtering")]
    [Tooltip("Treat surfaces as WALL only if their normal is this many degrees or more away from Up")]
    [Range(0f, 90f)] public float minWallAngleFromUp = 70f;
    [Tooltip("Optional upper bound to exclude ceilings")]
    [Range(90f, 180f)] public float maxWallAngleFromUp = 110f;

    //New Input System
    private PlayerControlsB controls;
    private Vector2 moveInput;
    private bool jumpPressed;
    private bool moveForwardHeld;

    private void Awake()
    {
        controls = new PlayerControlsB();

        // Movement input
        controls.Player.Move.performed += ctx =>
        {
            moveInput = ctx.ReadValue<Vector2>();
            moveForwardHeld = moveInput.y > 0.1f; // forward stick / W
        };
        controls.Player.Move.canceled += ctx =>
        {
            moveInput = Vector2.zero;
            moveForwardHeld = false;
        };

        // Jump input
        controls.Player.Jump.performed += ctx => jumpPressed = true;
        controls.Player.Jump.canceled += ctx => jumpPressed = false;
    }

    private void OnEnable() => controls.Player.Enable();
    private void OnDisable() => controls.Player.Disable();

    private void Start()
    {
        lg = GetComponent<LedgeGrabbing>();
    }

    private void Update()
    {
        if (jumpIgnoreTimer > 0f)
            jumpIgnoreTimer -= Time.deltaTime;

        WallCheck();

        if (!climbEnabled)
        {
            if (climbing) StopClimbing();
            return;
        }

        StateMachine();

        if (climbing && !exitingWall)
            ClimbingMovement();
    }

    private void StateMachine()
    {
        // State 0 - Ledge Grabbing
        if (lg.holding)
        {
            if (climbing) StopClimbing();
            return;
        }

        // State 1 - Climbing
        if (!tpm.grounded && wallFront && moveForwardHeld && wallLookAngle < maxWallLookAngle && !exitingWall)
        {
            if (!climbing && climbTimer > 0)
                StartClimbing();

            if (climbTimer > 0)
                climbTimer -= Time.deltaTime;
            else
                StopClimbing();
        }
        // State 2 - Exiting
        else if (exitingWall)
        {
            if (climbing)
                StopClimbing();

            if (exitWallTimer > 0)
                exitWallTimer -= Time.deltaTime;
            else
                exitingWall = false;
        }
        // State 3 - None
        else
        {
            if (climbing)
                StopClimbing();
        }

        // Handle Climb Jump (press jump while climbing)
        if (wallFront && jumpPressed && climbJumpsLeft > 0)
        {
            jumpPressed = false; // consume input
            ClimbJump();
        }
    }

    private void WallCheck()
    {
        // prevent climb detection while just jumping up
        if (jumpIgnoreTimer > 0f)
        {
            wallFront = false;
            return;
        }

        bool hit = Physics.SphereCast(transform.position, sphereCastRadius, orientation.forward,
                                      out frontWallHit, detectionLength, whatIsWall);

        wallFront = hit && IsVerticalWall(frontWallHit.normal);
        wallLookAngle = wallFront ? Vector3.Angle(orientation.forward, -frontWallHit.normal) : 0f;

        bool newWall = wallFront && (frontWallHit.transform != lastWall
                || Mathf.Abs(Vector3.Angle(lastWallNormal, frontWallHit.normal)) > minWallNormalAngleChange);

        if ((wallFront && newWall) || tpm.grounded)
        {
            climbTimer = maxClimbTime;
            climbJumpsLeft = climbJumps;
        }
    }

    bool IsVerticalWall(Vector3 surfaceNormal)
    {
        float angleToUp = Vector3.Angle(surfaceNormal, Vector3.up);
        return angleToUp >= minWallAngleFromUp && angleToUp <= maxWallAngleFromUp;
    }

    private void StartClimbing()
    {
        climbing = true;
        tpm.climbing = true;
        lastWall = frontWallHit.transform;
        lastWallNormal = frontWallHit.normal;
    }

    private void ClimbingMovement()
    {
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, climbSpeed, rb.linearVelocity.z);
    }

    private void StopClimbing()
    {
        climbing = false;
        tpm.climbing = false;
    }

    private void ClimbJump()
    {
        if (tpm.grounded) return;
        if (lg.holding || lg.exitingLedge) return;

        exitingWall = true;
        exitWallTimer = exitWallTime;

        Vector3 forceToApply = transform.up * climbJumpUpForce + frontWallHit.normal * climbJumpBackForce;

        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        rb.AddForce(forceToApply, ForceMode.Impulse);

        climbJumpsLeft--;
    }
}
