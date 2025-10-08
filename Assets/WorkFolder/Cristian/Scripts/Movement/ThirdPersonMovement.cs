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

    [Header("Crouching")]
    public float crouchSpeed;
    public float crouchYScale;
    private float startYScale;

    [Header("Keybinds")]
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode sprintKey = KeyCode.LeftShift;
    public KeyCode crouchKey = KeyCode.LeftAlt;
    public KeyCode slideKey = KeyCode.LeftControl;

    [Header("Ground Check")]
    public float playerHeight;
    public LayerMask whatIsTheGround;
    public bool grounded;

    [Header("Slope Handling")]
    public float maxSlopeAngle;
    private RaycastHit slopeHit;
    private bool exitingSlope;

    [Header("Air Gravity Settings")]
    public float upwardGravityMultiplier;     //unified gravity now, this manages GOING UP
    public float downwardGravityMultiplier;   // THIS MANAGES FORCE DOWN

    [Header("Momentum Tuning")]
    public float momentumBlendRiseRate; //40f
    public float momentumBlendFallRate;  //25f

    [Header("Debuffs")]
    [Range(0f, 1f)] public float movementSlowMultiplier; //1f

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
    }

    private void Update()
    {
        grounded = Physics.Raycast(transform.position, Vector3.down,
            playerHeight * 0.5f + 0.2f, whatIsTheGround);

        MyInput();
        SpeedControl();
        StateHandler();

        // drag
        rb.linearDamping = grounded ? groundDrag : 0f;

        // apply unified gravity while airborne
        if (!grounded)
            HandleAirGravity();
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

        if (Input.GetKey(crouchKey))
        {
            transform.localScale = new Vector3(transform.localScale.x, crouchYScale, transform.localScale.z);
            rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);
        }

        if (Input.GetKeyUp(crouchKey))
        {
            transform.localScale = new Vector3(transform.localScale.x, startYScale, transform.localScale.z);
        }
    }

    private void HandleAirGravity()
    {
        if (OnSlope()) return;

        // going up
        if (rb.linearVelocity.y > 0.1f)
        {
            rb.AddForce(Physics.gravity * (upwardGravityMultiplier - 1f), ForceMode.Acceleration);
        }
        // falling down
        else if (rb.linearVelocity.y < -0.1f)
        {
            rb.AddForce(Physics.gravity * (downwardGravityMultiplier - 1f), ForceMode.Acceleration);
        }
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
        else if (unlimited)
        {
            state = MovementState.unlimited;
            desiredMovementSpeed = 999f;
        }
        else if (vaulting)
        {
            state = MovementState.vaulting;
            desiredMovementSpeed = vaultSpeed;
        }
        else if (climbing)
        {
            state = MovementState.climbing;
            desiredMovementSpeed = climbSpeed;
        }
        else if (wallrunning)
        {
            state = MovementState.wallrunning;
            desiredMovementSpeed = wallRunSpeed;
        }
        else if (sliding)
        {
            state = MovementState.sliding;
            desiredMovementSpeed = OnSlope() && rb.linearVelocity.y < 0.1f ? slideSpeed : sprintSpeed;
            keepMomentum = true;
        }
        else if (crouching)
        {
            state = MovementState.crouching;
            desiredMovementSpeed = crouchSpeed;
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
            if (thirdPersonMovementSpeed < airMinSpeed)
                desiredMovementSpeed = airMinSpeed;
        }

        bool speedChanged = desiredMovementSpeed != lastDesiredMovementSpeed;
        if (speedChanged)
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

        // momentum blending
        Sliding slideComponent = GetComponent<Sliding>();
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
        float time = 0;
        float difference = Mathf.Abs(desiredMovementSpeed - thirdPersonMovementSpeed);
        float startValue = thirdPersonMovementSpeed;

        while (time < difference)
        {
            thirdPersonMovementSpeed = Mathf.Lerp(startValue, desiredMovementSpeed, time / difference) * movementSlowMultiplier;

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
        if (climbingScript.exitingWall) return;

        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        if (OnSlope() && !exitingSlope)
        {
            rb.AddForce(GetSlopeMoveDirection(moveDirection) * thirdPersonMovementSpeed * 20f, ForceMode.Force);
            if (rb.linearVelocity.y > 0)
                rb.AddForce(Vector3.down * 80f, ForceMode.Force);
        }
        if (grounded)
        {
            rb.AddForce(moveDirection.normalized * thirdPersonMovementSpeed * 10f, ForceMode.Force);
        }
        else if (!grounded)
        {
            rb.AddForce(moveDirection.normalized * thirdPersonMovementSpeed * 10f * airMultiplier, ForceMode.Force);
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
