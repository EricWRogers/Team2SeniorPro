using UnityEngine;
using UnityEngine.InputSystem;

public class NewDashing : MonoBehaviour
{
    [Header("References")]
    public Transform orientation;
    public Transform playerCam;

    private Rigidbody rb;
    private NewThirdPlayerMovement tpm;

    [Header("Dashing")]
    public float dashForce = 70f;
    public float dashUpwardForce = 2f;
    public float dashDuration = 0.4f;

    [Header("Settings")]
    public bool useCameraForward = true;
    public bool allowAllDirections = true;
    public bool disableGravity = false;
    public bool resetYVel = true;

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

        // safety cleanup
        CancelInvoke();
        dashPressedThisFrame = false;

        if (tpm != null)
        {
            tpm.dashing = false;
            tpm.restricted = false; // only matters if you use restricted elsewhere
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

    private void OnMove(InputAction.CallbackContext ctx) => moveInput = ctx.ReadValue<Vector2>();
    private void OnDashStarted(InputAction.CallbackContext _) => dashPressedThisFrame = true;

    private void TryDash()
    {
        if (dashCdTimer > 0f) return;
        dashCdTimer = dashCd;

        CancelInvoke(nameof(DelayedDashForce));
        CancelInvoke(nameof(ResetDash));

        if (tpm != null)
        {
            tpm.dashing = true;     // <-- important
            // tpm.restricted = true; // optional (usually not needed if MovePlayer/SpeedControl bail on dashing)
        }

        Transform forwardT = (useCameraForward && playerCam != null) ? playerCam : orientation;
        Vector3 direction = GetDirection(forwardT);

        Vector3 forceToApply = direction * dashForce + orientation.up * dashUpwardForce;

        if (disableGravity) rb.useGravity = false;

        if (resetYVel)
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

        delayedForceToApply = forceToApply;
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

        if (disableGravity) rb.useGravity = true;
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

        dir.y = 0f; // optional: keeps dash flat, remove if you want dash to follow camera pitch
        return dir.normalized;
    }
}
