using UnityEngine;
using UnityEngine.InputSystem;

public class NewDashing : MonoBehaviour
{
    [Header("References")]
    public Transform orientation;
    public Transform playerCam;

    private Rigidbody rb;
    private NewThirdPlayerMovement tpm;
    private NewSliding slidingScript;

    [Header("Dashing")]
    public float dashForce = 70f;
    public float dashUpwardForce = 2f;
    public float dashDuration = 0.4f;

    [Header("Settings")]
    public bool useCameraForward = true;
    public bool allowAllDirections = true;
    public bool disableGravity = false;

    [Tooltip("If true, grounded dashes reset Y velocity before dashing.")]
    public bool resetYVelOnGround = true;

    [Tooltip("If true, air dashes keep current Y velocity instead of pausing upward/falling momentum.")]
    public bool preserveAirYMomentum = true;

    [Tooltip("If true, grounded/sliding dashes clear X/Z velocity so slide momentum does not stack.")]
    public bool resetFlatVelocityOnGround = true;

    [Tooltip("If true, air dashes keep current X/Z velocity and add dash force on top.")]
    public bool preserveAirFlatMomentum = true;

    [Header("Cooldown")]
    public float dashCd = 1.5f;
    private float dashCdTimer;

    private PlayerControlsB controls;
    private Vector2 moveInput;
    private bool dashPressedThisFrame;

    private Vector3 delayedForceToApply;

    private void Awake()
    {
        controls = new PlayerControlsB();
    }

    private void Start()
    {
        if (orientation == null) orientation = transform;
        if (playerCam == null && Camera.main != null) playerCam = Camera.main.transform;

        rb = GetComponent<Rigidbody>();
        tpm = GetComponent<NewThirdPlayerMovement>();
        slidingScript = GetComponent<NewSliding>();
    }

    private void OnEnable()
    {
        controls.Player.Move.performed += OnMove;
        controls.Player.Move.canceled += OnMove;
        controls.Player.Dash.started += OnDashStarted;
        controls.Player.Enable();
    }

    private void OnDisable()
    {
        controls.Player.Move.performed -= OnMove;
        controls.Player.Move.canceled -= OnMove;
        controls.Player.Dash.started -= OnDashStarted;
        controls.Player.Disable();

        CancelInvoke();
        dashPressedThisFrame = false;

        if (tpm != null)
        {
            tpm.dashing = false;
            tpm.restricted = false;
        }

        if (rb != null && disableGravity)
            rb.useGravity = true;
    }

    private void Update()
    {
        if (dashCdTimer > 0f)
            dashCdTimer -= Time.deltaTime;

        if (dashPressedThisFrame)
        {
            dashPressedThisFrame = false;
            TryDash();
        }
    }

    private void OnMove(InputAction.CallbackContext ctx)
    {
        moveInput = ctx.ReadValue<Vector2>();
    }

    private void OnDashStarted(InputAction.CallbackContext _)
    {
        dashPressedThisFrame = true;
    }

    private void TryDash()
    {
        if (dashCdTimer > 0f) return;
        dashCdTimer = dashCd;

        CancelInvoke(nameof(DelayedDashForce));
        CancelInvoke(nameof(ResetDash));

        bool isGrounded = tpm != null && tpm.grounded;
        bool isAirDash = !isGrounded;

        // Only stop sliding when grounded/sliding.
        // Air dash should not kill momentum.
        if (!isAirDash && slidingScript != null)
            slidingScript.StopSlideExternal();

        if (tpm != null)
        {
            tpm.dashing = true;
            tpm.restricted = false;
        }

        Transform forwardT = (useCameraForward && playerCam != null) ? playerCam : orientation;
        Vector3 direction = GetDirection(forwardT);

        if (disableGravity)
            rb.useGravity = false;

        Vector3 currentVel = rb.linearVelocity;

        float newX = currentVel.x;
        float newY = currentVel.y;
        float newZ = currentVel.z;

        if (isAirDash)
        {
            if (!preserveAirFlatMomentum)
            {
                newX = 0f;
                newZ = 0f;
            }

            if (!preserveAirYMomentum)
                newY = 0f;
        }
        else
        {
            if (resetFlatVelocityOnGround)
            {
                newX = 0f;
                newZ = 0f;
            }

            if (resetYVelOnGround)
                newY = 0f;
        }

        rb.linearVelocity = new Vector3(newX, newY, newZ);

        delayedForceToApply = direction * dashForce + orientation.up * dashUpwardForce;

        Invoke(nameof(DelayedDashForce), 0.025f);
        Invoke(nameof(ResetDash), dashDuration);
    }

    private void DelayedDashForce()
    {
        rb.AddForce(delayedForceToApply, ForceMode.Impulse);
    }

    private void ResetDash()
    {
        if (tpm != null)
        {
            tpm.dashing = false;
            tpm.restricted = false;
        }

        if (disableGravity)
            rb.useGravity = true;
    }

    private Vector3 GetDirection(Transform forwardT)
    {
        float x = moveInput.x;
        float z = moveInput.y;

        Vector3 dir = allowAllDirections
            ? forwardT.forward * z + forwardT.right * x
            : forwardT.forward;

        if (Mathf.Abs(x) < 0.001f && Mathf.Abs(z) < 0.001f)
            dir = forwardT.forward;

        dir.y = 0f;
        return dir.normalized;
    }
}