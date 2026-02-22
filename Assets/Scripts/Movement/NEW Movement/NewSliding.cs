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

    [Header("Uphill vs Downhill")]
    public float uphillDecel = 20f;
    public float uphillBrakeForceMult = 0.8f;
    public float stopSpeed = 0.25f;

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

        // If externally started and no input, give a default direction
        if (externallyForcedSlide && inputDirection.sqrMagnitude < 0.001f)
            inputDirection = orientation.forward;

        bool onSlope = tpm != null && tpm.OnSlope();

//NOT ON SLOPE REGULAR
        if (!onSlope)
        {
            if (inputDirection.sqrMagnitude > 0.001f)
                rb.AddForce(inputDirection.normalized * slideForce, ForceMode.Force);

            slideTimer -= Time.fixedDeltaTime;

            if (slideTimer <= 0f)
                StopSlideInternal();

            if (!slideHeld && tpm.sliding && !externallyForcedSlide)
                StopSlideInternal();

            return;
        }

        /*public Vector3 GetSlopeDownDirection(Vector3 surfaceNormal)
        {
            Vector3 rightOnPlane = Vector3.Cross(Vector3.up, surfaceNormal);
            Vector3 forwardOnPlane = Vector3.Cross(rightOnPlane, surfaceNormal);
            return forwardOnPlane.normalized;
        }*/
        
        //when ON SLOPE BELOW
        Vector3 normal = tpm.CurrentSlopeNormal;

        // downhill direction along slope plane
        Vector3 downhill = Vector3.ProjectOnPlane(Vector3.down, normal).normalized;

        // velocity along slope plane
        Vector3 planarVel = Vector3.ProjectOnPlane(rb.linearVelocity, normal);

        // input along slope plane
        Vector3 inputOnSlope = Vector3.ProjectOnPlane(inputDirection, normal);
        if (inputOnSlope.sqrMagnitude > 0.0001f) inputOnSlope.Normalize();

        // + = moving downhill, - = moving uphill
        float velDotDownhill = (planarVel.sqrMagnitude > 0.0001f) ? Vector3.Dot(planarVel.normalized, downhill) : 0f;

        // + = input wants downhill, - = input wants uphill
        float inputDotDownhill = (inputOnSlope.sqrMagnitude > 0.0001f) ? Vector3.Dot(inputOnSlope, downhill) : 0f;

        // If you are actually moving uphill OR player is trying to push uphill cut speed.
        bool movingUphill = velDotDownhill < -0.05f;
        bool inputUphill = inputDotDownhill < -0.05f;

        if (movingUphill || inputUphill)
        {
            // Brake opposite the current planar motion (only if we have planar motion)
            if (planarVel.sqrMagnitude > 0.0001f)
            {
                rb.AddForce(-planarVel.normalized * slideForce * uphillBrakeForceMult, ForceMode.Force);

                float newMag = Mathf.MoveTowards(planarVel.magnitude, 0f, uphillDecel * Time.fixedDeltaTime);
                Vector3 newPlanar = planarVel.normalized * newMag;

                // keep component into normal (so you don't pop off the slope)
                Vector3 intoNormal = Vector3.Project(rb.linearVelocity, normal);
                rb.linearVelocity = newPlanar + intoNormal;
            }

            // stop if basically zero momentum on the slope
            if (planarVel.magnitude <= stopSpeed)
                StopSlideInternal();
        }
        else
        {
            // DOWNHILL / NEUTRAL: push downhill (prefer input if it's meaningful, else just downhill)
            Vector3 pushDir = (inputOnSlope.sqrMagnitude > 0.0001f && inputDotDownhill > 0.05f)
                ? inputOnSlope
                : downhill;

            rb.AddForce(pushDir * slideForce, ForceMode.Force);
        }

        // + = moving downhill, - = moving uphill
        float downhillSpeed = Vector3.Dot(planarVel, downhill);
        bool movingDownhill = downhillSpeed > 0.1f; // small threshold to avoid jitter

        // NEW Timer behavior: FAHHHHHHHHHHHHHHHHHHHHH
        // on downhill slope: DO NOT drain timer (keeps sliding)
        // otherwise: drain timer N O R M A L
        if (!movingDownhill)
        {
            slideTimer -= Time.fixedDeltaTime;
            if (slideTimer <= 0f)
            {
                StopSlideInternal();
                return;
            }
        }
        else
        {
            //keep timer alive so it never ends immediately after leaving the slope
            slideTimer = Mathf.Max(slideTimer, 0.15f);
        }

         //End on release if this was not externally forced
        if (!slideHeld && tpm.sliding && !externallyForcedSlide)
            StopSlideInternal();

        //if (!slideHeld && tpm.sliding && !externallyForcedSlide && !movingDownhill)
        //StopSlideInternal();
    }

    private void StopSlideInternal()
    {
        if (tpm == null) return;

        tpm.sliding = false;
        externallyForcedSlide = false;

        playerObj.localScale = new Vector3(playerObj.localScale.x, startYScale, playerObj.localScale.z);
    }
}
