using UnityEngine;
using UnityEngine.InputSystem;

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
    private bool slideHeld;

    private bool externallyForcedSlide;

    private void Awake()
    {
        controls = new PlayerControlsB();
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        tpm = GetComponent<NewThirdPlayerMovement>();

        if (tpm != null && orientation == null) orientation = tpm.orientation;
        if (orientation == null) orientation = transform;

        if (playerObj == null) playerObj = transform;

        startYScale = playerObj.localScale.y;
    }

    private void OnEnable()
    {
        controls.Player.Move.performed += OnMove;
        controls.Player.Move.canceled += OnMove;

        controls.Player.Slide.started += OnSlideStarted;
        controls.Player.Slide.canceled += OnSlideCanceled;

        controls.Player.Enable();
    }

    private void OnDisable()
    {
        controls.Player.Move.performed -= OnMove;
        controls.Player.Move.canceled -= OnMove;

        controls.Player.Slide.started -= OnSlideStarted;
        controls.Player.Slide.canceled -= OnSlideCanceled;

        controls.Player.Disable();
    }

    private void FixedUpdate()
    {
        if (tpm != null && tpm.sliding)
            SlidingMovement();
    }

    public void StartSlideExternal(bool resetTimer = true)
    {
        StartSlideInternal(isExternal: true, resetTimer: resetTimer);
    }

    public void StopSlideExternal()
    {
        StopSlideInternal();
    }

    private void OnMove(InputAction.CallbackContext ctx)
    {
        moveInput = ctx.ReadValue<Vector2>();
    }

    private void OnSlideStarted(InputAction.CallbackContext _)
    {
        slideHeld = true;

        if (moveInput.sqrMagnitude > 0.001f)
            StartSlideInternal(isExternal: false, resetTimer: true);
    }

    private void OnSlideCanceled(InputAction.CallbackContext _)
    {
        slideHeld = false;

        if (tpm != null && tpm.sliding && !externallyForcedSlide)
            StopSlideInternal();
    }

    private void StartSlideInternal(bool isExternal, bool resetTimer = true)
    {
        if (tpm == null) return;
        if (tpm.wallrunning) return;

        externallyForcedSlide = isExternal;
        tpm.sliding = true;

        playerObj.localScale = new Vector3(playerObj.localScale.x, slideYScale, playerObj.localScale.z);
        rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);

        if (resetTimer)
            slideTimer = maxSlideTime;
        else if (slideTimer <= 0f)
            slideTimer = maxSlideTime; // safety so it doesn't insta-end
    }

    private void SlidingMovement()
    {
        Vector3 inputDirection = orientation.forward * moveInput.y + orientation.right * moveInput.x;

        if (externallyForcedSlide && inputDirection.sqrMagnitude < 0.001f)
            inputDirection = orientation.forward;

        if (!tpm.OnSlope() || rb.linearVelocity.y > -0.1f)
        {
            rb.AddForce(inputDirection.normalized * slideForce, ForceMode.Force);

            if (slideTimer > 0f)
            {
                slideTimer -= Time.fixedDeltaTime;
                if (slideTimer <= 0f) StopSlideInternal();
            }
        }
        else
        {
            rb.AddForce(tpm.GetSlopeMoveDirection(inputDirection) * slideForce, ForceMode.Force);
            // optional: you can also tick timer here if you want slope slide to time out
        }

        if (!slideHeld && tpm.sliding && !externallyForcedSlide)
            StopSlideInternal();
    }

    private void StopSlideInternal()
    {
        if (tpm == null) return;

        tpm.sliding = false;
        externallyForcedSlide = false;

        playerObj.localScale = new Vector3(playerObj.localScale.x, startYScale, playerObj.localScale.z);
    }
}
