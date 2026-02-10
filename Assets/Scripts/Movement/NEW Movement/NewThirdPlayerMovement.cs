using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;

public class NewThirdPlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    private float moveSpeed;
    private float desiredMoveSpeed;
    private float lastDesiredMoveSpeed;
    public float walkSpeed;
    public float sprintSpeed;
    public float slideSpeed;
    public float wallrunSpeed;
    public float climbSpeed;
    public float vaultSpeed;
    public float airMinSpeed;

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
    public KeyCode crouchKey = KeyCode.LeftControl;

    [Header("Ground Pound")]
    public KeyCode groundPoundKey = KeyCode.LeftAlt;
    public float groundPoundWindup = 0.08f;
    public float groundPoundDownVelocity = 35f;
    public float groundPoundExtraGravity = 2.5f;
    public float groundPoundImpactCooldown = 0.15f;
    public float groundPoundBounceVelocity = 0f;

    [Header("Ground Pound -> Slope Slide")]
    public bool groundPoundBoostOnSlope = true;
    [Tooltip("Minimum planar speed after landing on a slope. If 0, uses walkSpeed.")]
    public float groundPoundSlopeEnterSpeed = 0f;

    private bool groundPounding;
    private bool wasGroundedLastFrame;
    private float groundPoundCooldownTimer;
    private bool leftGroundSinceLastPound;

    [Header("Ground Check")]
    public float playerHeight;
    public LayerMask whatIsGround;
    public bool grounded;

    [Header("Slope Handling")]
    public float maxSlopeAngle;
    private RaycastHit slopeHit;
    private bool exitingSlope;

    [Header("New Input System")]
    public PlayerInput playerInput;

    private InputAction moveAction;
    private InputAction jumpAction;
    private InputAction sprintAction;
    private InputAction crouchAction;
    private InputAction groundPoundAction;

    private Vector2 moveInput;
    private bool sprintHeld;
    private bool crouchHeld;
    private bool jumpPressedThisFrame;
    private bool groundPoundPressedThisFrame;


    [Header("References")]
    public NewClimbing climbingScript;
    private ClimbingDone climbingScriptDone;
    private NewSliding slidingScript; // <-- IMPORTANT: we call this on slope impact
    public Transform orientation;

    float horizontalInput;
    float verticalInput;

    Vector3 moveDirection;
    Rigidbody rb;

    public MovementState state;
    public enum MovementState
    {
        freeze,
        unlimited,
        walking,
        sprinting,
        wallrunning,
        climbing,
        vaulting,
        crouching,
        sliding,
        groundPounding,
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

    public TextMeshProUGUI text_speed;
    public TextMeshProUGUI text_mode;

    private void Start()
    {
        climbingScriptDone = GetComponent<ClimbingDone>();
        slidingScript = GetComponent<NewSliding>();
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        readyToJump = true;
        startYScale = transform.localScale.y;

        wasGroundedLastFrame = false;
        leftGroundSinceLastPound = true;
    }

    private void Update()
    {
        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, whatIsGround, QueryTriggerInteraction.Ignore);

        if (groundPoundCooldownTimer > 0f)
            groundPoundCooldownTimer -= Time.deltaTime;

        if (!grounded)
            leftGroundSinceLastPound = true;

        MyInput();
        SpeedControl();
        StateHandler();
        TextStuff();

        // Impact detection (first grounded frame)
        if (groundPounding && grounded && !wasGroundedLastFrame)
        {
            GroundPoundImpact();
        }
        wasGroundedLastFrame = grounded;

        if (state == MovementState.walking || state == MovementState.sprinting || state == MovementState.crouching)
            rb.linearDamping = groundDrag;
        else
            rb.linearDamping = 0;
    }

    private void FixedUpdate()
    {
        MovePlayer();

        if (groundPounding && !grounded && groundPoundExtraGravity > 1f)
        {
            Vector3 extra = Physics.gravity * (groundPoundExtraGravity - 1f);
            rb.AddForce(extra, ForceMode.Acceleration);
        }
    }

    private void MyInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        // Jump
        if (Input.GetKey(jumpKey) && readyToJump && grounded)
        {
            readyToJump = false;
            Jump();
            Invoke(nameof(ResetJump), jumpCooldown);
        }

        // Ground pound
        if (Input.GetKeyDown(groundPoundKey)
            && !grounded
            && !groundPounding
            && groundPoundCooldownTimer <= 0f
            && leftGroundSinceLastPound
            && !wallrunning
            && !climbing
            && !vaulting)
        {
            StartCoroutine(GroundPoundRoutine());
        }

        // Crouch (NOTE: if you keep crouchKey = LeftControl, change slideKey in NewSliding)
        if (Input.GetKeyDown(crouchKey) && horizontalInput == 0 && verticalInput == 0)
        {
            transform.localScale = new Vector3(transform.localScale.x, crouchYScale, transform.localScale.z);
            rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);
            crouching = true;
        }

        if (Input.GetKeyUp(crouchKey))
        {
            transform.localScale = new Vector3(transform.localScale.x, startYScale, transform.localScale.z);
            crouching = false;
        }
    }

    bool keepMomentum;
    private void StateHandler()
    {
        if (freeze)
        {
            state = MovementState.freeze;
            rb.linearVelocity = Vector3.zero;
            desiredMoveSpeed = 0f;
        }
        else if (unlimited)
        {
            state = MovementState.unlimited;
            desiredMoveSpeed = 999f;
        }
        else if (vaulting)
        {
            state = MovementState.vaulting;
            desiredMoveSpeed = vaultSpeed;
        }
        else if (climbing)
        {
            state = MovementState.climbing;
            desiredMoveSpeed = climbSpeed;
        }
        else if (wallrunning)
        {
            state = MovementState.wallrunning;
            desiredMoveSpeed = wallrunSpeed;
        }
        else if (sliding)
        {
            state = MovementState.sliding;

            if (OnSlope() && rb.linearVelocity.y < 0.1f)
            {
                desiredMoveSpeed = slideSpeed;
                keepMomentum = true;
            }
            else
            {
                desiredMoveSpeed = sprintSpeed;
            }
        }
        else if (crouching)
        {
            state = MovementState.crouching;
            desiredMoveSpeed = crouchSpeed;
        }
        else if (groundPounding)
        {
            state = MovementState.groundPounding;
            desiredMoveSpeed = 0f;
        }
        else if (grounded && Input.GetKey(sprintKey))
        {
            state = MovementState.sprinting;
            desiredMoveSpeed = sprintSpeed;
        }
        else if (grounded)
        {
            state = MovementState.walking;
            desiredMoveSpeed = walkSpeed;
        }
        else
        {
            state = MovementState.air;

            if (!groundPounding && moveSpeed < airMinSpeed)
                desiredMoveSpeed = airMinSpeed;
        }

        bool desiredMoveSpeedHasChanged = desiredMoveSpeed != lastDesiredMoveSpeed;
        if (desiredMoveSpeedHasChanged)
        {
            if (keepMomentum)
            {
                StopAllCoroutines();
                StartCoroutine(SmoothlyLerpMoveSpeed());
            }
            else
            {
                moveSpeed = desiredMoveSpeed;
            }
        }

        lastDesiredMoveSpeed = desiredMoveSpeed;
        if (Mathf.Abs(desiredMoveSpeed - moveSpeed) < 0.1f) keepMomentum = false;
    }

    private IEnumerator SmoothlyLerpMoveSpeed()
    {
        float time = 0;
        float difference = Mathf.Abs(desiredMoveSpeed - moveSpeed);
        float startValue = moveSpeed;

        while (time < difference)
        {
            moveSpeed = Mathf.Lerp(startValue, desiredMoveSpeed, time / difference);

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

        moveSpeed = desiredMoveSpeed;
    }

    private void MovePlayer()
    {
        if (groundPounding) return;
        if (climbingScript != null && climbingScript.exitingWall) return;
        if (restricted) return;

        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        if (OnSlope() && !exitingSlope)
        {
            rb.AddForce(GetSlopeMoveDirection(moveDirection) * moveSpeed * 20f, ForceMode.Force);

            if (rb.linearVelocity.y > 0)
                rb.AddForce(Vector3.down * 80f, ForceMode.Force);
        }
        else if (grounded)
        {
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);
        }
        else
        {
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f * airMultiplier, ForceMode.Force);
        }

        if (!wallrunning)
            rb.useGravity = !OnSlope();
    }

    private void SpeedControl()
    {
        if (OnSlope() && !exitingSlope)
        {
            if (rb.linearVelocity.magnitude > moveSpeed)
                rb.linearVelocity = rb.linearVelocity.normalized * moveSpeed;
        }
        else
        {
            Vector3 flatVel = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

            if (flatVel.magnitude > moveSpeed)
            {
                Vector3 limitedVel = flatVel.normalized * moveSpeed;
                rb.linearVelocity = new Vector3(limitedVel.x, rb.linearVelocity.y, limitedVel.z);
            }
        }
    }

    private void Jump()
    {
        exitingSlope = true;

        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }

    private void ResetJump()
    {
        readyToJump = true;
        exitingSlope = false;
    }

    public bool OnSlope()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out slopeHit, playerHeight * 0.5f + 0.3f, whatIsGround, QueryTriggerInteraction.Ignore))
        {
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
            return angle < maxSlopeAngle && angle != 0;
        }

        return false;
    }

    public Vector3 GetSlopeMoveDirection(Vector3 direction)
    {
        return Vector3.ProjectOnPlane(direction, slopeHit.normal).normalized;
    }

    private void TextStuff()
    {
        if (text_speed == null || text_mode == null) return;

        Vector3 flatVel = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

        if (OnSlope())
            text_speed.SetText("Speed: " + Round(rb.linearVelocity.magnitude, 1) + " / " + Round(moveSpeed, 1));
        else
            text_speed.SetText("Speed: " + Round(flatVel.magnitude, 1) + " / " + Round(moveSpeed, 1));

        text_mode.SetText(state.ToString());
    }

    public static float Round(float value, int digits)
    {
        float mult = Mathf.Pow(10.0f, digits);
        return Mathf.Round(value * mult) / mult;
    }

//Ground pound
    private IEnumerator GroundPoundRoutine()
    {
        groundPounding = true;

        //temp gravity might be off — force it ON for the slam.
        rb.useGravity = true;

        // delay hang: kill upward velocity and damp horizontal
        Vector3 v = rb.linearVelocity;
        if (v.y > 0f) v.y = 0f;
        v.x *= 0.6f;
        v.z *= 0.6f;
        rb.linearVelocity = v;

        float t = 0f;
        while (t < groundPoundWindup && !grounded)
        {
            t += Time.deltaTime;

            Vector3 vv = rb.linearVelocity;
            if (vv.y > 0f) vv.y = 0f;
            rb.linearVelocity = vv;

            yield return null;
        }

        Vector3 slam = rb.linearVelocity;
        slam.y = -groundPoundDownVelocity;
        rb.linearVelocity = slam;
    }

    private void GroundPoundImpact()
    {
        groundPounding = false;
        groundPoundCooldownTimer = groundPoundImpactCooldown;
        leftGroundSinceLastPound = false;

        bool onSlopeNow = OnSlope();

        // If landing on a slope: boost to at least walkSpeed, then start your normal slide script.
        if (groundPoundBoostOnSlope && onSlopeNow && slidingScript != null)
        {
            float minEnterSpeed = (groundPoundSlopeEnterSpeed > 0f) ? groundPoundSlopeEnterSpeed : walkSpeed;

            // downhill direction on the slope
            Vector3 downhill = Vector3.ProjectOnPlane(Vector3.down, slopeHit.normal).normalized;
            if (downhill.sqrMagnitude < 0.001f)
                downhill = Vector3.ProjectOnPlane(orientation.forward, slopeHit.normal).normalized;

            // project current velocity onto slope plane
            Vector3 v = rb.linearVelocity;
            Vector3 planar = Vector3.ProjectOnPlane(v, slopeHit.normal);

            // ensure moving downhill & at least min speed
            float alongDownhill = Vector3.Dot(planar, downhill);
            if (planar.sqrMagnitude < 0.01f || alongDownhill < 0.1f)
                planar = downhill * minEnterSpeed;

            if (planar.magnitude < minEnterSpeed)
                planar = planar.normalized * minEnterSpeed;

            // kill downward Y to avoid jitter
            float newY = v.y;
            if (newY < 0f) newY = 0f;

            rb.linearVelocity = new Vector3(planar.x, newY, planar.z);

            // make your speed system immediately “caught up”
            moveSpeed = Mathf.Max(moveSpeed, walkSpeed);
            lastDesiredMoveSpeed = -9999f;

            // start slide using the actual slide script (scale + forces)
            slidingScript.StartSlideExternal(resetTimer: true);

            return;
        }

        // 
        if (groundPoundBounceVelocity > 0f)
        {
            Vector3 v = rb.linearVelocity;
            v.y = groundPoundBounceVelocity;
            rb.linearVelocity = v;
        }
        else
        {
            Vector3 v = rb.linearVelocity;
            if (v.y < 0f) v.y = 0f;
            rb.linearVelocity = v;
        }
    }
}


//////////////////////////////////////////////
/*
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;

public class NewThirdPlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    private float moveSpeed;
    private float desiredMoveSpeed;
    private float lastDesiredMoveSpeed;
    public float walkSpeed;
    public float sprintSpeed;
    public float slideSpeed;
    public float wallrunSpeed;
    public float climbSpeed;
    public float vaultSpeed;
    public float airMinSpeed;

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
    public KeyCode crouchKey = KeyCode.LeftControl;

    [Header("Ground Pound")]
    public KeyCode groundPoundKey = KeyCode.LeftAlt;
    public float groundPoundWindup = 0.08f;
    public float groundPoundDownVelocity = 35f;
    public float groundPoundExtraGravity = 2.5f;
    public float groundPoundImpactCooldown = 0.15f;
    public float groundPoundBounceVelocity = 0f;

    [Header("Ground Pound -> Slope Slide")]
    public bool groundPoundBoostOnSlope = true;
    [Tooltip("Minimum planar speed after landing on a slope. If 0, uses walkSpeed.")]
    public float groundPoundSlopeEnterSpeed = 0f;

    private bool groundPounding;
    private bool wasGroundedLastFrame;
    private float groundPoundCooldownTimer;
    private bool leftGroundSinceLastPound;

    [Header("Ground Check")]
    public float playerHeight;
    public LayerMask whatIsGround;
    public bool grounded;

    [Header("Slope Handling")]
    public float maxSlopeAngle;
    private RaycastHit slopeHit;
    private bool exitingSlope;

    [Header("References")]
    public NewClimbing climbingScript;
    private ClimbingDone climbingScriptDone;
    private NewSliding slidingScript; // <-- IMPORTANT: we call this on slope impact
    public Transform orientation;

    float horizontalInput;
    float verticalInput;

    Vector3 moveDirection;
    Rigidbody rb;

    public MovementState state;
    public enum MovementState
    {
        freeze,
        unlimited,
        walking,
        sprinting,
        wallrunning,
        climbing,
        vaulting,
        crouching,
        sliding,
        groundPounding,
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

    public TextMeshProUGUI text_speed;
    public TextMeshProUGUI text_mode;

    private void Start()
    {
        climbingScriptDone = GetComponent<ClimbingDone>();
        slidingScript = GetComponent<NewSliding>();
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        readyToJump = true;
        startYScale = transform.localScale.y;

        wasGroundedLastFrame = false;
        leftGroundSinceLastPound = true;
    }

    private void Update()
    {
        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, whatIsGround, QueryTriggerInteraction.Ignore);

        if (groundPoundCooldownTimer > 0f)
            groundPoundCooldownTimer -= Time.deltaTime;

        if (!grounded)
            leftGroundSinceLastPound = true;

        MyInput();
        SpeedControl();
        StateHandler();
        TextStuff();

        // Impact detection (first grounded frame)
        if (groundPounding && grounded && !wasGroundedLastFrame)
        {
            GroundPoundImpact();
        }
        wasGroundedLastFrame = grounded;

        if (state == MovementState.walking || state == MovementState.sprinting || state == MovementState.crouching)
            rb.linearDamping = groundDrag;
        else
            rb.linearDamping = 0;
    }

    private void FixedUpdate()
    {
        MovePlayer();

        if (groundPounding && !grounded && groundPoundExtraGravity > 1f)
        {
            Vector3 extra = Physics.gravity * (groundPoundExtraGravity - 1f);
            rb.AddForce(extra, ForceMode.Acceleration);
        }
    }

    private void MyInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        // Jump
        if (Input.GetKey(jumpKey) && readyToJump && grounded)
        {
            readyToJump = false;
            Jump();
            Invoke(nameof(ResetJump), jumpCooldown);
        }

        // Ground pound
        if (Input.GetKeyDown(groundPoundKey)
            && !grounded
            && !groundPounding
            && groundPoundCooldownTimer <= 0f
            && leftGroundSinceLastPound
            && !wallrunning
            && !climbing
            && !vaulting)
        {
            StartCoroutine(GroundPoundRoutine());
        }

        // Crouch (NOTE: if you keep crouchKey = LeftControl, change slideKey in NewSliding)
        if (Input.GetKeyDown(crouchKey) && horizontalInput == 0 && verticalInput == 0)
        {
            transform.localScale = new Vector3(transform.localScale.x, crouchYScale, transform.localScale.z);
            rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);
            crouching = true;
        }

        if (Input.GetKeyUp(crouchKey))
        {
            transform.localScale = new Vector3(transform.localScale.x, startYScale, transform.localScale.z);
            crouching = false;
        }
    }

    bool keepMomentum;
    private void StateHandler()
    {
        if (freeze)
        {
            state = MovementState.freeze;
            rb.linearVelocity = Vector3.zero;
            desiredMoveSpeed = 0f;
        }
        else if (unlimited)
        {
            state = MovementState.unlimited;
            desiredMoveSpeed = 999f;
        }
        else if (vaulting)
        {
            state = MovementState.vaulting;
            desiredMoveSpeed = vaultSpeed;
        }
        else if (climbing)
        {
            state = MovementState.climbing;
            desiredMoveSpeed = climbSpeed;
        }
        else if (wallrunning)
        {
            state = MovementState.wallrunning;
            desiredMoveSpeed = wallrunSpeed;
        }
        else if (sliding)
        {
            state = MovementState.sliding;

            if (OnSlope() && rb.linearVelocity.y < 0.1f)
            {
                desiredMoveSpeed = slideSpeed;
                keepMomentum = true;
            }
            else
            {
                desiredMoveSpeed = sprintSpeed;
            }
        }
        else if (crouching)
        {
            state = MovementState.crouching;
            desiredMoveSpeed = crouchSpeed;
        }
        else if (groundPounding)
        {
            state = MovementState.groundPounding;
            desiredMoveSpeed = 0f;
        }
        else if (grounded && Input.GetKey(sprintKey))
        {
            state = MovementState.sprinting;
            desiredMoveSpeed = sprintSpeed;
        }
        else if (grounded)
        {
            state = MovementState.walking;
            desiredMoveSpeed = walkSpeed;
        }
        else
        {
            state = MovementState.air;

            if (!groundPounding && moveSpeed < airMinSpeed)
                desiredMoveSpeed = airMinSpeed;
        }

        bool desiredMoveSpeedHasChanged = desiredMoveSpeed != lastDesiredMoveSpeed;
        if (desiredMoveSpeedHasChanged)
        {
            if (keepMomentum)
            {
                StopAllCoroutines();
                StartCoroutine(SmoothlyLerpMoveSpeed());
            }
            else
            {
                moveSpeed = desiredMoveSpeed;
            }
        }

        lastDesiredMoveSpeed = desiredMoveSpeed;
        if (Mathf.Abs(desiredMoveSpeed - moveSpeed) < 0.1f) keepMomentum = false;
    }

    private IEnumerator SmoothlyLerpMoveSpeed()
    {
        float time = 0;
        float difference = Mathf.Abs(desiredMoveSpeed - moveSpeed);
        float startValue = moveSpeed;

        while (time < difference)
        {
            moveSpeed = Mathf.Lerp(startValue, desiredMoveSpeed, time / difference);

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

        moveSpeed = desiredMoveSpeed;
    }

    private void MovePlayer()
    {
        if (groundPounding) return;
        if (climbingScript != null && climbingScript.exitingWall) return;
        if (restricted) return;

        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        if (OnSlope() && !exitingSlope)
        {
            rb.AddForce(GetSlopeMoveDirection(moveDirection) * moveSpeed * 20f, ForceMode.Force);

            if (rb.linearVelocity.y > 0)
                rb.AddForce(Vector3.down * 80f, ForceMode.Force);
        }
        else if (grounded)
        {
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);
        }
        else
        {
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f * airMultiplier, ForceMode.Force);
        }

        if (!wallrunning)
            rb.useGravity = !OnSlope();
    }

    private void SpeedControl()
    {
        if (OnSlope() && !exitingSlope)
        {
            if (rb.linearVelocity.magnitude > moveSpeed)
                rb.linearVelocity = rb.linearVelocity.normalized * moveSpeed;
        }
        else
        {
            Vector3 flatVel = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

            if (flatVel.magnitude > moveSpeed)
            {
                Vector3 limitedVel = flatVel.normalized * moveSpeed;
                rb.linearVelocity = new Vector3(limitedVel.x, rb.linearVelocity.y, limitedVel.z);
            }
        }
    }

    private void Jump()
    {
        exitingSlope = true;

        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }

    private void ResetJump()
    {
        readyToJump = true;
        exitingSlope = false;
    }

    public bool OnSlope()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out slopeHit, playerHeight * 0.5f + 0.3f, whatIsGround, QueryTriggerInteraction.Ignore))
        {
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
            return angle < maxSlopeAngle && angle != 0;
        }

        return false;
    }

    public Vector3 GetSlopeMoveDirection(Vector3 direction)
    {
        return Vector3.ProjectOnPlane(direction, slopeHit.normal).normalized;
    }

    private void TextStuff()
    {
        if (text_speed == null || text_mode == null) return;

        Vector3 flatVel = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

        if (OnSlope())
            text_speed.SetText("Speed: " + Round(rb.linearVelocity.magnitude, 1) + " / " + Round(moveSpeed, 1));
        else
            text_speed.SetText("Speed: " + Round(flatVel.magnitude, 1) + " / " + Round(moveSpeed, 1));

        text_mode.SetText(state.ToString());
    }

    public static float Round(float value, int digits)
    {
        float mult = Mathf.Pow(10.0f, digits);
        return Mathf.Round(value * mult) / mult;
    }

//Ground pound
    private IEnumerator GroundPoundRoutine()
    {
        groundPounding = true;

        //temp gravity might be off — force it ON for the slam.
        rb.useGravity = true;

        // delay hang: kill upward velocity and damp horizontal
        Vector3 v = rb.linearVelocity;
        if (v.y > 0f) v.y = 0f;
        v.x *= 0.6f;
        v.z *= 0.6f;
        rb.linearVelocity = v;

        float t = 0f;
        while (t < groundPoundWindup && !grounded)
        {
            t += Time.deltaTime;

            Vector3 vv = rb.linearVelocity;
            if (vv.y > 0f) vv.y = 0f;
            rb.linearVelocity = vv;

            yield return null;
        }

        Vector3 slam = rb.linearVelocity;
        slam.y = -groundPoundDownVelocity;
        rb.linearVelocity = slam;
    }

    private void GroundPoundImpact()
    {
        groundPounding = false;
        groundPoundCooldownTimer = groundPoundImpactCooldown;
        leftGroundSinceLastPound = false;

        bool onSlopeNow = OnSlope();

        // If landing on a slope: boost to at least walkSpeed, then start your normal slide script.
        if (groundPoundBoostOnSlope && onSlopeNow && slidingScript != null)
        {
            float minEnterSpeed = (groundPoundSlopeEnterSpeed > 0f) ? groundPoundSlopeEnterSpeed : walkSpeed;

            // downhill direction on the slope
            Vector3 downhill = Vector3.ProjectOnPlane(Vector3.down, slopeHit.normal).normalized;
            if (downhill.sqrMagnitude < 0.001f)
                downhill = Vector3.ProjectOnPlane(orientation.forward, slopeHit.normal).normalized;

            // project current velocity onto slope plane
            Vector3 v = rb.linearVelocity;
            Vector3 planar = Vector3.ProjectOnPlane(v, slopeHit.normal);

            // ensure moving downhill & at least min speed
            float alongDownhill = Vector3.Dot(planar, downhill);
            if (planar.sqrMagnitude < 0.01f || alongDownhill < 0.1f)
                planar = downhill * minEnterSpeed;

            if (planar.magnitude < minEnterSpeed)
                planar = planar.normalized * minEnterSpeed;

            // kill downward Y to avoid jitter
            float newY = v.y;
            if (newY < 0f) newY = 0f;

            rb.linearVelocity = new Vector3(planar.x, newY, planar.z);

            // make your speed system immediately “caught up”
            moveSpeed = Mathf.Max(moveSpeed, walkSpeed);
            lastDesiredMoveSpeed = -9999f;

            // start slide using the actual slide script (scale + forces)
            slidingScript.StartSlideExternal(resetTimer: true);

            return;
        }

        // 
        if (groundPoundBounceVelocity > 0f)
        {
            Vector3 v = rb.linearVelocity;
            v.y = groundPoundBounceVelocity;
            rb.linearVelocity = v;
        }
        else
        {
            Vector3 v = rb.linearVelocity;
            if (v.y < 0f) v.y = 0f;
            rb.linearVelocity = v;
        }
    }
}
*/