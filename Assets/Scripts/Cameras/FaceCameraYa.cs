using Unity.Mathematics;
using UnityEngine;

public class FaceCameraYa : MonoBehaviour
{
    public Transform cameraTransform;
    public float turnSpeed = 12f;
    public bool onlyWhenMoving = false; // set true to rotate only on input
    public quaternion target;

    void Update()
    {
        if (!cameraTransform) { var cam = Camera.main; if (cam) cameraTransform = cam.transform; }

        if (onlyWhenMoving)
        {
            float h = Input.GetAxisRaw("Horizontal");
            float v = Input.GetAxisRaw("Vertical");
            if (new Vector2(h, v).sqrMagnitude < 0.01f) return;
        }

        Vector3 yawForward = cameraTransform.forward;
        yawForward.y = 0f;
        if (yawForward.sqrMagnitude < 0.0001f) return;

        Quaternion target = Quaternion.LookRotation(yawForward.normalized, Vector3.up);
        
    }
    void FixedUpdate()
    {
        transform.rotation = Quaternion.RotateTowards(transform.rotation, target, turnSpeed * Time.deltaTime);
    }
}
