using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    public KeyCode jumpKey = KeyCode.Space;
    public int climbJumps;
    private int climbJumpsLeft;

    [Header("Detection")]
    public float detectionLength;
    public float sphereCastRadius;
    public float maxWallLookAngle;
    private float wallLookAngle;

    [Header("Heist Integration")]
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

    [Header("Wall Filtering")]
    [Tooltip("Treat surfaces as WALL only if their normal is this many degrees or more away from Up")]
    [Range(0f, 90f)] public float minWallAngleFromUp = 70f; // 70–80° is somewhat vertical
    [Tooltip("Optional upper bound to exclude ceilings")]
    [Range(90f, 180f)] public float maxWallAngleFromUp = 110f;

    private void Start()
    {
        lg = GetComponent<LedgeGrabbing>();
    }

    private void Update()
    {
        WallCheck();
        if (!climbEnabled) { if (climbing) StopClimbing(); return; }  // added wall check
        StateMachine();

        if (climbing && !exitingWall) ClimbingMovement();
        
    }

    private void StateMachine()
    {
        // State 0 - Ledge Grabbing
        if (lg.holding)
        {
            if (climbing) StopClimbing();

            // everything else gets handled by the SubStateMachine() in the ledge grabbing script
        }
        
        // State 1 - Climbing
        if (!tpm.grounded && wallFront && Input.GetKey(KeyCode.W) && wallLookAngle < maxWallLookAngle && !exitingWall)
        {
            if (!climbing && climbTimer > 0) StartClimbing();

            // timer
            if (climbTimer > 0) climbTimer -= Time.deltaTime;
            if (climbTimer < 0) StopClimbing();
        }

        // State 2 - Exiting
        else if (exitingWall)
        {
            if (climbing) StopClimbing();

            if (exitWallTimer > 0) exitWallTimer -= Time.deltaTime;
            if (exitWallTimer < 0) exitingWall = false;
        }

        // State 3 - None
        else
        {
            if (climbing) StopClimbing();
        }

        if (wallFront && Input.GetKeyDown(jumpKey) && climbJumpsLeft > 0) ClimbJump();
    }

    private void WallCheck()
    {
        bool hit = Physics.SphereCast(transform.position, sphereCastRadius, orientation.forward,
                                    out frontWallHit, detectionLength, whatIsWall);

        // Only treat as a wall if it’s vertical-ish
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
        lastWallNormal = frontWallHit.normal; //normal in this is direction pointing away from wall

        /// idea - camera fov change
        /// //thinking of adding Tweening FOV camera change here, may be cool but need more research
        /// 
    }
    private void ClimbingMovement()
    {
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, climbSpeed, rb.linearVelocity.z);

        
        /// //this is where I am putting a lot of thoughts for some reason, probably add a sound effect here for climbing and stopping climbing like a grab and release 
    }

    

    private void StopClimbing()
    {
        climbing = false;
        tpm.climbing = false;

        //maybe particles for when timer runs out, this is just for if we end up having a time limit for climbing otherwise this will be removed.
    }
    

    private void ClimbJump()
    {
        if (tpm.grounded) return;
        if (lg.holding || lg.exitingLedge) return;

        print("climbjump");

        exitingWall = true;
        exitWallTimer = exitWallTime;

        Vector3 forceToApply = transform.up * climbJumpUpForce + frontWallHit.normal * climbJumpBackForce;

        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        rb.AddForce(forceToApply, ForceMode.Impulse);

        climbJumpsLeft--;
    }
}

