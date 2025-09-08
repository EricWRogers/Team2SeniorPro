using UnityEngine;

public class CardinalMovement : MonoBehaviour 
{
    [Header("Move")]
    public float moveSpeed = 5f;

    [Header("Camera")]
    public Transform cameraTransform;

    private Rigidbody rb;
    private Animator anim;
    private SpriteRenderer spriteRenderer;


    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        anim = GetComponentInChildren<Animator>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.constraints = RigidbodyConstraints.FreezeRotation; // lock rotation
    }

    void FixedUpdate()
    {
        // Input
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        Vector2 input = Vector2.ClampMagnitude(new Vector2(h, v), 1f);


        Vector3 fwd = Vector3.forward;
        Vector3 right = Vector3.right;

        if (cameraTransform != null)
        {
            Vector3 camFwd = cameraTransform.forward; camFwd.y = 0f; camFwd.Normalize();
            Vector3 camRight = cameraTransform.right; camRight.y = 0f; camRight.Normalize();
            fwd = camFwd; right = camRight;
        }


        Vector3 desiredPlanarVel = (right * input.x + fwd * input.y) * moveSpeed;


        Vector3 vel = rb.linearVelocity;
        vel.x = desiredPlanarVel.x;
        vel.z = desiredPlanarVel.z;

        // stop tiny drift when no input
        if (input.sqrMagnitude < 0.0001f)
        {
            vel.x = 0f;
            vel.z = 0f;
        }

        rb.linearVelocity = vel;
        anim.SetBool("IsWalking", input.sqrMagnitude > 0.0001f);

        if (h > 0.1f)
        {
            spriteRenderer.flipX = false; // Facing Right
        }
        else if (h < -0.1f)
        {
            spriteRenderer.flipX = true; // Facing Left
        }
    }
}