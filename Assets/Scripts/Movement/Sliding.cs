using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Sliding : MonoBehaviour
{
    [Header("References")]
    public Transform orientation;
    private Rigidbody rb;
    private ThirdPersonMovement tpm;

    [Header("Slide Movement")]
    public float slideForce = 10f;      // acceleration while sliding
    public float slideYScale = 0.5f;    // crouch height while sliding
    private float startYScale;

    [Header("Momentum")]
    public float maxMomentumSpeed = 20f;          // hard cap
    public float initialSlideBonus = 2f;          // added at slide start
    public float baseDecayRate = 6f;              // always-on decay (units/sec)
    public float noInputExtraDecay = 2f;          // added when no WASD
    public float stopAtSpeedFraction = 0.15f;     // end slide when <= walkSpeed * this

    [Header("Slope Gain")]
    public float minSlopeAngleGain = 10f;         // only gain on slopes steeper than this
    public float slopeGainRate = 8f;              // gain per second on steep downhill
    public float slopeGainAngleScale = 1f;        // multiplies with angle ratio (angle / tpm.maxSlopeAngle)

    // inputs
    public KeyCode slideKey = KeyCode.LeftControl;
    private float horizontalInput;
    private float verticalInput;

    // runtime
    private float currentMomentum;
    private bool  startedThisFrame;
    private float slideStartTime;

    // expose to TPM
    public float MomentumBoost => currentMomentum;

    private void Start()
    {
        rb  = GetComponent<Rigidbody>();
        tpm = GetComponent<ThirdPersonMovement>();
        startYScale = transform.localScale.y;
    }

    private void Update()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput   = Input.GetAxisRaw("Vertical");


        if (Input.GetKeyDown(slideKey))
        {
            Vector3 flatVel = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
            bool hasVelocity = flatVel.magnitude > tpm.walkSpeed * 0.4f;
            bool hasInput    = Mathf.Abs(horizontalInput) > 0.05f || Mathf.Abs(verticalInput) > 0.05f;

            if (hasVelocity || hasInput)
                StartSlide();
        }

        // --- Stop conditions ---
        // 1) Letting go of the key, or
        // 2) Momentum fell to 0
        if (tpm.sliding)
        {
            bool released = !Input.GetKey(slideKey);
            float stopThreshold = Mathf.Max(0.1f, tpm.walkSpeed * stopAtSpeedFraction);
            bool momentumDepleted = currentMomentum <= stopThreshold;

            // tiny grace period to avoid immediate cancel on start
            bool allowStop = (Time.time - slideStartTime) > 0.08f;

            if (allowStop && (released || momentumDepleted))
                StopSlide();
        }

        // --- When NOT actively sliding: keep decaying stored momentum then apply it ---
        if (!tpm.sliding && currentMomentum > 0f)
        {
            bool hasInput = Mathf.Abs(horizontalInput) > 0.05f || Mathf.Abs(verticalInput) > 0.05f;
            float decay = baseDecayRate + (hasInput ? 0f : noInputExtraDecay);
            currentMomentum = Mathf.Max(0f, currentMomentum - decay * Time.deltaTime);

            ApplyMomentum(); // gently blend velocity toward stored momentum
        }
    }

    private void FixedUpdate()
    {
        if (tpm.sliding)
            SlidingMovement();
    }

    // ====== SLIDE LIFECYCLE ======

    public void StartSlide()
    {
        if (tpm.sliding) return;

        tpm.sliding = true;
        slideStartTime = Time.time;
        startedThisFrame = true;

        // apply crouch and a tiny downward impulse to seat on ground
        transform.localScale = new Vector3(transform.localScale.x, slideYScale, transform.localScale.z);
        rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);

        // capture initial horizontal speed and add a small bonus
        Vector3 flatVel = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        float startSpeed = flatVel.magnitude + initialSlideBonus;

        // store as momentum (do NOT overwrite every frame!)
        currentMomentum = Mathf.Clamp(startSpeed, 0f, maxMomentumSpeed);
    }

    public void StartSlideExternally()
    {
        if (!tpm.sliding)
            StartSlide();
    }

    private void StopSlide()
    {
        if (!tpm.sliding) return;

        tpm.sliding = false;

        // return to full height
        transform.localScale = new Vector3(transform.localScale.x, startYScale, transform.localScale.z);

        // keep whatever momentum remains; it will continue to decay in Update()
        // optional small settle impulse:
        rb.AddForce(Vector3.down * 3f, ForceMode.Impulse);
    }

    // ====== SLIDE PHYSICS ======

    private void SlidingMovement()
    {
        // Normalize once for consistent direction
        Vector3 inputDir = (orientation.forward * verticalInput + orientation.right * horizontalInput).normalized;

        // Apply slide acceleration (flat or slope)
        bool onSlope = tpm.OnSlope();
        float angle  = onSlope ? Vector3.Angle(Vector3.up, GetSlopeNormalSafe()) : 0f;
        bool steepEnough = onSlope && angle > minSlopeAngleGain && rb.linearVelocity.y <= 0f;

        if (steepEnough)
        {
            // push along the slope
            rb.AddForce(tpm.GetSlopeMoveDirection(inputDir) * slideForce, ForceMode.Force);

            // down-slope **builds** momentum a bit (but decay still runs below)
            float angleFactor = Mathf.Clamp01(angle / Mathf.Max(1f, tpm.maxSlopeAngle));
            AddMomentum(slopeGainRate * slopeGainAngleScale * angleFactor * Time.deltaTime);
        }
        else
        {
            // flat / gentle slope: normal input push
            rb.AddForce(inputDir * slideForce, ForceMode.Force);
        }

        // --- Always-on decay while sliding ---
        bool hasInput = Mathf.Abs(horizontalInput) > 0.05f || Mathf.Abs(verticalInput) > 0.05f;
        float decay = baseDecayRate + (hasInput ? 0f : noInputExtraDecay);
        currentMomentum = Mathf.Max(0f, currentMomentum - decay * Time.deltaTime);

        // Clamp the *resulting* horizontal speed toward stored momentum
        ClampFlatSpeedToMomentum();
        startedThisFrame = false;
    }

    private Vector3 GetSlopeNormalSafe()
    {
        // tpm.slopeHit is private there; copy of their ray done in OnSlope()
        // If you want the exact normal, expose slopeHit in TPM. For now, approximate using ground ray.
        //implement this fix later for the slope
        Physics.Raycast(transform.position, Vector3.down, out var hit, tpm.playerHeight * 0.5f + 0.3f, tpm.whatIsTheGround);
        return hit.normal != Vector3.zero ? hit.normal : Vector3.up;
    }

    // Keep the rigidbody's flat speed near the stored momentum value (without snapping)
    private void ClampFlatSpeedToMomentum()
    {
        Vector3 v = rb.linearVelocity;
        Vector3 flat = new Vector3(v.x, 0f, v.z);

        float target = Mathf.Min(currentMomentum, maxMomentumSpeed);
        if (flat.sqrMagnitude < 0.0001f) return;

        // Blend flat speed toward target magnitude, preserve direction
        float newMag = Mathf.MoveTowards(flat.magnitude, target, slideForce * Time.fixedDeltaTime);
        Vector3 newFlat = flat.normalized * newMag;

        rb.linearVelocity = new Vector3(newFlat.x, v.y, newFlat.z);
    }

    private void ApplyMomentum()
    {
        // When not sliding, gently steer the flat velocity to match currentMomentum
        Vector3 v = rb.linearVelocity;
        Vector3 flat = new Vector3(v.x, 0f, v.z);
        if (flat.sqrMagnitude < 0.0001f || currentMomentum <= 0f) return;

        float newMag = Mathf.MoveTowards(flat.magnitude, currentMomentum, slideForce * Time.deltaTime);
        Vector3 newFlat = flat.normalized * newMag;
        rb.linearVelocity = new Vector3(newFlat.x, v.y, newFlat.z);
    }

    // Public hook called from TPM on slide-jump / boosts
    public void AddMomentumFromJump(float added)
    {
        AddMomentum(added);
    }

    // Internal add with clamp (decay still applies every frame)
    private void AddMomentum(float amount)
    {
        currentMomentum = Mathf.Clamp(currentMomentum + amount, 0f, maxMomentumSpeed);
    }
}


//check how to slow down or limit the boost or where to edit this amount
//Add movement when crouching with tab.
//Fix slope downward interaction bugs
//theres something else but i cant remember
//scuffed decay when sliding, increase vals for it or make it actually make you slow down into sliding as in when you slide make it feel like its ACTUALLY slowing down over time
//from the momentum building, only take from the initla boost of momentum, so when sliding the initial amount, after that the decay should start, only if you start building momentum should it start going un, but still it should always decay as well.  This is because as of now you can infinitely slide after presing slide once.  This should not be, if you press slide once, after sliding the decay should start and if you do not keep building momentum, you should decay to 0 speed and then return to walking state.
