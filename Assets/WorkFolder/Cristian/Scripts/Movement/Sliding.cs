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

    [Header("Momentum")]
    public float maxMomentumSpeed = 20f;
    public float momentumDecayRate = 5f;         // normal decay speed
    public float momentumDecayRateNoInput = 10f; // faster decay when no inputs
    public float momentumDuration = 1.5f;        // how long momentum can persist

    private float currentMomentum;
    private float momentumTimer;

    public float MomentumBoost => currentMomentum; // expose to ThirdPersonMovement

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

        // Handle momentum decay when not sliding
        if (!tpm.sliding && currentMomentum > 0f)
        {
            if (momentumTimer > 0f)
            {
                momentumTimer -= Time.deltaTime;

                // Target is sprint speed if sprinting, otherwise walk speed
                float targetSpeed = Input.GetKey(tpm.sprintKey) ? tpm.sprintSpeed : tpm.walkSpeed;

                bool hasInput = Mathf.Abs(horizontalInput) > 0.1f || Mathf.Abs(verticalInput) > 0.1f;
                float decayRate = hasInput ? momentumDecayRate : momentumDecayRateNoInput;

                // Decay momentum down toward base speed
                currentMomentum = Mathf.MoveTowards(currentMomentum, targetSpeed, decayRate * Time.deltaTime);

                ApplyMomentum();
            }
            else
            {
                // ✅ Reset fully to walk speed once timer runs out
                currentMomentum = tpm.walkSpeed;
            }
        }
    }

    public void UpdateMomentum(float targetSpeed, bool slidingActive)
    {
        if (!slidingActive && currentMomentum > 0f)
        {
            if (momentumTimer > 0f)
            {
                momentumTimer -= Time.deltaTime;
                currentMomentum = Mathf.MoveTowards(currentMomentum, targetSpeed, momentumDecayRate * Time.deltaTime);
            }
            else
            {
                // ✅ Reset fully to walk speed once timer runs out
                currentMomentum = tpm.walkSpeed;
            }
        }

        // Safety clamp
        currentMomentum = Mathf.Min(currentMomentum, maxMomentumSpeed);
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

        // Sliding on flat ground or in midair
        if (!tpm.OnSlope() || rb.linearVelocity.y > -0.1f)
        {
            rb.AddForce(inputDirection.normalized * slideForce, ForceMode.Force);

            currentMomentum = Mathf.Min(rb.linearVelocity.magnitude, maxMomentumSpeed);
            momentumTimer = momentumDuration; // refresh duration
            slideTimer -= Time.deltaTime;
        }
        // Sliding down a slope
        else
        {
            rb.AddForce(tpm.GetSlopeMoveDirection(inputDirection) * slideForce, ForceMode.Force);

            currentMomentum = Mathf.MoveTowards(
                currentMomentum,
                Mathf.Min(rb.linearVelocity.magnitude, maxMomentumSpeed),
                slideForce * Time.deltaTime
            );
            momentumTimer = momentumDuration;
        }

        if (slideTimer <= 0)
            StopSlide();
    }

    private void StopSlide()
    {
        tpm.sliding = false;

        transform.localScale = new Vector3(transform.localScale.x, startYScale, transform.localScale.z);
        //rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);

        // Preserve momentum briefly after stopping
        currentMomentum = Mathf.MoveTowards(
            currentMomentum,
            Mathf.Min(rb.linearVelocity.magnitude, maxMomentumSpeed),
            slideForce * Time.deltaTime
        );
        momentumTimer = momentumDuration;
    }

    private void ApplyMomentum()
    {
        Vector3 flatVel = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

        if (flatVel.sqrMagnitude > 0.01f)
        {
            Vector3 momentumVel = flatVel.normalized * currentMomentum;
            rb.linearVelocity = new Vector3(momentumVel.x, rb.linearVelocity.y, momentumVel.z);
        }
    }
}
