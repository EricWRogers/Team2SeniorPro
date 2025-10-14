using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.InputSystem;

public class TestMove3D : MonoBehaviour
{
    public Vector3 movementVel;
    public float forwardSpeed;
    public Rigidbody rb;
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        //parentTransform = GetComponentInParent();
    }

    void Update()
    {
        if (Input.GetKeyDown("w"))
        {
            rb.linearVelocity = Vector3.forward * forwardSpeed;
        }
        if (Input.GetKeyDown("a"))
        {
            rb.linearVelocity = Vector3.left * forwardSpeed;
        }
        if (Input.GetKeyDown("d"))
        {
            rb.linearVelocity = Vector3.right * forwardSpeed;
        }
        if (Input.GetKeyDown("s"))
        {
            rb.linearVelocity = Vector3.back * forwardSpeed;
        }
        //Camera.main.transform.rotation.x = Input.mousePositionDelta.x;
    }
    void FixedUpdate()
    {
        
    }
}
