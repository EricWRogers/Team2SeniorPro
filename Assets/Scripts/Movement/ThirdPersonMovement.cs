using UnityEngine;
using System.Collections;
using TMPro;
using UnityEditor;
using NUnit.Framework;
using RangeAttribute = UnityEngine.RangeAttribute;

public class ThirdPersonMovement : MonoBehaviour
{
    [Header("Movement")]
    private float thirdPersonMovementSpeed;
    public float walkSpeed;
    public float sprintSpeed;
    public float slideSpeed;
    public float wallRunSpeed;
    public float vaultSpeed;
    public float climbSpeed;
    public float airMinSpeed;

    private float desiredMovementSpeed;
    private float lastDesiredMovementSpeed;

    public float speedIncreaseMultiplier;
    public float slopeIncreaseMultiplier;

    public float groundDrag;

    [Header("Jumping")]
    public float jumpForce;
    public float jumpCooldown;
    public float airMultiplier;
    bool readyToJump;

    [Header("Crouching")]
    public float crouchSpeed;
    public float crouchYScale;
    private float startYScale;

    [Header("Keybinds")]
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode sprintKey = KeyCode.LeftShift;
    public KeyCode crouchKey = KeyCode.Tab;
    public KeyCode slideKey = KeyCode.LeftControl;

    [Header("Ground Check")]
    public float playerHeight;
    public LayerMask whatIsTheGround;
    public bool grounded;

    [Header("Slope Handling")]
    public float maxSlopeAngle;
    private RaycastHit slopeHit;
    private bool exitingSlope;

    [Header("Air Gravity")]
    public float fallGravityMultiplier; //1.4F
    private float airTimeCounter; //0f

    [Header("Momentum Tuning")]
    public float momentumBlendRiseRate;
    public float momentumBlendFallRate; //25f

    [Header("Slope Adhesion Tuning")]

    [Tooltip("Downward force applied while moving uphill on a slope (helps keep stable contact)")]
    public float uphillStickForce; //60f

    [Tooltip("Downward force while moving downhill on a slope (keep this small so you can jump off/downhill)")]
    public float downhillStickForce; //10f

    [Tooltip("Offset above feet for slope/ground rays")]
    public float groundCheckOffset;  //0.2f

    [Tooltip("Extra length beyond half height for rays to find ground on slopes")]
    public float groundCheckDistance; //0.6f

    [Tooltip("Brief time you remain grounded after small terrain gaps")] 
    public float groundedGraceTime ; //0.15f

    private float lastGroundedTime;

    [Header("Jump Cooldown Filter")]
    public float jumpBufferTime;   // 0.1f time window to ignore duplicate jumps
    private float lastJumpTime;

    [Header("Ground Pound Settings")]
    public bool enableGroundPound = true;
    public float groundPoundForce; //20f
    public float slopeBoostMultiplier; //1.5f
    public float groundPoundCooldown; //0.8f
    public int maxGroundPounds = 3;

    [Header("Crouch Jump Tuning")]

    [Tooltip("Multiplier to make crouch jumps as tall as standing jumps (1 = no change).")]
    public float crouchJumpBoost = 1.15f;

    [Tooltip("Time after a jump during which the crouch downward impulse is suppressed.")]
    public float crouchImpulseBlockTime = 0.08f;


    private bool groundPounding = false;
    private bool canGroundPound = true;
    private int currentGroundPounds;

    /* [Tooltip("Delay before the downward slam starts (like Mario-style charge)")]
     public float groundPoundDelay = 0.3f;

     [Tooltip("Upward stall force during the charge for a visual effect, this is very broken so im stashing for now")]
     public float groundPoundStallForce = 3f;*/


    [Header("Ground Pound UI")]
    public TextMeshProUGUI groundPoundText;

    [Header("Ground Pound SFX")]
    public AudioSource groundPoundSFXSource;
    public AudioClip groundPoundSFX;

    [Header("Sliding SFX")]
    public AudioSource slidingSFXSource;
    public AudioClip slidingSFX;

    [Header("Debuffs")]
    [Range(0f, 1f)] public float movementSlowMultiplier = 1f; 

    [Header("References")]
    public Climbing climbingScript;
    public Transform orientation;

    float horizontalInput;
    float verticalInput;
    Vector3 moveDirection;
    public Rigidbody rb;

    public MovementState state;
    public enum MovementState
    {
        walking,
        sprinting,
        crouching,
        sliding,
        wallrunning,
        vaulting,
        climbing,
        freeze,
        unlimited,
        air
    }

    public bool sliding;
    public bool crouching;
    public bool wallrunning;
    public bool climbing;
    public bool vaulting;
    public bool freeze;
    public bool unlimited;
    public bool restricted;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        readyToJump = true;
        startYScale = transform.localScale.y;

        currentGroundPounds = maxGroundPounds;
    }

    private void Update()
    {
        //grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, whatIsTheGround);

        //Improved slope chck plus grace system
        // Grounded check: only count surfaces whose normal isn't too steep
        Vector3 rayOrigin = transform.position + Vector3.up * groundCheckOffset;
        Vector3 rayDir    = -transform.up; // follows capsule orientation over slopes
        float   rayLen    = playerHeight * 0.5f + groundCheckDistance;

        // Precompute the minimum upDot a surface must have to be walkable.
        float minGroundUpDot = Mathf.Cos(maxSlopeAngle * Mathf.Deg2Rad);

        bool hit = Physics.Raycast(rayOrigin, rayDir, out RaycastHit gHit, rayLen, whatIsTheGround);
        if (hit)
        {
            // upDot = 1 for flat ground, 0 for vertical wall
            float upDot = Vector3.Dot(gHit.normal, Vector3.up);

            // Only treat as grounded if the surface is <= maxSlopeAngle
            if (upDot >= minGroundUpDot)
            {
                grounded = true;
                lastGroundedTime = Time.time;
            }
            else
            {
                // Too steep, use coyote/grace time instead of latching to walls
                grounded = (Time.time - lastGroundedTime) < groundedGraceTime;
            }
        }
        else
        {
            grounded = (Time.time - lastGroundedTime) < groundedGraceTime;
        }
        //WIP for removing walls tick i give up for now.
        Vector3 frontOrigin = transform.position + Vector3.up * (playerHeight * 0.5f);
        if (Physics.Raycast(frontOrigin, orientation.forward, out RaycastHit wallHit, 0.5f, whatIsTheGround))
        {
            float frontUpDot = Vector3.Dot(wallHit.normal, Vector3.up);
            if (frontUpDot < minGroundUpDot)
            {
                // Wall in front is too steep this is supposed to prevent it from being marked but doesnt.
                grounded = (Time.time - lastGroundedTime) < groundedGraceTime;
            }
        }

        MyInput();
        SpeedControl();
        StateHandler();
        HandleSlideAudio();

        if (!grounded)
        {
            airTimeCounter += Time.deltaTime;
            ApplyExtraGravity();
        }
        else //Has Landed on Ground
        {
            if (airTimeCounter > 0.2f)
                ParticleManager.Instance.SpawnParticle("LandingParticleEffect", transform.position - new Vector3(0, playerHeight * 0.3f, 0), Quaternion.Euler(90, 0, 0));

            airTimeCounter = 0f;
        }

        rb.linearDamping = grounded ? groundDrag : 0;
        HandleGroundPoundLanding();
    }

    private void FixedUpdate()
    {
        MovePlayer();
    }

    private void MyInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        // Normal jump
        if (Input.GetKey(jumpKey) && readyToJump && grounded)
        {
            readyToJump = false;
            Jump();
            Invoke(nameof(ResetJump), jumpCooldown);
        }

        // Ground Pound trigger (delayed slam version that doesn't block jump added later as theres issues)
        if (enableGroundPound && Input.GetKeyDown(crouchKey) && !grounded && canGroundPound && currentGroundPounds > 0 && !groundPounding)
        {
            groundPounding = true;
            canGroundPound = false;
            currentGroundPounds--;
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
            rb.AddForce(Vector3.down * groundPoundForce, ForceMode.Impulse);
        }

                // Ground crouching
        if (Input.GetKey(crouchKey) && grounded)
        {
            transform.localScale = new Vector3(transform.localScale.x, crouchYScale, transform.localScale.z);

            // IMPORTANT: don't apply the downward poke if we just jumped
            // (prevents canceling the jump impulse on crouch-jump frames)
            if (Time.time - lastJumpTime >= crouchImpulseBlockTime)
            {
                rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);
            }
        }

        if (Input.GetKeyUp(crouchKey))
            transform.localScale = new Vector3(transform.localScale.x, startYScale, transform.localScale.z);

    }
    
    /*private void PerformGroundPoundSlam()
    {
        // If already landed early, skip
        if (grounded)
        {
            groundPounding = false;
            return;
        }

        rb.AddForce(Vector3.down * groundPoundForce, ForceMode.Impulse);
    }*/
    

    private void HandleGroundPoundLanding()
    {
        if (groundPounding && grounded)
        {
            groundPounding = false;

            // Momentum boost when landing on slope
            if (OnSlope())
            {
                Vector3 slopeDir = GetSlopeMoveDirection(Vector3.down);
                Vector3 slideBoost = slopeDir * groundPoundForce * slopeBoostMultiplier;
                rb.AddForce(slideBoost, ForceMode.Impulse);

                Sliding slide = GetComponent<Sliding>();
                if (slide != null && !sliding)
                    slide.StartSlideExternally();
            }
            else
            {
                // COSMETICS YAYYYYY, this makes theoretically a little bouncy, will be another one too where it will act like marios 3 second delay animation before ground pounding for fun
                //rb.AddForce(Vector3.up * 3f, ForceMode.Impulse);
            }

            // Play SFX
            if (groundPoundSFXSource != null && groundPoundSFX != null)
            {
                groundPoundSFXSource.PlayOneShot(groundPoundSFX);
            }

            Invoke(nameof(ResetGroundPound), groundPoundCooldown);
        }
    }

    private void ResetGroundPound()
    {
        canGroundPound = true;
        if (currentGroundPounds < maxGroundPounds)
            currentGroundPounds++;
    }

    bool keepMomentum;
    private void StateHandler()
    {
        if (freeze)
        {
            state = MovementState.freeze;
            rb.linearVelocity = Vector3.zero;
            desiredMovementSpeed = 0f;
        }
        else if (sliding)
        {
            state = MovementState.sliding;

            desiredMovementSpeed = slideSpeed;
            keepMomentum = true;
        }
        else if (grounded && Input.GetKey(sprintKey))
        {
            state = MovementState.sprinting;
            desiredMovementSpeed = sprintSpeed;
        }
        else if (grounded)
        {
            state = MovementState.walking;
            desiredMovementSpeed = walkSpeed;
        }
        else
        {
            state = MovementState.air;
            desiredMovementSpeed = Mathf.Max(lastDesiredMovementSpeed, airMinSpeed);
        }

        float baseSpeed = desiredMovementSpeed * movementSlowMultiplier;
        Sliding slideComponent = GetComponent<Sliding>();
        float momentum = (slideComponent != null) ? slideComponent.MomentumBoost : 0f;
        float targetSpeed = Mathf.Max(baseSpeed, momentum);
        float rate = (thirdPersonMovementSpeed < targetSpeed) ? momentumBlendRiseRate : momentumBlendFallRate;
        thirdPersonMovementSpeed = Mathf.MoveTowards(thirdPersonMovementSpeed, targetSpeed, rate * Time.deltaTime);
    }

    private void MovePlayer()
    {
        if (restricted) return;
        if (climbingScript != null && climbingScript.exitingWall) return;

        moveDirection = (orientation.forward * verticalInput + orientation.right * horizontalInput).normalized;

        bool onSlope = OnSlope() && !exitingSlope;
        bool hasInput = Mathf.Abs(horizontalInput) > 0.05f || Mathf.Abs(verticalInput) > 0.05f;

        if (onSlope)
        {
            Vector3 slopeDir = GetSlopeMoveDirection(moveDirection);

            // Only move along slope if player is giving input
            if (hasInput)
            {
                rb.AddForce(slopeDir * thirdPersonMovementSpeed * 20f, ForceMode.Force);
            }
            else
            {
                //No input = gently freeze horizontal slope velocity to prevent drift
                Vector3 flatVel = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
                if (flatVel.magnitude > 0.05f)
                {
                    rb.linearVelocity = Vector3.Lerp(rb.linearVelocity, 
                        new Vector3(0f, rb.linearVelocity.y, 0f), 
                        Time.deltaTime * 8f);
                }
            }

            // Adhesion
            float downhillFactor = Vector3.Dot(slopeDir, Vector3.down);

            if (!groundPounding)
            {
                if (downhillFactor > 0f)
                {
                    // Going DOWN slope — use SMALL adhesion
                    rb.AddForce(Vector3.down * downhillStickForce, ForceMode.Force);
                    if (!wallrunning) rb.useGravity = true;
                }
                else
                {
                    // Going UP slope — stronger adhesion
                    if (rb.linearVelocity.y > 0f)
                        rb.AddForce(Vector3.down * uphillStickForce, ForceMode.Force);
                    if (!wallrunning) rb.useGravity = false;
                }
            }

        }
        else if (grounded)
        {
            rb.AddForce(moveDirection * thirdPersonMovementSpeed * 10f, ForceMode.Force);
            if (!wallrunning) rb.useGravity = true;
        }
        else
        {
            //movement while in air
            rb.AddForce(moveDirection * thirdPersonMovementSpeed * 10f * airMultiplier, ForceMode.Force);
            rb.linearDamping = 0.05f;
            if (!wallrunning) rb.useGravity = true;
        }

        if (!wallrunning)
            rb.useGravity = !OnSlope();
    }


    private void SpeedControl()
    {
        if (OnSlope() && !exitingSlope)
        {
            if (rb.linearVelocity.magnitude > thirdPersonMovementSpeed)
                rb.linearVelocity = rb.linearVelocity.normalized * thirdPersonMovementSpeed;
        }
        else
        {
            Vector3 flatVel = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
            if (flatVel.magnitude > thirdPersonMovementSpeed)
            {
                Vector3 limitedVel = flatVel.normalized * thirdPersonMovementSpeed;
                rb.linearVelocity = new Vector3(limitedVel.x, rb.linearVelocity.y, limitedVel.z);
            }
        }
    }

    private void ApplyExtraGravity()
    {
        if (rb.linearVelocity.y < -0.1f)
            rb.AddForce(Physics.gravity * (fallGravityMultiplier - 1f), ForceMode.Acceleration);
    }

    private void Jump()
    {
            // Prevent duplicate calls within jumpBufferTime
        if (Time.time - lastJumpTime < jumpBufferTime)
            return; // ignore if called again too soon

        lastJumpTime = Time.time; // record this jump time
    
        exitingSlope = true;
        grounded = false;

        Vector3 currentVel = rb.linearVelocity;
        rb.linearVelocity = new Vector3(currentVel.x, 0f, currentVel.z);

        bool isCrouchedNow = transform.localScale.y < (startYScale * 0.999f);
        float jumpMult = isCrouchedNow ? crouchJumpBoost : 1f;
        rb.AddForce(transform.up * (jumpForce * jumpMult), ForceMode.Impulse);


       
        if (climbingScript != null)
            climbingScript.jumpIgnoreTimer = climbingScript.jumpIgnoreTime;

        Sliding slide = GetComponent<Sliding>();
        if (slide != null && sliding)
        {
            float slideJumpBoost = Mathf.Lerp(thirdPersonMovementSpeed * 0.5f, slide.maxMomentumSpeed, 0.5f);
            rb.AddForce(orientation.forward * slideJumpBoost, ForceMode.Impulse);
            slide.AddMomentumFromJump(slideJumpBoost * 0.5f);
        }
        else
        {
            rb.AddForce(orientation.forward * (thirdPersonMovementSpeed * 0.25f), ForceMode.Impulse);
        }

        Debug.Log("Player Has Jumped: ");

        StartCoroutine(ForceUngroundRoutine());
    }

    private IEnumerator ForceUngroundRoutine()
    {
        yield return new WaitForSeconds(0.1f);
        exitingSlope = false;
    }

    private void ResetJump()
    {
        readyToJump = true;
        exitingSlope = false;
    }

    public bool OnSlope()
    {
        Vector3 rayOrigin = transform.position + Vector3.up * groundCheckOffset;
        Vector3 rayDir = -transform.up;
        float rayLen = playerHeight * 0.5f + groundCheckDistance;

        if (Physics.Raycast(rayOrigin, rayDir, out slopeHit, rayLen, whatIsTheGround))
        {
            float upDot = Vector3.Dot(slopeHit.normal, Vector3.up);
            float minGroundUpDot = Mathf.Cos(maxSlopeAngle * Mathf.Deg2Rad);

            // Must be walkable (≤ maxSlopeAngle) but not perfectly flat
            return upDot >= minGroundUpDot && upDot < 0.999f;
        }
        return false;

        
    }

    public Vector3 GetSlopeMoveDirection(Vector3 direction)
    {
        return Vector3.ProjectOnPlane(direction, slopeHit.normal).normalized;
    }

    private void HandleSlideAudio()
    {
        // Check if we are sliding
        if (sliding)
        {
            if (!slidingSFXSource.isPlaying)
            {
                slidingSFXSource.clip = slidingSFX;
                slidingSFXSource.loop = true; // keeps playing while sliding
                slidingSFXSource.Play();
            }
        }
        else
        {
            if (slidingSFXSource.isPlaying)
                slidingSFXSource.Stop();
        }
    }
    
}

//player brushing into the wall makes player jump height, thids is due to raycast that is effectve in the tpm controller for tracking walls in climbing
//wall jump consistency make sure isnt being called multiple times, go back through and completely debug the game
// jump off of sloped platforms needs to be fixed, the jumping is less that regualr the gravity on slopes is messed up
//fix air drag way too much and you loose momentum while in the air.

//being able to maintain your momentum even when you stop, so re add when you stop you lose momentum from my original build nefore adding the effect.  Check for the variable that is adding the extra momentum slide after building momentum.
//Currently, fix wall and climb jumps off wall adding extra force randomly, make it fixed / also fix ground pound only allowing movement when slidng down slopes, fix so that you can actually crouch walk
//dont be able to slide up a angle, depending on the angle thats set, it should slow down the character and add decay depending on the angle, make it look realistic so that you dont just speed up slopes unrealistically
//when you stop moving now, remove all momentum in general, dont continue the little slide that decays momentum, just completely cut it.  This causes a bug where when you can momentum towards a wall, you can still move back with the same momentum.
//ground pound is extra broken now, with the change in height gotta fix that.

//RAYCAST NEEDS TO BE DRAGGED DOWN OR UP RESPECTIVELY IN ORDER FOR THE GROUND POUND TO SEE THAT ITS OVER THE GROUND.  At this point the old sensor for grounded, //grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, whatIsTheGround); , is what this needs to mirror with the same new updated functionality that I have now.  What is ground check offset and distance even for when my old one uses the WhatIsGround Layer?

//new primaryt issues, inFINITE juMP  CAN BE a feature technically for now, just gotta mek it less random.  SECOND Getting Stuck on walls AFTER ThIS THE CRAZY BUGS WILL BE NUETRALIZED
//I will need to make a dedicated wall jump, currently wall jump functions from wall running and then climb jumping, but rn it works. Ill lower wall running amount so that it should reduce rubberbanding.
//Remaining issues are, what i typed above, sliding up slopes, and wall stick, moving momentum after running (i have a fix but im tired rn), probably other stuff too, but I got some stuff done. 

//ok i lied wall jumping does work i just edited the values, welp thats done, i also removed wall runing up and down and increased the time, not that thats kinda patched, pretty much got most stuff done so ill worry about it after this build

//relevant changes include wall jump up and side, exacly how it sounds, and climbing, needs to be set right to make easy on lvl