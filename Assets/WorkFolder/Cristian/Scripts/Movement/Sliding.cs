using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sliding : MonoBehaviour
{
    [Header("References")]
    public Transform orientation;
    private Rigidbody rb;
    private ThirdPersonMovement tpm;

    [Header("Sliding")]
    public float maxSlideTime;
    public float slideForce;
    private float slideTimer;

    public float slideYScale;
    private float startYScale;

    [Header("Input")]
    public KeyCode slideKey = KeyCode.LeftControl;
    private float horizontalInput;
    private float verticalInput;


    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        tpm = GetComponent<ThirdPersonMovement>();

        startYScale = transform.localScale.y;
    }

    private void Update()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        if (Input.GetKeyDown(slideKey) && (horizontalInput != 0 || verticalInput != 0))
            StartSlide();

        if (Input.GetKeyUp(slideKey) && tpm.sliding)
            StopSlide();
    }

    private void FixedUpdate()
    {
        if (tpm.sliding)
            SlidingMovement();
    }

    private void StartSlide()
    {
        tpm.sliding = true;

        transform.localScale = new Vector3(transform.localScale.x, slideYScale, transform.localScale.z);
        rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);

        slideTimer = maxSlideTime;
    }

    private void SlidingMovement()
    {
        Vector3 inputDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        // sliding normal
        if (!tpm.OnSlope() || rb.linearVelocity.y > -0.1f)
        {
            rb.AddForce(inputDirection.normalized * slideForce, ForceMode.Force);

            slideTimer -= Time.deltaTime;
        }

        // sliding down a slope
        else
        {
            rb.AddForce(tpm.GetSlopeMoveDirection(inputDirection) * slideForce, ForceMode.Force);
        }

        if (slideTimer <= 0)
            StopSlide();
    }

    private void StopSlide()
    {
        tpm.sliding = false;

        transform.localScale = new Vector3(transform.localScale.x, startYScale, transform.localScale.z);
        //rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);
    }

    //after sliding keep momentum of the sliding and put it into a time which then slowly goes down like done when going down a slope, character needs to not slow down abruptly after sliding or jumping instead the speed that is gained in momentum slowly goes down, so if you slide jump you can gain speed but it can also slowly go down too so you keep the momentum while moving and lose slowly at all times, increase gravity after jumping
}
