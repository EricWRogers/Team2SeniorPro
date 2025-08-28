using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Sliding : MonoBehaviour
{

    [Header("The References")]
    public Transform orientation;
    public Transform playerObj;
    private Rigidbody rb;
    private ThirdPersonMovement tpm;

    [Header("Sliding")]

    public float maxSlideTime;
    public float slideForce;
    private float slideTimer;

    public float slideYScale;
    private float startYScale;

    [Header("Inputs")]

    public KeyCode slideKey = KeyCode.LeftControl;
    private float horizontalInput;
    private float verticalInput;

    //private bool sliding;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody>();  //gets actual rigid body component for referencing
        tpm = GetComponent<ThirdPersonMovement>();

        startYScale = playerObj.transform.localScale.y;  //may not need transform

    }

    private void Update()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal"); //a and d keys
        verticalInput = Input.GetAxisRaw("Vertical"); // w and s keys

        if(Input.GetKeyDown(slideKey) && (horizontalInput != 0 || verticalInput != 0) && tpm.grounded)
             StartSlide();   //if the slide key is pressed and the movement is not just zero period begin sliding by all means necessary
        
        if(Input.GetKeyUp(slideKey) && tpm.sliding)
             StopSlide();  //stops the slide
    }

    private void FixedUpdate()
    {
        if(tpm.sliding)
             SlidingMovement();
    }

 
    private void StartSlide()  //functions for starting and stoping and handlin slidin movementn
    {
        tpm.sliding = true;

        playerObj.localScale = new Vector3(playerObj.localScale.x, slideYScale, playerObj.localScale.z);
        rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);

        slideTimer = maxSlideTime;
    }

    private void SlidingMovement()
    {
        Vector3 inputDirection = orientation.forward * verticalInput + orientation.right * horizontalInput; //allows for 360 sliding in respective directions, seems relevant
        
        //when sliding normally on flat ground NOT on slope
        if (!tpm.OnSlope() || rb.linearVelocity.y > -0.1f)
        {
            rb.AddForce(inputDirection.normalized * slideForce, ForceMode.Force); //force modes

            slideTimer -= Time.deltaTime;
        }
        
        //Specifically when sliding down a slope
        else
        {
            rb.AddForce(tpm.GetSlopeMoveDirection(inputDirection) * slideForce, ForceMode.Force);  //no timer so inf slide down slopes
            rb.AddForce(Vector3.down * 30f, ForceMode.Force); //testing to see if can fix bug
        }

        if (slideTimer <= 0)
            StopSlide();
    }

    private void StopSlide()
    {
        tpm.sliding = false;

        playerObj.localScale = new Vector3(playerObj.localScale.x, startYScale, playerObj.localScale.z); //change to startyscale aka original size of player
        rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);
    }
}
