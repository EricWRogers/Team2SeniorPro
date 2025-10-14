using UnityEngine;

public class PushableBox : MonoBehaviour
{
    [Header("Grab & Control")]
    public string playerTag = "Player";
    public bool requireGrabKey = true;
    public KeyCode grabKey = KeyCode.E;

    [Tooltip("Acceleration applied while dragging (m/s^2).")]
    public float dragAcceleration = 12f;

    [Tooltip("Horizontal speed cap while being dragged (m/s).")]
    public float maxDragSpeed = 3.5f;

    [Tooltip("Keeps the box in contact with the player so you can pull it.")]
    public float contactStickForce = 20f;

    [Tooltip("Desired player↔box distance to maintain while dragging (m).")]
    public float desiredContactDistance = 0.7f;

    [Tooltip("How ‘square’ your inputs need to be before we treat as directional intent.")]
    public float inputDeadzone = 0.1f;

    Rigidbody rb;
    Transform grabber;         
    bool touchingGrabber;      

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        
        rb.constraints |= RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
    }

    void FixedUpdate()
    {

        bool wantGrab = !requireGrabKey || Input.GetKey(grabKey);
        if (!grabber || !touchingGrabber || !wantGrab)
        {
           
            grabber = null;
            touchingGrabber = false;
            return;
        }

        // World-space input relative to the player's facing
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        Vector2 iv = new Vector2(h, v);

        // Direction we want to drag
        Vector3 dragDir = Vector3.zero;
        if (iv.sqrMagnitude > inputDeadzone * inputDeadzone)
        {
            dragDir = (grabber.forward * v + grabber.right * h);
            dragDir.y = 0f;
            dragDir.Normalize();
        }

        // Apply acceleration in any direction (push, pull, strafe)
        Vector3 hv = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        if (dragDir != Vector3.zero && hv.magnitude < maxDragSpeed)
        {
            rb.AddForce(dragDir * dragAcceleration, ForceMode.Acceleration);
        }

        // Gentle spring toward the player to keep contact for pulling
        Vector3 toGrabber = grabber.position - transform.position;
        toGrabber.y = 0f;
        float dist = toGrabber.magnitude;
        if (dist > desiredContactDistance)
        {
            Vector3 keepContactDir = toGrabber.normalized;
            rb.AddForce(keepContactDir * contactStickForce, ForceMode.Acceleration);
        }

       
        touchingGrabber = false;
    }

    void OnCollisionStay(Collision col)
    {
        if (!col.collider.CompareTag(playerTag)) return;

        // Only drag while key held (if required)
        if (requireGrabKey && !Input.GetKey(grabKey)) return;

        grabber = col.collider.transform;
        touchingGrabber = true;
    }

    void OnCollisionExit(Collision col)
    {
        if (grabber && col.collider.transform == grabber)
        {
            touchingGrabber = false;
            grabber = null;
        }
    }
}
