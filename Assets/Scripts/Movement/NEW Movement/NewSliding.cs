using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// added if(tpm.wallrunning) return;

public class NewSliding : MonoBehaviour
{
    [Header("References")]
    public Transform orientation;
    public Transform playerObj;
    private Rigidbody rb;
    private NewThirdPlayerMovement tpm;

    [Header("Sliding")]
    public float maxSlideTime;
    public float slideForce;
    private float slideTimer;

    public float slideYScale;
    private float startYScale;

    [Header("Input")]
    public KeyCode slideKey = KeyCode.C; // CHANGE THIS if crouch is LeftControl
    private float horizontalInput;
    private float verticalInput;

    private bool externallyForcedSlide; // started by something else (ground pound, etc.)

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        tpm = GetComponent<NewThirdPlayerMovement>();

        startYScale = playerObj.localScale.y;
    }

    private void Update()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        // Normal input slide start
        if (Input.GetKeyDown(slideKey) && (horizontalInput != 0 || verticalInput != 0))
            StartSlideInternal(isExternal: false);

        // Normal input slide stop (only if slide was started by key)
        if (Input.GetKeyUp(slideKey) && tpm.sliding && !externallyForcedSlide)
            StopSlideInternal();
    }

    private void FixedUpdate()
    {
        if (tpm.sliding)
            SlidingMovement();
    }

    /// <summary>
    /// Call this from other scripts (like ground pound) to start a slide
    /// without needing slideKey / input direction.
    /// </summary>
    public void StartSlideExternal(bool resetTimer = true)
    {
        StartSlideInternal(isExternal: true, resetTimer: resetTimer);
    }

    public void StopSlideExternal()
    {
        StopSlideInternal();
    }

    private void StartSlideInternal(bool isExternal, bool resetTimer = true)
    {
        if (tpm.wallrunning) return;

        externallyForcedSlide = isExternal;

        tpm.sliding = true;

        // apply slide scale
        playerObj.localScale = new Vector3(playerObj.localScale.x, slideYScale, playerObj.localScale.z);
        rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);

        if (resetTimer)
            slideTimer = maxSlideTime;
    }

    private void SlidingMovement()
    {
        Vector3 inputDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        // If externally started and no input, give it *some* direction so slope slide works instantly
        if (externallyForcedSlide && inputDirection.sqrMagnitude < 0.001f)
        {
            inputDirection = orientation.forward;
        }

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
            StopSlideInternal();
    }

    private void StopSlideInternal()
    {
        tpm.sliding = false;
        externallyForcedSlide = false;

        playerObj.localScale = new Vector3(playerObj.localScale.x, startYScale, playerObj.localScale.z);
    }
}
