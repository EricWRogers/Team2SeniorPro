using UnityEngine;
using UnityEngine.InputSystem;

public class Sliding : MonoBehaviour
{
    [Header("References")]
    public Transform orientation;
    private Rigidbody rb;
    private ThirdPersonMovement tpm;

    [Header("Slide Movement")]
    public float slideForce = 10f;
    public float slideYScale = 0.5f;
    private float startYScale;

    [Header("Momentum")]
    public float maxMomentumSpeed = 20f;
    public float initialSlideBonus = 2f;
    public float baseDecayRate = 6f;
    public float noInputExtraDecay = 2f;
    public float stopAtSpeedFraction = 0.15f;
    public float momentumDecayRate = 5f;
    public float momentumDecayRateNoInput = 10f;
    public float momentumDuration = 1.5f;
    private float momentumTimer;

    [Header("Slope Gain")]
    public float minSlopeAngleGain = 10f;
    public float slopeGainRate = 8f;
    public float slopeGainAngleScale = 1f;

    // runtime
    private float currentMomentum;
    private bool startedThisFrame;
    private float slideStartTime;

    // inputs
    private PlayerControlsB controls;
    private Vector2 moveInput;
    private bool slidePressed;
    private bool slideHeld;

    // expose to TPM
    public float MomentumBoost => currentMomentum;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        tpm = GetComponent<ThirdPersonMovement>();

        controls = new PlayerControlsB();

        // Movement input
        controls.Player.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        controls.Player.Move.canceled += ctx => moveInput = Vector2.zero;

        // Slide input
        controls.Player.CrouchSlide.performed += ctx =>
        {
            slidePressed = true;
            slideHeld = true;
        };
        controls.Player.CrouchSlide.canceled += ctx => slideHeld = false;
    }

    private void OnEnable() => controls.Player.Enable();
    private void OnDisable() => controls.Player.Disable();

    private void Start()
    {
        startYScale = transform.localScale.y;

        /*// Safety reset: make sure sliding is stopped if scene is reloaded while sliding  //Check this for bugs
        if (tpm.sliding)
        {
            StopSlide();
        }

        // Reset input flags in case they were stuck from previous scene
        slidePressed = false;
        slideHeld = false;
        currentMomentum = 0f;

        // Reset scale
        transform.localScale = new Vector3(
            transform.localScale.x,
            startYScale,
            transform.localScale.z
        );*/
    }

    private void Update()
    {
        float horizontalInput = moveInput.x;
        float verticalInput = moveInput.y;

        Vector3 flatVel = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        bool hasVelocity = flatVel.magnitude > tpm.walkSpeed * 0.4f;
        bool hasInput = Mathf.Abs(horizontalInput) > 0.05f || Mathf.Abs(verticalInput) > 0.05f;

        // --- Slide Start ---
        if (slidePressed)
        {
            slidePressed = false; // consume event
            if (hasVelocity || hasInput)
                StartSlide();
        }

        // --- Slide Stop ---
        if (tpm.sliding)
        {
            bool released = !slideHeld;
            float stopThreshold = Mathf.Max(0.1f, tpm.walkSpeed * stopAtSpeedFraction);
            bool momentumDepleted = currentMomentum <= stopThreshold;
            bool allowStop = (Time.time - slideStartTime) > 0.08f;

            if (allowStop && (released || momentumDepleted))
                StopSlide();
        }

        // --- Momentum Decay ---
        if (!tpm.sliding)
        {
            hasInput = Mathf.Abs(horizontalInput) > 0.1f || Mathf.Abs(verticalInput) > 0.1f;

            // Stop momentum completely if no input
            if (!hasInput)
            {
                currentMomentum = 0f;
                return;
            }

            // Gradual decay
            if (currentMomentum > 0f)
            {
                momentumTimer = Mathf.Max(0f, momentumTimer - Time.deltaTime);

                float decayRate = hasInput ? momentumDecayRate : momentumDecayRateNoInput;
                currentMomentum = Mathf.Max(0f, currentMomentum - decayRate * Time.deltaTime);

                ApplyMomentum();
            }
            else
            {
                currentMomentum = 0f;
            }
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
        momentumTimer = momentumDuration;

        // Scale down character for slide crouch
        transform.localScale = new Vector3(transform.localScale.x, slideYScale, transform.localScale.z);

        // Small downward push (works even midair)
        rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);

        // Capture initial horizontal speed
        Vector3 flatVel = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        float startSpeed = flatVel.magnitude + initialSlideBonus;

        currentMomentum = Mathf.Clamp(startSpeed, 0f, maxMomentumSpeed);
    }

    public void StartSlideExternally()
    {
        if (!tpm.sliding)
            StartSlide();
    }

    public void StopSlideExternally() => StopSlide();

    private void StopSlide()
    {
        if (!tpm.sliding) return;
        tpm.sliding = false;

        transform.localScale = new Vector3(transform.localScale.x, startYScale, transform.localScale.z);
        rb.AddForce(Vector3.down * 3f, ForceMode.Impulse);
    }

    // ====== SLIDE PHYSICS ======

    private void SlidingMovement()
    {
        float horizontalInput = moveInput.x;
        float verticalInput = moveInput.y;

        Vector3 inputDir = (orientation.forward * verticalInput + orientation.right * horizontalInput).normalized;

        bool onSlope = tpm.OnSlope();
        float angle = onSlope ? Vector3.Angle(Vector3.up, GetSlopeNormalSafe()) : 0f;
        bool steepEnough = onSlope && angle > minSlopeAngleGain && rb.linearVelocity.y <= 0f;

        if (steepEnough)
        {
            rb.AddForce(tpm.GetSlopeMoveDirection(inputDir) * slideForce, ForceMode.Force);
            float angleFactor = Mathf.Clamp01(angle / Mathf.Max(1f, tpm.maxSlopeAngle));
            AddMomentum(slopeGainRate * slopeGainAngleScale * angleFactor * Time.deltaTime);
        }
        else
        {
            rb.AddForce(inputDir * slideForce, ForceMode.Force);
        }

        // Always-on decay
        bool hasInput = Mathf.Abs(horizontalInput) > 0.05f || Mathf.Abs(verticalInput) > 0.05f;
        float decay = baseDecayRate + (hasInput ? 0f : noInputExtraDecay);
        currentMomentum = Mathf.Max(0f, currentMomentum - decay * Time.deltaTime);

        ClampFlatSpeedToMomentum();
        startedThisFrame = false;
    }

    private Vector3 GetSlopeNormalSafe()
    {
        Physics.Raycast(transform.position, Vector3.down, out var hit, tpm.playerHeight * 0.5f + 0.3f, tpm.whatIsTheGround);
        return hit.normal != Vector3.zero ? hit.normal : Vector3.up;
    }

    private void ClampFlatSpeedToMomentum()
    {
        Vector3 v = rb.linearVelocity;
        Vector3 flat = new Vector3(v.x, 0f, v.z);

        float target = Mathf.Min(currentMomentum, maxMomentumSpeed);
        if (flat.sqrMagnitude < 0.0001f) return;

        float newMag = Mathf.MoveTowards(flat.magnitude, target, slideForce * Time.fixedDeltaTime);
        Vector3 newFlat = flat.normalized * newMag;

        rb.linearVelocity = new Vector3(newFlat.x, v.y, newFlat.z);
    }

    private void ApplyMomentum()
    {
        Vector3 v = rb.linearVelocity;
        Vector3 flat = new Vector3(v.x, 0f, v.z);
        if (flat.sqrMagnitude < 0.0001f || currentMomentum <= 0f) return;

        float newMag = Mathf.MoveTowards(flat.magnitude, currentMomentum, slideForce * Time.deltaTime);
        Vector3 newFlat = flat.normalized * newMag;
        rb.linearVelocity = new Vector3(newFlat.x, v.y, newFlat.z);
    }

    public void AddMomentumFromJump(float added) => AddMomentum(added);

    private void AddMomentum(float amount)
    {
        currentMomentum = Mathf.Clamp(currentMomentum + amount, 0f, maxMomentumSpeed);
    }

    public void ResetSlideState() //Imma have to string these through msot scripts ig BECAUSE NEW INPUT SUCSK
    {
        currentMomentum = 0f;
        slidePressed = false;
        slideHeld = false;
        momentumTimer = 0f;
        startedThisFrame = false;
        slideStartTime = 0f;
        tpm.sliding = false;
        transform.localScale = new Vector3(
            transform.localScale.x,
            startYScale,
            transform.localScale.z
        );
    }   
}
