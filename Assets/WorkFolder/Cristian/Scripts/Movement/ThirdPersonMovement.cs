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

    //[Header("Climbing")]
    //public float climbSpeed;
    //public float climbXSpeed; //moving up down left right on wall


    [Header("Crouching")]

     public float crouchSpeed;
     public float crouchYScale;
     private float startYScale;

    [Header("Keybinds")]

    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode sprintKey = KeyCode.LeftShift;  //will use states for this that go between sprinting, walking and air static positions to shift movement similar to the camera change
    public KeyCode crouchKey = KeyCode.LeftAlt;
    public KeyCode slideKey = KeyCode.LeftControl;

    [Header("Ground Check")]

    public float playerHeight;

    public LayerMask whatIsTheGround;

    public bool grounded;

    [Header("Slope Handling")]  //enhance this
    public float maxSlopeAngle;
    private RaycastHit slopeHit;
    private bool exitingSlope;

    [Header("Air Gravity")]
    public float extraGravityDelay = 7f;       // time in air before stronger gravity kicks in
    public float fallGravityMultiplier = 1f;   // how much stronger gravity gets

    private float airTimeCounter = 0f;

    [Header("Air Gravity Tuning")]
    public float upwardGravityMultiplier = 1.5f;  // when going up
    public float downwardGravityMultiplier = 2.5f; // when falling

    [Header("Momentum Tuning")]
    public float momentumBlendRiseRate = 40f; // how fast we blend up toward target
    public float momentumBlendFallRate = 25f; // how fast we blend down toward base


    [Header("Debuffs")]  //regardence to adding slow mult for debuffs from traps
    [Range(0f, 1f)] public float movementSlowMultiplier = 1f; // 1 = normal, 0.5 = half speed, 0 = frozen

    [Header("References")]
    public Climbing climbingScript;

    public Transform orientation;

    float horizontalInput;
    float verticalInput;

    Vector3 moveDirection;

    Rigidbody rb;

    public MovementState state; //stores current state player is in

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
        //ground check will cast raycast down to see if the ground exist 
        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, whatIsTheGround);

        MyInput();
        SpeedControl();
        StateHandler();

        //this if statement handles the drag and requires layer mask set up on map ground

        if (!grounded)
        {
            airTimeCounter += Time.deltaTime;
            ApplyExtraGravity();
        }
        else
        {
            airTimeCounter = 0f; // reset on landing
        }

            if (!grounded)
        {
            ApplyBetterJumpGravity();
        }

        if (grounded)
            rb.linearDamping = groundDrag;
        else
            rb.linearDamping = 0;
    }

    private void FixedUpdate()
    {
        MovePlayer();
    }

    private void MyInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        //when jumps checks to see if space jump input is pressed
        if(Input.GetKey(jumpKey) && readyToJump && grounded)
        {
            readyToJump = false;

            Jump();

            Invoke(nameof(ResetJump), jumpCooldown);  //deals w/ continuous jumping
        }

        //commence crouching goofy shringking that may work wierd
        if (Input.GetKey(crouchKey))
        {
            transform.localScale = new Vector3(transform.localScale.x, crouchYScale, transform.localScale.z);
            rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);  //this makes it to where u dont float in air and you go down to the ground that is marked with tag 
        }

        //End Crouching
        if(Input.GetKeyUp(crouchKey))
        {
            transform.localScale = new Vector3(transform.localScale.x, startYScale, transform.localScale.z);
        }
    }

        private void ApplyBetterJumpGravity()
    {
        // Going up
        if (rb.linearVelocity.y > 0.1f)
        {
            rb.AddForce(Physics.gravity * (upwardGravityMultiplier - 1f), ForceMode.Acceleration);
        }
        // Falling down
        else if (rb.linearVelocity.y < -0.1f)
        {
            rb.AddForce(Physics.gravity * (downwardGravityMultiplier - 1f), ForceMode.Acceleration);
        }
    }

    bool keepMomentum;
    private void StateHandler()
    {
        // Mode - Freeze
        if (freeze)
        {
            state = MovementState.freeze;
            rb.linearVelocity = Vector3.zero;
            desiredMovementSpeed = 0f;
        }

        // Mode - Unlimited
        else if (unlimited)
        {
            state = MovementState.unlimited;
            desiredMovementSpeed = 999f;
        }

        // Mode - Vaulting
        else if (vaulting)
        {
            state = MovementState.vaulting;
            desiredMovementSpeed = vaultSpeed;
        }

        // Mode - Climbing
        else if (climbing)
        {
            state = MovementState.climbing;
            desiredMovementSpeed = climbSpeed;
        }

        // Mode - Wallrunning
        else if (wallrunning)
        {
            state = MovementState.wallrunning;
            desiredMovementSpeed = wallRunSpeed;
        }

        // Mode - Sliding
        else if (sliding)
        {
            state = MovementState.sliding;

            // increase speed by one every second
            if (OnSlope() && rb.linearVelocity.y < 0.1f)
            {
                desiredMovementSpeed = slideSpeed;
                keepMomentum = true;
            }

            else
                desiredMovementSpeed = sprintSpeed;
        }

        // Mode - Crouching
        else if (crouching)
        {
            state = MovementState.crouching;
            desiredMovementSpeed = crouchSpeed;
        }

        // Mode - Sprinting
        else if (grounded && Input.GetKey(sprintKey))
        {
            state = MovementState.sprinting;
            desiredMovementSpeed = sprintSpeed;
        }

        // Mode - Walking
        else if (grounded)
        {
            state = MovementState.walking;
            desiredMovementSpeed = walkSpeed;
        }

        // Mode - Air
        else
        {
            state = MovementState.air;

            if (thirdPersonMovementSpeed < airMinSpeed)
                desiredMovementSpeed = airMinSpeed;
        }

        bool desiredMovementSpeedHasChanged = desiredMovementSpeed != lastDesiredMovementSpeed;

        if (desiredMovementSpeedHasChanged)
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

        lastDesiredMovementSpeed = desiredMovementSpeed * movementSlowMultiplier;

        // --- Momentum resolution (smoothly blend speed toward the higher of base or momentum) ---
        Sliding slideComponent = GetComponent<Sliding>();
        float baseSpeed = desiredMovementSpeed * movementSlowMultiplier;
        float momentum  = (slideComponent != null) ? slideComponent.MomentumBoost : 0f;

        // the target is whichever is higher: your state speed or the fading momentum
        float targetSpeed = Mathf.Max(baseSpeed, momentum);

        // choose rise vs fall rate for natural feel
        float rate = (thirdPersonMovementSpeed < targetSpeed) ? momentumBlendRiseRate : momentumBlendFallRate;

        // blend current speed toward target
        thirdPersonMovementSpeed = Mathf.MoveTowards(thirdPersonMovementSpeed, targetSpeed, rate * Time.deltaTime);

        // keep lastDesiredMovementSpeed as you already do
        lastDesiredMovementSpeed = baseSpeed;

        // if we're basically at the base speed again, momentum is effectively done
        if (Mathf.Abs(thirdPersonMovementSpeed - baseSpeed) < 0.05f) keepMomentum = false;

        // deactivate keepMomentum
        if (Mathf.Abs(desiredMovementSpeed - thirdPersonMovementSpeed) < 0.1f) keepMomentum = false;
    }


    //to make to where the speed adjusts slowly and doesnt immediately change back to og speed

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
                time += Time.deltaTime * speedIncreaseMultiplier;

            yield return null;  
        }

        thirdPersonMovementSpeed = desiredMovementSpeed * movementSlowMultiplier;
    }
    
    //reference shade
    public float CurrentSpeed
    {
        get { return thirdPersonMovementSpeed; }
    }

    private void MovePlayer()
    {
        if (restricted) return; //restricts movement and also occurs during climbing, ledge stuff, and vaulting i think

        if (climbingScript.exitingWall) return;
        //calculates players overall movement direction
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        //when on slope
        if (OnSlope() && !exitingSlope)
        {
            rb.AddForce(GetSlopeMoveDirection(moveDirection) * thirdPersonMovementSpeed * 20f, ForceMode.Force);

            if (rb.linearVelocity.y > 0)
                rb.AddForce(Vector3.down * 80f, ForceMode.Force);  //adds vector3 force down so that you dont ascend to space
        }
        //When on the ground
        if (grounded)
            rb.AddForce(moveDirection.normalized * thirdPersonMovementSpeed * 10f, ForceMode.Force); //10f adds speed to make the player move a bit faster than normal
        //When in the air
        else if (!grounded)
            rb.AddForce(moveDirection.normalized * thirdPersonMovementSpeed * 10f * airMultiplier, ForceMode.Force);

        //turns gravity off when on slope
        if (!wallrunning) rb.useGravity = !OnSlope();

    }

    private void SpeedControl()
    {
        //this limits your speed on a slope so it doesnt update and elongate your speed values
        if (OnSlope() && !exitingSlope)
        {
            if(rb.linearVelocity.magnitude > thirdPersonMovementSpeed)
                rb.linearVelocity = rb.linearVelocity.normalized * thirdPersonMovementSpeed;
        }
        else //limits speed on ground and or in air
        {
            Vector3 flatVel = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

            //this if statement can limit the velocity if needed
            if (flatVel.magnitude > thirdPersonMovementSpeed)
            {
                Vector3 limitedVel = flatVel.normalized * thirdPersonMovementSpeed;
                rb.linearVelocity = new Vector3(limitedVel.x, rb.linearVelocity.y, limitedVel.z);  //basically if you go faster than you movement speed is set to in the little setup thing with the tags you calculate what your max vel would be then apply it forcefully
            }
        }
       
    }

    private void ApplyExtraGravity()
    {
        if (!OnSlope())
        {
            // fraction of how long we’ve been in the air relative to delay
            float t = Mathf.Clamp01((airTimeCounter - extraGravityDelay) / 1.5f); 
            // lerp from normal gravity (1) to fall multiplier
            float gravityMult = Mathf.Lerp(1f, fallGravityMultiplier, t);

            rb.AddForce(Physics.gravity * (gravityMult - 1f), ForceMode.Acceleration);
        }
    }

    private void Jump()
    {
        exitingSlope = true;
        //this resets the y vel
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);  //sets y to 0f

        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);

    }

    private void ResetJump()
    {
        readyToJump = true;

        exitingSlope = false;
    }

    public bool OnSlope()  //these must be public to grab from sliding script
    {
        if(Physics.Raycast(transform.position, Vector3.down, out slopeHit, playerHeight * 0.5f + 0.3f))
        {
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
            return angle < maxSlopeAngle && angle != 0f;  //f ?
        }

        return false; //if doesnt hit nothing
    }

    public Vector3 GetSlopeMoveDirection(Vector3 direction)
    {
        return Vector3.ProjectOnPlane(direction, slopeHit.normal).normalized;
    }

    

}
