using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;
using System.Collections;


public class GroundPound : MonoBehaviour
{
    [Header("References")]
    public Rigidbody rb;
    public ThirdPersonMovement tpm;

    [Header("Ground Pound Settings")]
    public float groundPoundForce = 40f;
    public float slopeBoostMultiplier = 2f;
    public float flattenScaleMultiplier = 0.6f;
    public float rayLengthMultiplier = 1.2f;

    [Header("Cooldown")]
    public float cooldown = 1.0f;
    private float cooldownTimer = 0f;
    private bool canGroundPound = true;

    private bool groundPounding = false;
    private Vector3 originalScale;

    // New Input System
    private PlayerControlsB controls;
    private bool groundPoundHeld;

    private void Awake()
    {
        controls = new PlayerControlsB();
    }

    private void OnEnable()
    {
        // Subscribe to input events
        controls.Player.GroundPound.performed += ctx => OnGroundPoundPressed();
        controls.Player.GroundPound.canceled += ctx => OnGroundPoundReleased();
        controls.Player.Enable();
    }

    private void OnDisable()
    {
        controls.Player.Disable();
    }

    private void Start()
    {
        if (rb == null) rb = GetComponent<Rigidbody>();
        if (tpm == null) tpm = GetComponent<ThirdPersonMovement>();
        originalScale = transform.localScale;
    }

    private void Update()
    {
        // Safety net to ensure ground pound was correctly reset after cooldown
        if (!canGroundPound && !groundPounding && cooldownTimer <= 0)
        {
            canGroundPound = true;
        }
        if (groundPoundHeld && !tpm.grounded && canGroundPound && !groundPounding)
        {
            StartGroundPound();
        }

        if (!groundPoundHeld && groundPounding && !tpm.grounded)
        {
            CancelGroundPound();
        }

    }

    private void FixedUpdate()
    {
        if (groundPounding && tpm.grounded)
        {
            // Give one physics frame for the slope ray to stabilize
            StartCoroutine(DelayedGroundPoundLanding());
        }
    }

    private IEnumerator DelayedGroundPoundLanding()
    {
        if (!groundPounding) yield break;

        // Wait one physics frame for slope normal to register properly
        yield return new WaitForFixedUpdate();

        // Double check that we’re still grounded
        if (groundPounding && tpm.grounded)
        {
            HandleGroundPoundLanding();
        }
    }


    private void OnGroundPoundPressed()
    {
        groundPoundHeld = true;
    }

    private void OnGroundPoundReleased()
    {
        groundPoundHeld = false;
    }

    private Coroutine cooldownRoutine;

    private void StartGroundPound()
    {
        // Prevent double activation if already pounding or on cooldown
        if (!canGroundPound || groundPounding) return;

        groundPounding = true;
        canGroundPound = false;

        // cancel upward velocity
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        rb.AddForce(Vector3.down * groundPoundForce, ForceMode.Impulse);

        // flatten
        transform.localScale = new Vector3(
            originalScale.x,
            originalScale.y * flattenScaleMultiplier,
            originalScale.z
        );
    }

    private void CancelGroundPound()
    {
        if (!groundPounding) return;

        groundPounding = false;

        // safely reset visual even mid-air
        transform.localScale = originalScale;
    }

    private void HandleGroundPoundLanding()
    {
        if (!groundPounding) return;

        groundPounding = false;
        transform.localScale = originalScale;

        // --- SLOPE BOOST ---
        if (TryFindSlopeBelow(out Vector3 slopeDir, out float slopeAngle))
        {
            Debug.Log($"Ground Pound hit slope ({slopeAngle:F1}°) — applying boosted launch!");

            Vector3 boost = slopeDir * groundPoundForce * slopeBoostMultiplier;

            // Temporarily uncap momentum and allow absurd speed
            Sliding slide = GetComponent<Sliding>();
            if (slide != null)
            {
                StartCoroutine(TemporarilyUncapMomentum(slide, 0.75f)); // duration in seconds
            }

            // Apply big impulse
            rb.AddForce(boost, ForceMode.Impulse);

            // Start a slide to convert that energy
            if (!tpm.sliding)
                slide?.StartSlideExternally();
        }

        // --- START COOLDOWN ---
        if (cooldownRoutine != null)
            StopCoroutine(cooldownRoutine);
        cooldownRoutine = StartCoroutine(GroundPoundCooldownRoutine());
    }

    private IEnumerator TemporarilyUncapMomentum(Sliding slide, float duration)
    {
        if (slide == null) yield break;

        float originalCap = slide.maxMomentumSpeed;

        // Increase cap drastically for short burst
        slide.maxMomentumSpeed = originalCap * 5f;  // or any large multiplier 
        Debug.Log($"[GroundPound] Momentum temporarily uncapped (max {slide.maxMomentumSpeed})");

        yield return new WaitForSeconds(duration);

        slide.maxMomentumSpeed = originalCap;
        Debug.Log($"[GroundPound] Momentum cap restored ({originalCap})");
    }


    private IEnumerator GroundPoundCooldownRoutine()
    {
        yield return new WaitForSeconds(cooldown);
        canGroundPound = true;
    }

    /*private void ResetCooldown()
    {
        canGroundPound = true;
    }*/

    private bool TryFindSlopeBelow(out Vector3 slopeDirection, out float slopeAngle)
    {
        slopeDirection = Vector3.zero;
        slopeAngle = 0f;

        Vector3 origin = transform.position + Vector3.up * 0.2f;
        Vector3 castDir = -rb.linearVelocity.normalized; // cast along fall direction, not world down
        float rayLen = tpm.playerHeight * 0.75f * rayLengthMultiplier;

        if (Physics.Raycast(origin, castDir, out RaycastHit hit, rayLen, tpm.whatIsTheGround))
        {
            slopeAngle = Vector3.Angle(hit.normal, Vector3.up);
            if (slopeAngle > 5f)
            {
                // Use surface tangent relative to gravity, gives cleaner direction
                slopeDirection = Vector3.ProjectOnPlane(Vector3.down, hit.normal).normalized;
                return true;
            }
        }

        return false;
    }

}
