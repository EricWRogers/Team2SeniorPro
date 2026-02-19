using UnityEngine;

public class NewSliding : MonoBehaviour
{
    [Header("References")]
    public Transform orientation;
    public Transform playerObj;
    private Rigidbody rb;
    private NewThirdPlayerMovement tpm;

    [Header("Sliding")]
    public float maxSlideTime = 0.75f;
    public float slideForce = 400f;
    private float slideTimer;

    public float slideYScale = 0.5f;
    private float startYScale;

    [Header("Input (New Input System)")]
    private PlayerControlsB controls;
    private Vector2 moveInput;
    private bool slideHeld; // for "hold-to-slide" behavior

    private bool externallyForcedSlide; // started by something else (ground pound, etc.)

    private void Awake()
    {
        controls = new PlayerControlsB();
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        tpm = GetComponent<NewThirdPlayerMovement>();

        startYScale = playerObj.localScale.y;
    }

    private void OnEnable()
    {
        controls.Player.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        controls.Player.Move.canceled += ctx => moveInput = Vector2.zero;

        controls.Player.Slide.started += ctx =>
        {
            slideHeld = true;

            // Only start slide if player is trying to move (same logic you had)
            if (moveInput.sqrMagnitude > 0.001f)
                StartSlideInternal(isExternal: false);
        };

        controls.Player.Slide.canceled += ctx =>
        {
            slideHeld = false;

            // Only stop if this slide was started by the key (not external)
            if (tpm.sliding && !externallyForcedSlide)
                StopSlideInternal();
        };

        controls.Player.Enable();
    }

    private void OnDisable()
    {
        controls.Player.Disable();
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
        Vector3 inputDirection = orientation.forward * moveInput.y + orientation.right * moveInput.x;

        // If externally started and no input, give it some direction so slope slide works instantly
        if (externallyForcedSlide && inputDirection.sqrMagnitude < 0.001f)
            inputDirection = orientation.forward;

        // sliding normal
        if (!tpm.OnSlope() || rb.linearVelocity.y > -0.1f)
        {
            rb.AddForce(inputDirection.normalized * slideForce, ForceMode.Force);
            slideTimer -= Time.fixedDeltaTime;
        }
        // sliding down a slope
        else
        {
            rb.AddForce(tpm.GetSlopeMoveDirection(inputDirection) * slideForce, ForceMode.Force);
        }

        // End conditions
        if (slideTimer <= 0f)
            StopSlideInternal();

        // Optional: if you want hold-to-slide, end when released (only for non-external slides)
        // (Your old code ended on key up, so this matches.)
        if (!slideHeld && tpm.sliding && !externallyForcedSlide)
            StopSlideInternal();
    }

    private void StopSlideInternal()
    {
        tpm.sliding = false;
        externallyForcedSlide = false;

        playerObj.localScale = new Vector3(playerObj.localScale.x, startYScale, playerObj.localScale.z);
    }
}
