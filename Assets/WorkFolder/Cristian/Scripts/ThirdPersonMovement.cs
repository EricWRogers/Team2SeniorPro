using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ThirdPersonMovement : MonoBehaviour
{
    [Header("Movement")]

    public float thirdPersonMovementSpeed;

    public float groundDrag;

    public float jumpForce;
    public float jumpCooldown;
    public float airMultiplier;

    bool readyToJump;

    [Header("Keybinds")]

    public KeyCode jumpKey = KeyCode.Space;

    [Header("Ground Check")]

    public float playerHeight;

    public LayerMask whatIsTheGround;

    bool grounded;

    //[Tooltip("")]

    public Transform orientation;

    float horizontalInput;
    float verticalInput;

    Vector3 moveDirection;

    Rigidbody rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

    }

    private void Update()
    {
        //ground check will cast raycast down to see if the ground exist 
        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, whatIsTheGround);

        MyInput();
        SpeedControl();

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
    }

    private void MovePlayer()
    {
        //calculates players overall movement direction
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;
        //When on the ground
        if(grounded)
            rb.AddForce(moveDirection.normalized * thirdPersonMovementSpeed * 10f, ForceMode.Force); //10f adds speed to make the player move a bit faster than normal
        //When in the air
        else if (!grounded)
            rb.AddForce(moveDirection.normalized * thirdPersonMovementSpeed * 10f * airMultiplier, ForceMode.Force);
    }

    private void SpeedControl()
    {
        Vector3 flatVel = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

        //this if statement can limit the velocity if needed
        if (flatVel.magnitude > thirdPersonMovementSpeed)
        {
            Vector3 limitedVel = flatVel.normalized * thirdPersonMovementSpeed;
            rb.linearVelocity = new Vector3(limitedVel.x, rb.linearVelocity.y, limitedVel.z);  //basically if you go faster than you movement speed is set to in the little setup thing with the tags you calculate what your max vel would be then apply it forcefully
        }
    }

    private void Jump()
    {
        //this resets the y vel
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);  //sets y to 0f

        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);

    }

    private void ResetJump()
    {
        readyToJump = true;
    }
}
