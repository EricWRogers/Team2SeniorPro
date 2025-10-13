/*using UnityEngine;
using System.Collections;
using TMPro;

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
    public float airMultiplier = 0.7f;
    bool readyToJump = true;

    [Header("Crouching")]
    public float crouchSpeed;
    public float crouchYScale = 0.6f;
    private float startYScale;

    [Header("Keybinds")]
    public KeyCode jumpKey   = KeyCode.Space;
    public KeyCode sprintKey = KeyCode.LeftShift;
    public KeyCode crouchKey = KeyCode.LeftControl;   // unified key
    public KeyCode slideKey  = KeyCode.LeftControl;   // kept for reference, same key

    [Header("Ground Check")]
    public float playerHeight = 2f;
    public LayerMask whatIsTheGround;
    public bool grounded;

    [Header("Slope Handling")]
    public float maxSlopeAngle = 45f;
    private RaycastHit slopeHit;
    private bool exitingSlope;

    [Header("Air Gravity")]
    public float fallGravityMultiplier = 1.35f; // slightly heavier fall

    [Header("Momentum Tuning")]
    public float momentumBlendRiseRate = 40f; // blend toward higher of base/momentum
    public float momentumBlendFallRate = 25f; // blend down toward base

    [Header("Glide Settings")]
    public float glideForce = 12f;          // horizontal accel while gliding
    public float glideGravityScale = 0.6f;  // gravity during glide (0.6 = 60% of normal)
    public float glideMomentumGainPerSec = 6f; // add to momentum/sec while gliding

    [Header("Ground Pound Settings")]
    public bool enableGroundPound = true;
    public float groundPoundForce = 20f;
    public float slopeBoostMultiplier = 1.5f;
    public float groundPoundCooldown = 0.8f;
    public int   maxGroundPounds = 3;

    private bool groundPounding = false;
    private bool canGroundPound = true;
    private int  currentGroundPounds;

    [Header("Ground Pound UI (optional)")]
    public TextMeshProUGUI groundPoundText;

    [Header("Debuffs")]
    [Range(0f, 1f)] public float movementSlowMultiplier = 1f;

    [Header("References")]
    public Climbing climbingScript;
    public Transform orientation;

    float horizontalInput;
    float verticalInput;
    Vector3 moveDirection;
    Rigidbody rb;

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
        air,
        glide
    }

    public MovementState state;

    public bool sliding;
    public bool crouching;
    public bool wallrunning;
    public bool climbing;
    public bool vaulting;
    public bool freeze;
    public bool unlimited;
    public bool restricted;
    public bool gliding; 

    private Sliding slideComponent;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        startYScale = transform.localScale.y;

        slideComponent = GetComponent<Sliding>();

        currentGroundPounds = maxGroundPounds;
        if (groundPoundText) groundPoundText.text = $"Ground Pounds: {currentGroundPounds}";
    }

    private void Update()
    {
        // ground check
        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, whatIsTheGround);

        MyInput();
        SpeedControl();
        StateHandler();

        // gravity
        if (!grounded && !OnSlope())
        {
            // slightly heavier fall only
            if (!gliding && rb.linearVelocity.y < -0.1f)
                rb.AddForce(Physics.gravity * (fallGravityMultiplier - 1f), ForceMode.Acceleration);
        }

        // drag
        rb.linearDamping = grounded ? groundDrag : (gliding ? 0.02f : 0.05f);

        // ground pound landing resolve
        HandleGroundPoundLanding();
    }

    private void FixedUpdate()
    {
        MovePlayer();

        // continuous glide force + momentum build
        if (gliding)
        {
            // forward push
            Vector3 fwd = new Vector3(orientation.forward.x, 0f, orientation.forward.z).normalized;
            rb.AddForce(fwd * glideForce, ForceMode.Acceleration);

            // gravity reduced while gliding
            rb.AddForce(Physics.gravity * (glideGravityScale - 1f), ForceMode.Acceleration);

            // feed momentum into Sliding so chaining builds speed to momentum cap
            if (slideComponent != null)
                slideComponent.AddMomentum(glideMomentumGainPerSec * Time.fixedDeltaTime, refresh: 0.25f);
        }
    }

    private void MyInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput   = Input.GetAxisRaw("Vertical");

        // Jump
        if (Input.GetKey(jumpKey) && readyToJump && grounded)
        {
            readyToJump = false;
            Jump();
            Invoke(nameof(ResetJump), jumpCooldown);
        }

        // Unified CTRL behavior on key down
        if (Input.GetKeyDown(crouchKey))
        {
            if (grounded)
            {
                if (Input.GetKey(sprintKey))
                {
                    // start slide from sprint
                    if (slideComponent != null) slideComponent.StartSlide();
                }
                else
                {
                    // crouch (hold)
                    EnterCrouch();
                }
            }
            else // airborne
            {
                if (Input.GetKey(sprintKey))
                {
                    // GLIDE (air slide)
                    StartGlide();
                }
                else if (enableGroundPound && canGroundPound && currentGroundPounds > 0)
                {
                    // GROUND POUND (no sprint)
                    groundPounding = true;
                    canGroundPound = false;
                    currentGroundPounds--;
                    if (groundPoundText) groundPoundText.text = $"Ground Pounds: {currentGroundPounds}";

                    // reset vertical only, then slam down
                    rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
                    rb.AddForce(Vector3.down * groundPoundForce, ForceMode.Impulse);
                }
            }
        }

        // Unified CTRL behavior on key up 
        if (Input.GetKeyUp(crouchKey))
        {
            if (sliding && slideComponent != null)
            {
                slideComponent.StopSlide(); // release ends slide but momentum persists/decays
            }

            if (crouching)
            {
                ExitCrouch();
            }

            if (gliding)
            {
                StopGlide();
            }
        }
    }

    private void EnterCrouch()
    {
        crouching = true;
        transform.localScale = new Vector3(transform.localScale.x, crouchYScale, transform.localScale.z);
        rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);
    }

    private void ExitCrouch()
    {
        crouching = false;
        transform.localScale = new Vector3(transform.localScale.x, startYScale, transform.localScale.z);
    }

    private void StartGlide()
    {
        gliding = true;
        state = MovementState.glide;
    }

    private void StopGlide()
    {
        gliding = false;
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

                if (slideComponent != null && !sliding)
                    slideComponent.StartSlide();
            }
            else
            {
                // small bounce on flat
                rb.AddForce(Vector3.up * 3f, ForceMode.Impulse);
            }

            Invoke(nameof(ResetGroundPound), groundPoundCooldown);
        }
    }

    private void ResetGroundPound()
    {
        canGroundPound = true;

        if (currentGroundPounds < maxGroundPounds)
        {
            currentGroundPounds++;
            if (groundPoundText) groundPoundText.text = $"Ground Pounds: {currentGroundPounds}";
        }
    }

    bool keepMomentum;
    private void StateHandler()
    {
        // Freeze
        if (freeze)
        {
            state = MovementState.freeze;
            rb.linearVelocity = Vector3.zero;
            desiredMovementSpeed = 0f;
        }
        // Unlimited
        else if (unlimited)
        {
            state = MovementState.unlimited;
            desiredMovementSpeed = 999f;
        }
        // Climbing/Vaulting/Wallrun
        else if (vaulting)   { state = MovementState.vaulting;   desiredMovementSpeed = vaultSpeed; }
        else if (climbing)   { state = MovementState.climbing;   desiredMovementSpeed = climbSpeed; }
        else if (wallrunning){ state = MovementState.wallrunning;desiredMovementSpeed = wallRunSpeed; }
        // Sliding
        else if (sliding)
        {
            state = MovementState.sliding;
            desiredMovementSpeed = slideSpeed;
            keepMomentum = true;
        }
        // Gliding (air slide)
        else if (gliding)
        {
            state = MovementState.glide;
            desiredMovementSpeed = Mathf.Max(lastDesiredMovementSpeed, airMinSpeed);
        }
        // Crouching
        else if (crouching && grounded)
        {
            state = MovementState.crouching;
            desiredMovementSpeed = crouchSpeed;
        }
        // Sprinting
        else if (grounded && Input.GetKey(sprintKey))
        {
            state = MovementState.sprinting;
            desiredMovementSpeed = sprintSpeed;
        }
        // Walking
        else if (grounded)
        {
            state = MovementState.walking;
            desiredMovementSpeed = walkSpeed;
        }
        // Air
        else
        {
            state = MovementState.air;
            desiredMovementSpeed = Mathf.Max(lastDesiredMovementSpeed, airMinSpeed);
        }

        bool changed = desiredMovementSpeed != lastDesiredMovementSpeed;
        if (changed)
        {
            if (keepMomentum)
            {
                StopAllCoroutines();
                StartCoroutine(SmoothlyLerpMoveSpeed());
            }
            else
            {
                thirdPersonMovementSpeed = desiredMovementSpeed * movementSlowMultiplier;
            }
        }

        // Momentum blending with Sliding current momentum
        float baseSpeed = desiredMovementSpeed * movementSlowMultiplier;
        float momentum = (slideComponent != null) ? slideComponent.MomentumBoost : 0f;
        float targetSpeed = Mathf.Max(baseSpeed, momentum);
        float rate = (thirdPersonMovementSpeed < targetSpeed) ? momentumBlendRiseRate : momentumBlendFallRate;

        thirdPersonMovementSpeed = Mathf.MoveTowards(thirdPersonMovementSpeed, targetSpeed, rate * Time.deltaTime);
        lastDesiredMovementSpeed = baseSpeed;

        if (Mathf.Abs(thirdPersonMovementSpeed - baseSpeed) < 0.05f) keepMomentum = false;
    }

    private IEnumerator SmoothlyLerpMoveSpeed()
    {
        float time = 0f;
        float diff = Mathf.Abs(desiredMovementSpeed - thirdPersonMovementSpeed);
        float startValue = thirdPersonMovementSpeed;

        while (time < diff)
        {
            thirdPersonMovementSpeed = Mathf.Lerp(startValue, desiredMovementSpeed, time / diff) * movementSlowMultiplier;

            if (OnSlope())
            {
                float slopeAngle = Vector3.Angle(Vector3.up, slopeHit.normal);
                float slopeAngleIncrease = 1 + (slopeAngle / 90f);
                time += Time.deltaTime * speedIncreaseMultiplier * slopeIncreaseMultiplier * slopeAngleIncrease;
            }
            else
            {
                time += Time.deltaTime * speedIncreaseMultiplier;
            }

            yield return null;
        }

        thirdPersonMovementSpeed = desiredMovementSpeed * movementSlowMultiplier;
    }

    public float CurrentSpeed => thirdPersonMovementSpeed;

    private void MovePlayer()
    {
        if (restricted) return;
        if (climbingScript != null && climbingScript.exitingWall) return;

        // direction
        moveDirection = (orientation.forward * verticalInput + orientation.right * horizontalInput).normalized;

        // slope
        if (OnSlope() && !exitingSlope)
        {
            rb.AddForce(GetSlopeMoveDirection(moveDirection) * thirdPersonMovementSpeed * 20f, ForceMode.Force);
            if (rb.linearVelocity.y > 0) rb.AddForce(Vector3.down * 80f, ForceMode.Force);
        }
        // ground
        else if (grounded)
        {
            rb.AddForce(moveDirection * thirdPersonMovementSpeed * 10f, ForceMode.Force);
        }
        // air
        else
        {
            rb.AddForce(moveDirection * thirdPersonMovementSpeed * 10f * airMultiplier, ForceMode.Force);
        }

        if (!wallrunning) rb.useGravity = !OnSlope();
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

    private void Jump()
    {
        exitingSlope = true;

        // instantly allow air control
        grounded = false;

        // reset vertical only
        Vector3 v = rb.linearVelocity;
        rb.linearVelocity = new Vector3(v.x, 0f, v.z);

        // jump impulse
        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);

        // small forward push to keep flow
        rb.AddForce(orientation.forward * (thirdPersonMovementSpeed * 0.25f), ForceMode.Impulse);

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
        if (Physics.Raycast(transform.position, Vector3.down, out slopeHit, playerHeight * 0.5f + 0.3f))
        {
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
            return angle < maxSlopeAngle && angle != 0f;
        }
        return false;
    }

    public Vector3 GetSlopeMoveDirection(Vector3 direction)
    {
        return Vector3.ProjectOnPlane(direction, slopeHit.normal).normalized;
    }
}
*/
/*
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

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

    [Header("Keybinds")]
    public KeyCode jumpKey = KeyCode.Space;
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
    public float fallGravityMultiplier = 1.4f; // slightly heavier fall for less floatiness
    private float airTimeCounter = 0f;

    [Header("Momentum Tuning")]
    public float momentumBlendRiseRate = 40f; 
    public float momentumBlendFallRate = 25f;

    [Header("Air Boost & Ground Pound")]
    public bool enableGroundPound = true;
    public float groundPoundForce = 20f;
    public float slopeBoostMultiplier = 1.5f;
    public float airBoostForce = 15f;          // forward/downward boost
    public float airBoostMultiplier = 1.5f;    // medium-level boost strength
    public float groundPoundCooldown = 0.8f;

    private bool groundPounding = false;
    private bool canGroundPound = true;

    [Header("Debuffs")]
    [Range(0f, 1f)] public float movementSlowMultiplier = 1f;

    [Header("References")]
    public Climbing climbingScript;
    public Transform orientation;

    private Rigidbody rb;
    private Sliding slideScript; // momentum controller reference

    float horizontalInput;
    float verticalInput;
    Vector3 moveDirection;

    public MovementState state;
    public enum MovementState
    {
        walking,
        crouching,
        sliding,
        air
    }

    public bool sliding;
    public bool crouching;
    public bool restricted;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        readyToJump = true;
        startYScale = transform.localScale.y;

        slideScript = GetComponent<Sliding>(); // grab momentum handler
    }

    private void Update()
    {
        // --- Check ground status ---
        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, whatIsTheGround);

        HandleInput();
        SpeedControl();
        StateHandler();

        // --- Add fall gravity when in air ---
        if (!grounded)
            ApplyExtraGravity();

        // --- Handle air resistance ---
        rb.linearDamping = grounded ? groundDrag : 0f;

        HandleGroundPoundLanding();
    }

    private void FixedUpdate()
    {
        MovePlayer();
    }

    private void HandleInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");
        bool hasInput = Mathf.Abs(horizontalInput) > 0.1f || Mathf.Abs(verticalInput) > 0.1f;

        // --- Jump Logic ---
        if (Input.GetKey(jumpKey) && readyToJump && grounded)
        {
            readyToJump = false;
            Jump();
            Invoke(nameof(ResetJump), jumpCooldown);
        }

        // --- Slide / Crouch / Air Boost / Ground Pound unified control ---
        if (Input.GetKeyDown(slideKey))
        {
            // On the ground
            if (grounded)
            {
                if (hasInput)
                {
                    // Moving? → Start sliding
                    if (slideScript != null)
                        slideScript.StartSlideExternally();
                }
                else
                {
                    // Standing still → crouch
                    StartCrouch();
                }
            }
            // In air
            else
            {
                if (hasInput)
                    AirBoost(); // Forward + Down impulse for chaining
                else if (enableGroundPound && canGroundPound)
                    StartGroundPound(); // Downward only
            }
        }

        // --- Exit crouch when releasing Ctrl ---
        if (Input.GetKeyUp(slideKey) && grounded)
            StopCrouch();
    }

    private void StartCrouch()
    {
        crouching = true;
        state = MovementState.crouching;
        transform.localScale = new Vector3(transform.localScale.x, crouchYScale, transform.localScale.z);
        rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);
    }

    private void StopCrouch()
    {
        crouching = false;
        transform.localScale = new Vector3(transform.localScale.x, startYScale, transform.localScale.z);
    }

    private void AirBoost()
    {
        if (!canGroundPound) return;

        // Apply a medium forward + downward force
        Vector3 boostDir = (orientation.forward + Vector3.down * 0.4f).normalized;
        rb.AddForce(boostDir * airBoostForce * airBoostMultiplier, ForceMode.Impulse);

        // Extend slide momentum decay timer (allows chaining)
        if (slideScript != null)
            slideScript.RefreshMomentumFromAirBoost();

        // short cooldown to avoid spamming
        canGroundPound = false;
        Invoke(nameof(ResetGroundPound), groundPoundCooldown);
    }

    private void StartGroundPound()
    {
        groundPounding = true;
        canGroundPound = false;

        // Remove upward velocity
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        rb.AddForce(Vector3.down * groundPoundForce, ForceMode.Impulse);

        Invoke(nameof(ResetGroundPound), groundPoundCooldown);
    }

    private void HandleGroundPoundLanding()
    {
        if (groundPounding && grounded)
        {
            groundPounding = false;

            // Landing on slope → convert downward momentum to forward slide
            if (OnSlope())
            {
                Vector3 slopeDir = GetSlopeMoveDirection(Vector3.down);
                Vector3 slideBoost = slopeDir * groundPoundForce * slopeBoostMultiplier;
                rb.AddForce(slideBoost, ForceMode.Impulse);

                // Enter slide state automatically
                if (slideScript != null && !sliding)
                    slideScript.StartSlideExternally();
            }
            else
            {
                // On flat → small bounce
                rb.AddForce(Vector3.up * 3f, ForceMode.Impulse);
            }
        }
    }

    private void ResetGroundPound() => canGroundPound = true;

    private void StateHandler()
    {
        // Define active movement state
        if (slideScript != null && slideScript.sliding)
        {
            state = MovementState.sliding;
            desiredMovementSpeed = slideSpeed;
        }
        else if (grounded && crouching)
        {
            state = MovementState.crouching;
            desiredMovementSpeed = crouchSpeed;
        }
        else if (grounded)
        {
            state = MovementState.walking;
            desiredMovementSpeed = walkSpeed;
        }
        else
        {
            state = MovementState.air;
            desiredMovementSpeed = airMinSpeed;
        }

        // Smooth momentum blending between speeds
        float baseSpeed = desiredMovementSpeed * movementSlowMultiplier;
        float momentum = (slideScript != null) ? slideScript.MomentumBoost : 0f;
        float targetSpeed = Mathf.Max(baseSpeed, momentum);

        float rate = (thirdPersonMovementSpeed < targetSpeed) ? momentumBlendRiseRate : momentumBlendFallRate;
        thirdPersonMovementSpeed = Mathf.MoveTowards(thirdPersonMovementSpeed, targetSpeed, rate * Time.deltaTime);
    }

    private void MovePlayer()
    {
        if (restricted) return;
        if (climbingScript != null && climbingScript.exitingWall) return;

        moveDirection = (orientation.forward * verticalInput + orientation.right * horizontalInput).normalized;

        if (OnSlope() && !exitingSlope)
        {
            rb.AddForce(GetSlopeMoveDirection(moveDirection) * thirdPersonMovementSpeed * 20f, ForceMode.Force);
            if (rb.linearVelocity.y > 0)
                rb.AddForce(Vector3.down * 80f, ForceMode.Force);
        }
        else if (grounded)
        {
            rb.AddForce(moveDirection * thirdPersonMovementSpeed * 10f, ForceMode.Force);
        }
        else
        {
            rb.AddForce(moveDirection * thirdPersonMovementSpeed * 10f * airMultiplier, ForceMode.Force);
            rb.linearDamping = 0.05f;
        }

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
        exitingSlope = true;
        grounded = false;

        Vector3 currentVel = rb.linearVelocity;
        rb.linearVelocity = new Vector3(currentVel.x, 0f, currentVel.z);

        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
        rb.AddForce(orientation.forward * (thirdPersonMovementSpeed * 0.25f), ForceMode.Impulse);
    }

    private void ResetJump()
    {
        readyToJump = true;
        exitingSlope = false;
    }

    public bool OnSlope()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out slopeHit, playerHeight * 0.5f + 0.3f))
        {
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
            return angle < maxSlopeAngle && angle != 0f;
        }
        return false;
    }

    public Vector3 GetSlopeMoveDirection(Vector3 direction)
        => Vector3.ProjectOnPlane(direction, slopeHit.normal).normalized;
}
*/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;

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
    public KeyCode crouchKey = KeyCode. Tab;
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
    public float fallGravityMultiplier = 1.4f; // slightly heavier fall
    private float airTimeCounter = 0f;

    [Header("Momentum Tuning")]
    public float momentumBlendRiseRate; //40f
    public float momentumBlendFallRate = 25f;

    [Header("Ground Pound Settings")]
    public bool enableGroundPound = true;
    public float groundPoundForce = 20f;
    public float slopeBoostMultiplier = 1.5f;
    public float groundPoundCooldown = 0.8f;
    public int maxGroundPounds = 3;

    private bool groundPounding = false;
    private bool canGroundPound = true;
    private int currentGroundPounds;

    [Header("Ground Pound UI")]
    public TextMeshProUGUI groundPoundText; // Assign in inspector

    [Header("Debuffs")]
    [Range(0f, 1f)] public float movementSlowMultiplier = 1f;

    [Header("References")]
    public Climbing climbingScript;
    public Transform orientation;

    float horizontalInput;
    float verticalInput;
    Vector3 moveDirection;
    Rigidbody rb;

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
        //UpdateGroundPoundUI();
    }

    private void Update()
    {
        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, whatIsTheGround);

        MyInput();
        SpeedControl();
        StateHandler();

        if (!grounded)
        {
            airTimeCounter += Time.deltaTime;
            ApplyExtraGravity();
        }
        else
        {
            airTimeCounter = 0f;
        }

        if (grounded)
            rb.linearDamping = groundDrag;
        else
            rb.linearDamping = 0;

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


        if (Input.GetKey(jumpKey) && readyToJump && grounded)
        {
            readyToJump = false;
            Jump();
            Invoke(nameof(ResetJump), jumpCooldown);
        }

        // trigger GP
        if (enableGroundPound && Input.GetKeyDown(crouchKey) && !grounded && canGroundPound && currentGroundPounds > 0)
        {
            groundPounding = true;
            canGroundPound = false;
            currentGroundPounds--;
            //UpdateGroundPoundUI();

            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
            rb.AddForce(Vector3.down * groundPoundForce, ForceMode.Impulse);
        }

        // on ground only crouch
        if (Input.GetKey(crouchKey) && grounded)
        {
            transform.localScale = new Vector3(transform.localScale.x, crouchYScale, transform.localScale.z);
            rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);
        }

        if (Input.GetKeyUp(crouchKey))
        {
            transform.localScale = new Vector3(transform.localScale.x, startYScale, transform.localScale.z);
        }
    }

    private void HandleGroundPoundLanding()
    {
        if (groundPounding && grounded)
        {
            groundPounding = false;

            // if land on slope momentum boost forward
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
                // Flat ground small bounce
                rb.AddForce(Vector3.up * 3f, ForceMode.Impulse);
            }

            // recharge timer
            Invoke(nameof(ResetGroundPound), groundPoundCooldown);
        }
    }

    private void ResetGroundPound()
    {
        canGroundPound = true;

        // Recharge a ground pound after cooldown
        if (currentGroundPounds < maxGroundPounds)
        {
            currentGroundPounds++;
            //UpdateGroundPoundUI();
        }

        // Cooldown color fade effect add later
        //StartCoroutine(GroundPoundRechargeRoutine());
    }

   /* private IEnumerator GroundPoundRechargeRoutine()
    {
        if (groundPoundText == null) yield break;

        float elapsed = 0f;
        float duration = groundPoundCooldown;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float fill = Mathf.Clamp01(elapsed / duration);
            groundPoundText.color = Color.Lerp(Color.red, Color.yellow, fill);
            yield return null;
        }

        groundPoundText.color = Color.white;
    }*/

    /*private void UpdateGroundPoundUI() //later logic for ui featured implementation, did it prior but prolly bugs.  Remove all comments when implemented, will prolly give this to Luke or Kaden
    {
        if (groundPoundText != null)
        {
            groundPoundText.text = $"Ground Pounds: {currentGroundPounds}";
        }
    }*/

    bool keepMomentum;
    private void StateHandler()
    {
        // simplified movement state logic
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
            // Use the last ground state's desired speed (walk/sprint) as baseline, but never below airMinSpeed
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

        // --- Input direction ---
        moveDirection = (orientation.forward * verticalInput + orientation.right * horizontalInput).normalized;

        // --- On slope ---
        if (OnSlope() && !exitingSlope)
        {
            rb.AddForce(GetSlopeMoveDirection(moveDirection) * thirdPersonMovementSpeed * 20f, ForceMode.Force);

            if (rb.linearVelocity.y > 0)
                rb.AddForce(Vector3.down * 80f, ForceMode.Force);
        }
        // --- On ground ---
        else if (grounded)
        {
            rb.AddForce(moveDirection * thirdPersonMovementSpeed * 10f, ForceMode.Force);
        }
        // --- In air ---
        else
        {
            // Allow normal movement mid-air
            rb.AddForce(moveDirection * thirdPersonMovementSpeed * 10f * airMultiplier, ForceMode.Force);

            // Slight damping only, so movement continues
            rb.linearDamping = 0.05f;
        }

        // --- Ensure gravity behaves correctly ---
        if (!wallrunning)
            rb.useGravity = !OnSlope();

        if (!grounded)
        {
            Debug.Log($"AirMove active | restricted={restricted}, sliding={sliding}, linearVel={rb.linearVelocity}, airMult={airMultiplier}");
        }
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
        // slightly heavier fall only, no more confusing rising gravity thing that caused problems, atp just lets let the player flaot a bit but have heavy down grav because it need to be able to get into the air, its a platformer
        if (rb.linearVelocity.y < -0.1f)
        {
            rb.AddForce(Physics.gravity * (fallGravityMultiplier - 1f), ForceMode.Acceleration);
        }
    }

    private void Jump()
    {
        exitingSlope = true;

        // Immediately mark the player as not grounded so air movement activates right away
        grounded = false;

        // Reset vertical velocity, keep horizontal momentum
        Vector3 currentVel = rb.linearVelocity;
        rb.linearVelocity = new Vector3(currentVel.x, 0f, currentVel.z);

        // Add upward impulse
        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);

        // Small forward boost for smoother jumps
        rb.AddForce(orientation.forward * (thirdPersonMovementSpeed * 0.25f), ForceMode.Impulse);

        // Optional: add a short “air grace” timer if you like
        StartCoroutine(ForceUngroundRoutine());
    }

    private IEnumerator ForceUngroundRoutine()
    {
        // Forces a short delay before raycasts can mark you grounded again
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
        if (Physics.Raycast(transform.position, Vector3.down, out slopeHit, playerHeight * 0.5f + 0.3f))
        {
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
            return angle < maxSlopeAngle && angle != 0f;
        }
        return false;
    }

    public Vector3 GetSlopeMoveDirection(Vector3 direction)
    {
        return Vector3.ProjectOnPlane(direction, slopeHit.normal).normalized;
    }
}
