using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WNewWallRunning : MonoBehaviour
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

    [Header("Input")]
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode upwardsRunKey = KeyCode.LeftShift;
    public KeyCode downwardsRunKey = KeyCode.LeftControl;
    private bool upwardsRunning;
    private bool downwardsRunning;
    private float horizontalInput;
    private float verticalInput;

    [Header("Detection")]
    public float wallCheckDistance;
    public float minJumpHeight;
    private RaycastHit leftWallhit;
    private RaycastHit rightWallhit;
    private bool wallLeft;
    private bool wallRight;

    [Header("Exiting")]
    private bool exitingWall;
    public float exitWallTime;
    private float exitWallTimer;

    [Header("Gravity")]
    public bool useGravity;
    public float gravityCounterForce;

    [Header("References")]
    public Transform orientation;
    public NewPlayerCam cam;
    private NewThirdPlayerMovement tpm;
    private NewLedgeGrabbing lg;
    private Rigidbody rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        tpm = GetComponent<NewThirdPlayerMovement>();
        lg = GetComponent<NewLedgeGrabbing>();
    }

    private void Update()
    {
        CheckForWall();
        StateMachine();
    }

    private void FixedUpdate()
    {
        if (tpm.wallrunning)
            WallRunningMovement();
    }

    private void CheckForWall()
    {
        wallRight = Physics.Raycast(transform.position, orientation.right, out rightWallhit, wallCheckDistance, whatIsWall);
        wallLeft = Physics.Raycast(transform.position, -orientation.right, out leftWallhit, wallCheckDistance, whatIsWall);
    }

    private bool AboveGround()
    {
        return !Physics.Raycast(transform.position, Vector3.down, minJumpHeight, whatIsGround);
    }

    private void StateMachine()
    {
        // Getting Inputs
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        upwardsRunning = Input.GetKey(upwardsRunKey);
        downwardsRunning = Input.GetKey(downwardsRunKey);

        // State 1 - Wallrunning
        if((wallLeft || wallRight) && verticalInput > 0 && AboveGround() && !exitingWall)
        {
            if (!tpm.wallrunning)
                StartWallRun();

            // wallrun timer
            if (wallRunTimer > 0)
                wallRunTimer -= Time.deltaTime;

            if(wallRunTimer <= 0 && tpm.wallrunning)
            {
                exitingWall = true;
                exitWallTimer = exitWallTime;
            }

            // wall jump
            if (Input.GetKeyDown(jumpKey)) WallJump();
        }

        // State 2 - Exiting
        else if (exitingWall)
        {
            if (tpm.wallrunning)
                StopWallRun();

            if (exitWallTimer > 0)
                exitWallTimer -= Time.deltaTime;

            if (exitWallTimer <= 0)
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

        // apply camera effects
        if (cam == null) return;
        cam.DoFov(90f);
        if (wallLeft) cam.DoTilt(-5f);
        if (wallRight) cam.DoTilt(5f);
    }

    private void WallRunningMovement()
    {
        rb.useGravity = useGravity;

        Vector3 wallNormal = wallRight ? rightWallhit.normal : leftWallhit.normal;

        Vector3 wallForward = Vector3.Cross(wallNormal, transform.up);

        if ((orientation.forward - wallForward).magnitude > (orientation.forward - -wallForward).magnitude)
            wallForward = -wallForward;

        // forward force
        rb.AddForce(wallForward * wallRunForce, ForceMode.Force);

        // upwards/downwards force
        if (upwardsRunning)
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, wallClimbSpeed, rb.linearVelocity.z);
        if (downwardsRunning)
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, -wallClimbSpeed, rb.linearVelocity.z);

        // push to wall force
        if (!(wallLeft && horizontalInput > 0) && !(wallRight && horizontalInput < 0))
            rb.AddForce(-wallNormal * 100, ForceMode.Force);

        // weaken gravity
        if (useGravity)
            rb.AddForce(transform.up * gravityCounterForce, ForceMode.Force);
    }

    private void StopWallRun()
    {
        tpm.wallrunning = false;

        // reset camera effects
        if (cam == null) return;
        cam.DoFov(80f);
        cam.DoTilt(0f);
    }

    private void WallJump()
    {
        if (lg.holding || lg.exitingLedge) return;

        // enter exiting wall state
        exitingWall = true;
        exitWallTimer = exitWallTime;

        Vector3 wallNormal = wallRight ? rightWallhit.normal : leftWallhit.normal;

        Vector3 forceToApply = transform.up * wallJumpUpForce + wallNormal * wallJumpSideForce;

        // reset y velocity and add force
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        rb.AddForce(forceToApply, ForceMode.Impulse);
    }
}
