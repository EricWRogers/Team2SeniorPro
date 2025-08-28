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

    [Header("Climbing")]
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

    //[Tooltip("")]

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
        air
    }

    public bool sliding;

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

        if(grounded)
             rb.linearDamping = groundDrag;  //damping of linear velocity where higher values increase damping
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

    private void StateHandler()
    {

        // Mode ?: Sliding
        if(Input.GetKey(slideKey))
        {
            state = MovementState.sliding;

            if(OnSlope() && rb.linearVelocity.y < 0.1f)
                 desiredMovementSpeed = slideSpeed;
            else  
                desiredMovementSpeed = sprintSpeed;
        }
        // Mode ?: Crouching
        else if(Input.GetKey(crouchKey))
        {
            state = MovementState.crouching;
            desiredMovementSpeed = crouchSpeed;
        }
        // Mode 1: Sprinting
        else if(grounded && Input.GetKey(sprintKey))
        {
            state = MovementState.sprinting;
            desiredMovementSpeed = sprintSpeed;
        }

        // Mode 2: Walking
        else if(grounded)
        {
            state = MovementState.walking;
            desiredMovementSpeed = walkSpeed;
        }

        // Mode 3: In-Air
        else 
        {
            state = MovementState.air;
            
        }
        //checks if desired move speed has changed fr
        if(Mathf.Abs(desiredMovementSpeed - lastDesiredMovementSpeed) > 4f)
        {
            StopAllCoroutines();
            StartCoroutine(SmoothlyLerpMoveSpeed());
        }
        else
        {
            thirdPersonMovementSpeed = desiredMovementSpeed;  //keep momentum
        }
        lastDesiredMovementSpeed = desiredMovementSpeed;
    }

    //to make to where the speed adjusts slowly and doesnt immediately change back to og speed

    /*private IEnumerator SmoothlyLerpMoveSpeed() //nice coroutine
    {
        //use lerp to smoothly change movement speed to desire value

        float time = 0;
        float difference = Mathf.Abs(desiredMovementSpeed - thirdPersonMovementSpeed);
        float startValue = thirdPersonMovementSpeed;

        while (time < difference)
        {
            thirdPersonMovementSpeed = Mathf.Lerp(startValue, desiredMovementSpeed, time / difference);
            time += Time.deltaTime;
            yield return null; /// RECALL !!!
        }

        thirdPersonMovementSpeed = desiredMovementSpeed;
    }*/

    private IEnumerator SmoothlyLerpMoveSpeed()
    {
        float time = 0;
        float difference = Mathf.Abs(desiredMovementSpeed - thirdPersonMovementSpeed);
        float startValue = thirdPersonMovementSpeed;

        while (time < difference)
        {
            thirdPersonMovementSpeed = Mathf.Lerp(startValue, desiredMovementSpeed, time / difference);
            
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

        thirdPersonMovementSpeed = desiredMovementSpeed;
    }

    private void MovePlayer()
    {
        //calculates players overall movement direction
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        //when on slope
        if(OnSlope() && !exitingSlope)
        {
            rb.AddForce(GetSlopeMoveDirection(moveDirection) * thirdPersonMovementSpeed * 20f, ForceMode.Force);

            if(rb.linearVelocity.y > 0)
                 rb.AddForce(Vector3.down * 80f, ForceMode.Force);  //adds vector3 force down so that you dont ascend to space
        }
        //When on the ground
        if(grounded)
            rb.AddForce(moveDirection.normalized * thirdPersonMovementSpeed * 10f, ForceMode.Force); //10f adds speed to make the player move a bit faster than normal
        //When in the air
        else if(!grounded)
            rb.AddForce(moveDirection.normalized * thirdPersonMovementSpeed * 10f * airMultiplier, ForceMode.Force);

        //turns gravity off when on slope
        rb.useGravity = !OnSlope();
   
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
