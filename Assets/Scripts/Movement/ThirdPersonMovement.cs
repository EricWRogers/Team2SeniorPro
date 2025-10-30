using UnityEngine;
using System.Collections;
using TMPro;
using RangeAttribute = UnityEngine.RangeAttribute;
using UnityEngine.InputSystem;

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

    [Header("Crouching / Sliding")]
    public float crouchSpeed;
    public float crouchYScale;
    private float startYScale;

    [Header("Keybinds (for reference only)")]
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode sprintKey = KeyCode.LeftShift;
    public KeyCode crouchSlideKey = KeyCode.LeftControl;
    public KeyCode groundPoundKey = KeyCode.Tab;

    [Header("Ground Check")]
    public float playerHeight;
    public LayerMask whatIsTheGround;
    public bool grounded;

    [Header("Slope Handling")]
    public float maxSlopeAngle;
    private RaycastHit slopeHit;
    private bool exitingSlope;

    [Header("Air Gravity")]
    public float fallGravityMultiplier;
    private float airTimeCounter;

    [Header("Momentum Tuning")]
    public float momentumBlendRiseRate;
    public float momentumBlendFallRate;

    [Header("Slope Adhesion Tuning")]
    [Tooltip("Downward force applied while moving uphill on a slope (helps keep stable contact)")]
    public float uphillStickForce;
    [Tooltip("Downward force while moving downhill on a slope (keep this small so you can jump off/downhill)")]
    public float downhillStickForce;
    [Tooltip("Offset above feet for slope/ground rays")]
    public float groundCheckOffset;
    [Tooltip("Extra length beyond half height for rays to find ground on slopes")]
    public float groundCheckDistance;
    [Tooltip("Brief time you remain grounded after small terrain gaps")]
    public float groundedGraceTime;
    private float lastGroundedTime;

    [Header("Jump Cooldown Filter")]
    public float jumpBufferTime;
    private float lastJumpTime;

    [Header("Ground Pound Settings")]
    public bool enableGroundPound = true;
    public float groundPoundForce;
    public float slopeBoostMultiplier;
    public float groundPoundCooldown;
    public int maxGroundPounds = 3;

    [Header("Crouch Jump Tuning")]
    public float crouchJumpBoost = 1.15f;
    public float crouchImpulseBlockTime = 0.08f;

    private bool groundPounding = false;
    private bool canGroundPound = true;
    private int currentGroundPounds;

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
    Rigidbody rb;

    // --- NEW INPUT SYSTEM ---
    private PlayerControlsB controls;
    private Vector2 moveInput;
    private bool jumpPressed;
    private bool crouchSlidePressed;
    private bool crouchSlideHeld;
    private bool groundPoundPressed;

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

    // --- INPUT SETUP ---
    private void Awake()
    {
        controls = new PlayerControlsB();

        // Movement
        controls.Player.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        controls.Player.Move.canceled += ctx => moveInput = Vector2.zero;

        // Jump
        controls.Player.Jump.performed += ctx => jumpPressed = true;
        controls.Player.Jump.canceled += ctx => jumpPressed = false;

        // Combined Crouch / Slide
        controls.Player.CrouchSlide.performed += ctx => { crouchSlidePressed = true; crouchSlideHeld = true; };
        controls.Player.CrouchSlide.canceled += ctx => { crouchSlideHeld = false; };

        // Ground Pound (separate input)
        controls.Player.GroundPound.performed += ctx => groundPoundPressed = true;
        controls.Player.GroundPound.canceled += ctx => groundPoundPressed = false;
    }

    private void OnEnable() => controls.Player.Enable();
    private void OnDisable() => controls.Player.Disable();

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
        // Ground check system
        Vector3 rayOrigin = transform.position + Vector3.up * groundCheckOffset;
        Vector3 rayDir = -transform.up;
        float rayLen = playerHeight * 0.5f + groundCheckDistance;

        float minGroundUpDot = Mathf.Cos(maxSlopeAngle * Mathf.Deg2Rad);
        bool hit = Physics.Raycast(rayOrigin, rayDir, out RaycastHit gHit, rayLen, whatIsTheGround);
        if (hit)
        {
            float upDot = Vector3.Dot(gHit.normal, Vector3.up);
            grounded = upDot >= minGroundUpDot;
            if (grounded) lastGroundedTime = Time.time;
        }
        else
        {
            grounded = (Time.time - lastGroundedTime) < groundedGraceTime;
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
        else
        {
            airTimeCounter = 0f;
        }

        rb.linearDamping = grounded ? groundDrag : 0;
        HandleGroundPoundLanding();
    }

    private void FixedUpdate() => MovePlayer();

    private void MyInput()
    {
        horizontalInput = moveInput.x;
        verticalInput = moveInput.y;

        // --- Jump ---
        if (jumpPressed && readyToJump && grounded)
        {
            readyToJump = false;
            Jump();
            Invoke(nameof(ResetJump), jumpCooldown);
        }

        // --- Ground Pound (own button, now with scrunch effect) ---
        if (enableGroundPound && groundPoundPressed && !grounded && canGroundPound && currentGroundPounds > 0 && !groundPounding)
        {
            groundPoundPressed = false;
            groundPounding = true;
            canGroundPound = false;
            currentGroundPounds--;

            // stop vertical movement for precision
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

            // start the "scrunch down" animation effect before the slam
            StartCoroutine(GroundPoundScrunchThenSlam());
        }

        // --- Combined Crouch / Slide ---
        if (crouchSlideHeld)
        {
            float currentSpeed = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z).magnitude;

            // Case 1: crouch if slow AND grounded
            if (grounded && currentSpeed < (walkSpeed * 0.5f))
            {
                crouching = true;
                sliding = false;
                transform.localScale = new Vector3(transform.localScale.x, crouchYScale, transform.localScale.z);

                if (Time.time - lastJumpTime >= crouchImpulseBlockTime)
                    rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);
            }
            // Case 2: slide if fast OR in air
            else if (currentSpeed >= (walkSpeed * 0.5f) || !grounded)
            {
                crouching = false;

                if (!sliding)
                {
                    sliding = true;

                    // Optional: downward impulse if in air to emphasize aerial dive
                    if (!grounded)
                        rb.AddForce(Vector3.down * 3f, ForceMode.Impulse);

                    Sliding slideComponent = GetComponent<Sliding>();
                    if (slideComponent != null)
                        slideComponent.StartSlideExternally();
                }
            }
        }
        else
        {
            crouching = false;
            sliding = false;

            // only return height if grounded (avoids pop during aerial slides)
            if (grounded)
                transform.localScale = new Vector3(transform.localScale.x, startYScale, transform.localScale.z);
        }
    }
    
    private IEnumerator GroundPoundScrunchThenSlam()
    {
        // Visual wind-up / scrunch effect carried over to keep same feel
        Vector3 originalScale = transform.localScale;
        Vector3 scrunchedScale = new Vector3(originalScale.x, originalScale.y * 0.6f, originalScale.z);

        float scrunchTime = 1.0f;
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime / scrunchTime;
            transform.localScale = Vector3.Lerp(originalScale, scrunchedScale, t);
            yield return null;
        }

        // Apply downward impulse (slam)
        rb.AddForce(Vector3.down * groundPoundForce, ForceMode.Impulse);

        // optional: small delay before allowing scale to reset midair may remove
        yield return new WaitForSeconds(0.15f);

        // smooth restore while falling
        t = 0f;
        while (t < 1f && !grounded)
        {
            t += Time.deltaTime / 0.2f;
            transform.localScale = Vector3.Lerp(scrunchedScale, originalScale, t);
            yield return null;
        }

        // full reset safeguard
        transform.localScale = originalScale;
    }


    private void HandleGroundPoundLanding()
    {
        if (groundPounding && grounded)
        {
            groundPounding = false;

            if (OnSlope())
            {
                Vector3 slopeDir = GetSlopeMoveDirection(Vector3.down);
                Vector3 slideBoost = slopeDir * groundPoundForce * slopeBoostMultiplier;
                rb.AddForce(slideBoost, ForceMode.Impulse);

                Sliding slide = GetComponent<Sliding>();
                if (slide != null && !sliding)
                    slide.StartSlideExternally();
            }

            if (groundPoundSFXSource != null && groundPoundSFX != null)
                groundPoundSFXSource.PlayOneShot(groundPoundSFX);

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
        /*else if (grounded && sprintHeld)
        {
            state = MovementState.sprinting;
            desiredMovementSpeed = sprintSpeed;
        }*/
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
            if (hasInput)
            {
                rb.AddForce(slopeDir * thirdPersonMovementSpeed * 20f, ForceMode.Force);
            }
            else
            {
                Vector3 flatVel = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
                if (flatVel.magnitude > 0.05f)
                {
                    rb.linearVelocity = Vector3.Lerp(rb.linearVelocity,
                        new Vector3(0f, rb.linearVelocity.y, 0f),
                        Time.deltaTime * 8f);
                }
            }

            float downhillFactor = Vector3.Dot(slopeDir, Vector3.down);
            if (!groundPounding)
            {
                if (downhillFactor > 0f)
                {
                    rb.AddForce(Vector3.down * downhillStickForce, ForceMode.Force);
                    if (!wallrunning) rb.useGravity = true;
                }
                else
                {
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
        if (Time.time - lastJumpTime < jumpBufferTime)
            return;

        lastJumpTime = Time.time;
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
        if (sliding)
        {
            if (!slidingSFXSource.isPlaying)
            {
                slidingSFXSource.clip = slidingSFX;
                slidingSFXSource.loop = true;
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


//For slide and control, implement both of these inputs together now.  I need crouch to activate only if the player is moving less than half of the max walk speed, otherwise if the player is moving with wasd or the left stick on controller (just moving in general) more than half of the max speed, the button goes into the statemachine for sliding instead of crouching, this should seamlessly combine crouching and sliding into one button.  Then, we shall also remove ground pounding from crouching and make it its own button.  For keyboard it will be TAB and for controller it will be bumper (for slide, crouch the button on controller is Right bumper). 
//Finish adding controls for slide and crouch, ground pound, and other things in the new input.   