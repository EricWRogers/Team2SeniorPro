/*using UnityEngine;

public class Sliding : MonoBehaviour
{
    [Header("References")]
    public Transform orientation;
    public Transform playerObj;
    private Rigidbody rb;
    private ThirdPersonMovement tpm;

    [Header("Sliding")]
    public float maxSlideTime;  // still run a timer, but momentum can outlive it 2f
    public float slideForce; //18
    private float slideTimer;

    public float slideYScale  = 0.6f;
    private float startYScale;

    [Header("Momentum")]
    public float maxMomentumSpeed;  // soft cap 30 rn
    public float momentumDecayRate;   // toward 0 over time 4
    public float momentumDuration; // refresh window for chaining 1.5
    public float momentumNoInputBonus;   // set >0 if you want faster decay w/o input 

    private float currentMomentum;
    private float momentumTimer;

    public float MomentumBoost => currentMomentum;

    private float h, v;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        tpm = GetComponent<ThirdPersonMovement>();
        startYScale = playerObj != null ? playerObj.localScale.y : 1f;
    }

    private void Update()
    {
        h = Input.GetAxisRaw("Horizontal");
        v = Input.GetAxisRaw("Vertical");

        // Always decay momentum toward 0 when not being refreshed
        if (currentMomentum > 0f)
        {
            if (momentumTimer > 0f)
            {
                momentumTimer -= Time.deltaTime;

                // optional: slightly faster decay if no input (set momentumNoInputBonus > 0 if this ends up being what I do)
                bool hasInput = Mathf.Abs(h) > 0.1f || Mathf.Abs(v) > 0.1f;
                float decay = momentumDecayRate + (hasInput ? 0f : momentumNoInputBonus);
                currentMomentum = Mathf.MoveTowards(currentMomentum, 0f, decay * Time.deltaTime);
            }
            else
            {
                currentMomentum = Mathf.MoveTowards(currentMomentum, 0f, momentumDecayRate * Time.deltaTime);
            }
        }

        // If sliding and momentum effectively gone then stop sliding and go to crouch
        if (tpm.sliding && currentMomentum <= 0.05f)
        {
            StopSlide();           // exit sliding state
            tpm.crouching = true;  // fall back to crouch
            if (playerObj) playerObj.localScale = new Vector3(playerObj.localScale.x, slideYScale, playerObj.localScale.z);
        }
    }

    private void FixedUpdate()
    {
        if (tpm.sliding)
            SlidingMovement();
    }

    public void StartSlide()
    {
        if (tpm.sliding) return;
        tpm.sliding = true;
        tpm.crouching = false;

        if (playerObj) playerObj.localScale = new Vector3(playerObj.localScale.x, slideYScale, playerObj.localScale.z);
        rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);

        slideTimer = maxSlideTime;

        // send momentum from current horizontal speed
        Vector3 flat = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        currentMomentum = Mathf.Clamp(flat.magnitude, 0f, maxMomentumSpeed);
        momentumTimer = momentumDuration;
    }

    public void StopSlide()
    {
        if (!tpm.sliding) return;

        tpm.sliding = false;

        // return to crouch scale (TPM will exit crouch when Ctrl released)
        if (playerObj)
        {
            playerObj.localScale = new Vector3(playerObj.localScale.x, tpm != null ? tpm.crouchYScale : startYScale, playerObj.localScale.z);
        }

        // preserve whatever momentum remains; keep decaying in Update()
        momentumTimer = Mathf.Max(momentumTimer, 0.15f);
    }

    // Allow the TPM glide to increase momentum while airborne
    public void AddMomentum(float delta, float refresh)
    {
        currentMomentum = Mathf.Clamp(currentMomentum + delta, 0f, maxMomentumSpeed);
        momentumTimer   = Mathf.Max(momentumTimer, refresh);
    }

    // ---- Internals ----
    private void SlidingMovement()
    {
        Vector3 inputDirection = orientation.forward * v + orientation.right * h;

        // baseline push from slide
        Vector3 pushDir;
        if (!tpm.OnSlope() || rb.linearVelocity.y > -0.1f)
            pushDir = inputDirection.normalized;
        else
            pushDir = tpm.GetSlopeMoveDirection(inputDirection);

        rb.AddForce(pushDir * slideForce, ForceMode.Force);

        // update momentum from current speed (toward cap)
        Vector3 flatVel = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        float target = Mathf.Min(flatVel.magnitude, maxMomentumSpeed);
        currentMomentum = Mathf.MoveTowards(currentMomentum, target, slideForce * Time.fixedDeltaTime);

        // refresh momentum window to allow chaining while sliding like megabonk
        momentumTimer = momentumDuration;

        // optional legacy timer (doesn't hard-stop slide as momentum determines stop)
        slideTimer -= Time.fixedDeltaTime;
        // This forces stop after time:
        // if (slideTimer <= 0f) StopSlide();
    }
}
*/
/*
using UnityEngine;

public class Sliding : MonoBehaviour
{
    [Header("References")]
    public Transform orientation;
    public Transform playerObj;
    private Rigidbody rb;
    private ThirdPersonMovement tpm;

    [Header("Sliding")]
    public float slideForce = 10f;
    public float slideYScale = 0.5f;
    public float maxMomentumSpeed = 25f;
    public float momentumDecayRate = 4f;
    public float momentumDuration = 2.5f;

    [HideInInspector] public bool sliding;

    private float startYScale;
    private float currentMomentum;
    private float momentumTimer;

    public float MomentumBoost => currentMomentum;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        tpm = GetComponent<ThirdPersonMovement>();
        startYScale = playerObj.localScale.y;
    }

    private void FixedUpdate()
    {
        if (sliding) HandleSliding();
        else DecayMomentum();
    }

    public void StartSlideExternally()
    {
        sliding = true;
        playerObj.localScale = new Vector3(playerObj.localScale.x, slideYScale, playerObj.localScale.z);
        rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);

        // Initial push
        currentMomentum = Mathf.Min(rb.linearVelocity.magnitude + slideForce, maxMomentumSpeed);
        momentumTimer = momentumDuration;
    }

    private void HandleSliding()
    {
        Vector3 inputDir = (tpm.orientation.forward * Input.GetAxisRaw("Vertical") +
                            tpm.orientation.right * Input.GetAxisRaw("Horizontal")).normalized;

        rb.AddForce(inputDir * slideForce, ForceMode.Force);
        currentMomentum = Mathf.Min(rb.linearVelocity.magnitude, maxMomentumSpeed);

        momentumTimer -= Time.deltaTime;

        // Auto stop when timer expires or player stops moving
        if (momentumTimer <= 0f || currentMomentum <= tpm.walkSpeed + 0.1f)
            StopSlide();
    }

    public void RefreshMomentumFromAirBoost()
    {
        currentMomentum = Mathf.Min(currentMomentum + 3f, maxMomentumSpeed);
        momentumTimer = momentumDuration; // refresh
    }

    private void StopSlide()
    {
        sliding = false;
        playerObj.localScale = new Vector3(playerObj.localScale.x, startYScale, playerObj.localScale.z);
    }

    private void DecayMomentum()
    {
        if (currentMomentum > tpm.walkSpeed)
        {
            currentMomentum = Mathf.MoveTowards(currentMomentum, tpm.walkSpeed, momentumDecayRate * Time.deltaTime);
            ApplyMomentum();
        }
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
*/

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

        if (!tpm.sliding && currentMomentum > 0f)
        {
            if (momentumTimer > 0f)
            {
                momentumTimer -= Time.deltaTime;

                float targetSpeed = Input.GetKey(tpm.sprintKey) ? tpm.sprintSpeed : tpm.walkSpeed;
                bool hasInput = Mathf.Abs(horizontalInput) > 0.1f || Mathf.Abs(verticalInput) > 0.1f;
                float decayRate = hasInput ? momentumDecayRate : momentumDecayRateNoInput;

                currentMomentum = Mathf.MoveTowards(currentMomentum, targetSpeed, decayRate * Time.deltaTime);
                ApplyMomentum();
            }
            else
            {
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
                currentMomentum = tpm.walkSpeed;
            }
        }

        currentMomentum = Mathf.Min(currentMomentum, maxMomentumSpeed);
    }

    private void FixedUpdate()
    {
        if (tpm.sliding)
            SlidingMovement();
    }

    //gp logic stuff called externally
    public void StartSlide()
    {
        tpm.sliding = true;

        transform.localScale = new Vector3(transform.localScale.x, slideYScale, transform.localScale.z);
        rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);

        slideTimer = maxSlideTime;
    }

    public void StartSlideExternally()
    {
        if (!tpm.sliding)
            StartSlide();
    }

    private void SlidingMovement()
    {
        Vector3 inputDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        if (!tpm.OnSlope() || rb.linearVelocity.y > -0.1f)
        {
            rb.AddForce(inputDirection.normalized * slideForce, ForceMode.Force);

            currentMomentum = Mathf.Min(rb.linearVelocity.magnitude, maxMomentumSpeed);
            momentumTimer = momentumDuration;
            slideTimer -= Time.deltaTime;
        }
        else
        {
            rb.AddForce(tpm.GetSlopeMoveDirection(inputDirection) * slideForce, ForceMode.Force);
            currentMomentum = Mathf.MoveTowards(currentMomentum,
                Mathf.Min(rb.linearVelocity.magnitude, maxMomentumSpeed),
                slideForce * Time.deltaTime);
            momentumTimer = momentumDuration;
        }

        // Slide naturally ends when the timer expires
        if (slideTimer <= 0)
            StopSlide();
    }

    private void StopSlide()
    {
        if (!tpm.sliding) return;

        tpm.sliding = false;
        transform.localScale = new Vector3(transform.localScale.x, startYScale, transform.localScale.z);
        rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);

        currentMomentum = Mathf.MoveTowards(
            currentMomentum,
            Mathf.Min(rb.linearVelocity.magnitude, maxMomentumSpeed),
            slideForce * Time.deltaTime);

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
