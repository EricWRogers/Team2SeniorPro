using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WallRunning : MonoBehaviour  //this entire script is relative to being an addition to climbing, may not actually be implemented, converting 1d to 3d is strange.
{
    [Header("WallRunning")]
    public LayerMask whatIsWall;
    public LayerMask whatIsTheGround;
    public float wallRunForce;
    public float wallClimbSpeed;
    public float wallJumpUpForce;
    public float wallJumpSideForce;
    public float maxWallRunTime;
    private float wallRunTimer;

    [Header("Inputs")]
    //tests for diagnoal wall running which is basically just climbing but not
    public KeyCode upwardsRunKey = KeyCode.LeftShift;
    public KeyCode downwardsRunKey = KeyCode.LeftControl;
    public KeyCode jumpKey = KeyCode.Space;
    private bool upwardsRunning;
    private bool downwardsRunning;
    private float horizontalInput;
    private float verticalInput;

    [Header("Detection")]
    public float wallCheckDistance; //here raycasts have to be checked in a way where its kind of the front of the character since they wont go directly where cam is faced due to different cameras
    public float minJumpHeight;
    private RaycastHit leftWallHit;
    private RaycastHit rightWallHit;
    private bool wallLeft;
    private bool wallRight;

    [Header("Exiting Wall")]
    private bool exitingWall;
    public float exitWallTime;
    private float exitWallTimer;

    [Header("Gravity")]
    public bool useGravity;
    public float gravityCounterForce;

    [Header("References")]

    public Transform orientation;
    private ThirdPersonMovement tpm;
    private Rigidbody rb;
    //private PlayerCam cam;
    private LedgeGrabbing lg;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        tpm = GetComponent<ThirdPersonMovement>();
        lg = GetComponent<LedgeGrabbing>();
    }

    // Update is called once per frame
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
        wallRight = Physics.Raycast(transform.position /*start point*/, orientation.right /*direction*/, out rightWallHit /*stores info of the wall object hit*/, wallCheckDistance /*distance*/, whatIsWall);
        wallLeft = Physics.Raycast(transform.position /*start point*/, -orientation.right /*direction*/, out leftWallHit /*stores info of the wall object hit*/, wallCheckDistance /*distance*/, whatIsWall);
    }

    private bool AboveGround()
    {
        return !Physics.Raycast(transform.position, Vector3.down, minJumpHeight, whatIsTheGround);
    }

    private void StateMachine()
    {
        //Here we create the inputs we get to call
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        upwardsRunning = Input.GetKey(upwardsRunKey);
        downwardsRunning = Input.GetKey(downwardsRunKey);

        //State 1: WallRunning
        if((wallLeft || wallRight) && verticalInput > 0 && AboveGround() && !exitingWall)
        {
            if(!tpm.wallrunning)
                 StartWallRun();
            // wallrun timer
            if(wallRunTimer > 0)
                wallRunTimer -= Time.deltaTime;

            if(wallRunTimer <= 0 && tpm.wallrunning)
            {
                exitingWall = true;
                exitWallTimer = exitWallTime;
            }

            if(Input.GetKeyDown(jumpKey)) WallJump();
        }

        //State 2: Exiting

        else if (exitingWall)
        {
            if (tpm.wallrunning)
                StopWallRun();

            if(exitWallTimer > 0)
                exitWallTimer -= Time.deltaTime;

            if (exitWallTimer <= 0)
                exitingWall = false;
        }
        //State 3: Nothing
        else
        {
            if(tpm.wallrunning)
                 StopWallRun();
        }
    }

    private void StartWallRun() //vector3.Cross(a,b)
    {
        tpm.wallrunning = true;

        wallRunTimer = maxWallRunTime;

        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
    }

    private void WallRunningMovement()
    {
        rb.useGravity = useGravity;
        
        Vector3 wallNormal = wallRight ? rightWallHit.normal : leftWallHit.normal;

        Vector3 wallForward = Vector3.Cross(wallNormal, transform.up);

        if((orientation.forward - wallForward).magnitude > (orientation.forward - -wallForward).magnitude)
            wallForward = -wallForward;  //allows running forward and backwards
 
        //forward force
        rb.AddForce(wallForward * wallRunForce, ForceMode.Force);

        //upwards/downwards wall running force speed
        if (upwardsRunning)
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, wallClimbSpeed, rb.linearVelocity.z);
        if (downwardsRunning)
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, -wallClimbSpeed, rb.linearVelocity.z);

        //push to wall force to allow wall sticking
        if(!(wallLeft && horizontalInput > 0) && !(wallRight && horizontalInput < 0))
            rb.AddForce(-wallNormal * 100, ForceMode.Force);

        //dampens or weakened gravity affects on wall running
        if(useGravity)
            rb.AddForce(transform.up * gravityCounterForce, ForceMode.Force);
    }

    private void StopWallRun()
    {
        tpm.wallrunning = false;
    }

    private void WallJump()
    {
        if(lg.holding || lg.exitingLedge) return;
        // entering exiting wall state
        exitingWall = true;
        exitWallTimer = exitWallTime;

        Vector3 wallNormal = wallRight ? rightWallHit.normal : leftWallHit.normal;

        Vector3 forceToApply = transform.up * wallJumpUpForce + wallNormal * wallJumpSideForce; //so the location of up times force produced to make it go up plus normalized wall location and force from jumping to the side

        //addition of force and resseting of y velocity
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        rb.AddForce(forceToApply, ForceMode.Impulse);

    }
}
