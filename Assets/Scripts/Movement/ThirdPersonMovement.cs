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
    public float fallGravityMultiplier = 1.4f;
    private float airTimeCounter = 0f;

    [Header("Momentum Tuning")]
    public float momentumBlendRiseRate;
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
    public TextMeshProUGUI groundPoundText;

    [Header("Debuffs")]
    [Range(0f, 1f)] public float movementSlowMultiplier = 1f;

    [Header("References")]
    public GameObject landingParticleEffect;
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
        landingParticleEffect = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Particles/LandingParticleEffect.prefab");
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        readyToJump = true;
        startYScale = transform.localScale.y;

        currentGroundPounds = maxGroundPounds;
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
        else //Has Landed on Ground
        {
            if(airTimeCounter > 0.2f)
            Instantiate(landingParticleEffect, transform.position - new Vector3(0, playerHeight * 0.3f, 0), Quaternion.Euler(90, 0, 0));

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

        // Ground Pound trigger
        if (enableGroundPound && Input.GetKeyDown(crouchKey) && !grounded && canGroundPound && currentGroundPounds > 0)
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
            rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);
        }

        if (Input.GetKeyUp(crouchKey))
            transform.localScale = new Vector3(transform.localScale.x, startYScale, transform.localScale.z);
    }

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
                // --- Bounce on flat ground (commented out for now) ---
                // rb.AddForce(Vector3.up * 3f, ForceMode.Impulse);
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
        exitingSlope = true;
        grounded = false;

        Vector3 currentVel = rb.linearVelocity;
        rb.linearVelocity = new Vector3(currentVel.x, 0f, currentVel.z);
        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);

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

    public bool OnSlope() //THIS needs changing
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

//player brushing into the wall makes player jump height, thids is due to raycast that is effectve in the tpm controller for tracking walls in climbing
//wall jump consistency make sure isnt being called multiple times, go back through and completely debug the game
// jump off of sloped platforms needs to be fixed, the jumping is less that regualr the gravity on slopes is messed up
//fix air drag way too much and you loose momentum while in the air.
//being able to maintain your momentum even when you stop, so re add when you stop you lose momentum from my original build nefore adding the effect.  Check for the variable that is adding the extra momentum slide after building momentum.