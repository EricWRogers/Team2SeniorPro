using UnityEngine;

public class PushableBox : MonoBehaviour
{
    public string playerTag = "Player";
    public float pushAcceleration = 10f;   
    public float maxPushSpeed = 3.0f;      // horizontal cap
    public float minDotToPush = 0.6f;      
    public bool requireGrabKey = true;     //hold E to push 
    public KeyCode grabKey = KeyCode.E;

    Rigidbody rb;

    void Awake() { rb = GetComponent<Rigidbody>(); }

    void OnCollisionStay(Collision col)
    {
        if (!col.collider.CompareTag(playerTag)) return;

        
        if (requireGrabKey && !Input.GetKey(grabKey)) return;

        // Only push if player is pressing forward
        float vertical = Input.GetAxisRaw("Vertical");
        if (vertical <= 0f) return;

        // Determine if player is actually pushing into the face
        Transform playerT = col.collider.transform;
        Vector3 playerForward = playerT.forward; playerForward.y = 0f; playerForward.Normalize();

       
        Vector3 n = col.GetContact(0).normal; // box→player
        Vector3 pushDir = Vector3.ProjectOnPlane(-n, Vector3.up).normalized; // towards box, horizontal

        // Must be facing mostly into the box
        if (Vector3.Dot(playerForward, pushDir) < minDotToPush) return;

        // Clamp horizontal speed and apply acceleration
        Vector3 v = rb.linearVelocity; Vector3 hv = new Vector3(v.x, 0f, v.z);
        if (hv.magnitude < maxPushSpeed)
            rb.AddForce(pushDir * pushAcceleration, ForceMode.Acceleration);
    }
}
