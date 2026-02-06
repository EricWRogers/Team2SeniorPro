using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerGravityScaler : MonoBehaviour
{
    [Header("Overall Gravity")]
    [Tooltip("1 = normal gravity. >1 = heavier. <1 = floatier.")]
    [Range(0.1f, 5f)]
    public float gravityScale = 2.2f;

    [Tooltip("Max downward speed (positive number). 0 = no limit.")]
    public float terminalFallSpeed = 0f;

    [Tooltip("Disable gravity when grounded on slope (match your movement logic).")]
    public bool disableGravityOnSlope = true;

    private Rigidbody rb;
    private NewThirdPlayerMovement movement;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        movement = GetComponent<NewThirdPlayerMovement>();
    }

    private void FixedUpdate()
    {
        // Decide whether gravity should apply
        bool applyGravity = true;

        if (movement != null)
        {
            if (movement.wallrunning)
                applyGravity = false;

            if (disableGravityOnSlope && movement.grounded && movement.OnSlope())
                applyGravity = false;
        }

        if (!applyGravity)
            return;

        // Apply scaled gravity
        Vector3 gravity = Physics.gravity * gravityScale;
        rb.AddForce(gravity, ForceMode.Acceleration);

        // Optional terminal velocity
        if (terminalFallSpeed > 0f && rb.linearVelocity.y < -terminalFallSpeed)
        {
            Vector3 v = rb.linearVelocity;
            v.y = -terminalFallSpeed;
            rb.linearVelocity = v;
        }
    }
}
