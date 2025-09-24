using UnityEngine;

public class PlayerPushAssist : MonoBehaviour
{
    public float assistForce = 200f;
    public float maxAssistSpeed = 3.5f;
    public KeyCode grabKey = KeyCode.E;
    public string pushableTag = "Pushable";

    void OnCollisionStay(Collision col)
    {
        if (!Input.GetKey(grabKey)) return;
        if (!col.collider.CompareTag(pushableTag)) return;

        Rigidbody box = col.rigidbody;
        if (!box) return;

        // push along player forward on the contact plane
        Vector3 fwd = transform.forward; fwd.y = 0f; fwd.Normalize();
        Vector3 n = col.GetContact(0).normal;           // box→player
        Vector3 pushDir = Vector3.ProjectOnPlane(fwd, Vector3.up).normalized;

        Vector3 hv = new Vector3(box.linearVelocity.x, 0f, box.linearVelocity.z);
        if (hv.magnitude < maxAssistSpeed)
            box.AddForce(pushDir * assistForce * Time.deltaTime, ForceMode.VelocityChange);
    }
}
