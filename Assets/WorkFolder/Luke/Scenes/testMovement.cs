using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class testMovement : MonoBehaviour
{
    public float acceleration = 20f;
    public float maxSpeed = 5f;
    public float deceleration = 40f;
    public float jumpForce = 7f;
    public float wallJumpForce = 7f;
    public float wallJumpSideForce = 5f;
    public float groundCheckDistance = 0.1f;
    public float wallCheckDistance = 0.5f;
    public LayerMask groundMask;
    public LayerMask wallMask;

    private Rigidbody rb;
    private bool isGrounded;
    private bool isTouchingWall;
    private Vector3 wallNormal;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true; // Prevents unwanted rotation
    }

    void Update()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        Vector3 input = new Vector3(h, 0, v).normalized;

        if (input.magnitude > 0)
        {
            rb.AddForce(input * acceleration, ForceMode.Acceleration);

            Vector3 horizontalVelocity = rb.linearVelocity;
            horizontalVelocity.y = 0;
            if (horizontalVelocity.magnitude > maxSpeed)
            {
                horizontalVelocity = horizontalVelocity.normalized * maxSpeed;
                rb.linearVelocity = new Vector3(horizontalVelocity.x, rb.linearVelocity.y, horizontalVelocity.z);
            }
        }
        else
        {
            Vector3 horizontalVelocity = rb.linearVelocity;
            horizontalVelocity.y = 0;
            horizontalVelocity = Vector3.MoveTowards(horizontalVelocity, Vector3.zero, deceleration * Time.deltaTime);
            rb.linearVelocity = new Vector3(horizontalVelocity.x, rb.linearVelocity.y, horizontalVelocity.z);
        }

        // Ground check
        isGrounded = Physics.Raycast(transform.position, Vector3.down,
            GetComponent<Collider>().bounds.extents.y + groundCheckDistance, groundMask);

        // Wall check (left and right)
        isTouchingWall = false;
        wallNormal = Vector3.zero;
        RaycastHit hit;
        Vector3[] directions = { transform.right, -transform.right, transform.forward, -transform.forward };
        foreach (var dir in directions)
        {
            if (Physics.Raycast(transform.position, dir, out hit, wallCheckDistance, wallMask))
            {
                isTouchingWall = true;
                wallNormal = hit.normal;
                break;
            }
        }

        // Jump
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (isGrounded)
            {
                rb.AddForce(Vector3.up * jumpForce, ForceMode.VelocityChange);
            }
            else if (isTouchingWall)
            {
                // Wall jump: up and away from wall
                Vector3 jumpDir = (Vector3.up * wallJumpForce) + (wallNormal * wallJumpSideForce);
                rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z); // Reset Y velocity
                rb.AddForce(jumpDir, ForceMode.VelocityChange);
            }
        }
    }
}