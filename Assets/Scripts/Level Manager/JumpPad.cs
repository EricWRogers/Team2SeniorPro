using UnityEngine;

public class JumpPad : MonoBehaviour
{
    [SerializeField] private float jumpForce = 15f;

    private void OnTriggerEnter(Collider other)
{
    if (other.CompareTag("Player"))
    {
        Rigidbody rb = other.attachedRigidbody;

        if (rb != null)
        {
            Vector3 vel = rb.linearVelocity;
            vel.y = 0f;
            rb.linearVelocity = vel;

            rb.AddForce(Vector3.up * jumpForce, ForceMode.VelocityChange);
        }
    }
}
}
