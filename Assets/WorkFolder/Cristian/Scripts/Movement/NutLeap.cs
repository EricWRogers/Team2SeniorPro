using UnityEngine;

public class MidairGrabAbility : MonoBehaviour
{
    [Header("Settings")]
    public float slowMoTime = 2f;
    public float slowMoScale = 0.3f;
    public float doubleJumpForce = 10f;

    private bool canDoubleJump = false;
    private bool isSlowingTime = false;
    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        // Jump input for double jump
        if (Input.GetKeyDown(KeyCode.Space) && !IsGrounded() && canDoubleJump)
        {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z); // reset y velocity
            rb.AddForce(Vector3.up * doubleJumpForce, ForceMode.Impulse);
            canDoubleJump = false; // use it once
            ResetTime();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Throwable") && !IsGrounded())
        {
            // Grab midair
            GrabObject(other.gameObject);
            ActivateSlowMo();
            canDoubleJump = true;
        }
    }

    void GrabObject(GameObject obj)
    {
        // Your existing grab logic goes here
        Debug.Log("Grabbed object midair!");
    }

    void ActivateSlowMo()
    {
        if (!isSlowingTime)
        {
            Time.timeScale = slowMoScale;
            isSlowingTime = true;
            Invoke(nameof(ResetTime), slowMoTime);
        }
    }

    void ResetTime()
    {
        Time.timeScale = 1f;
        isSlowingTime = false;
    }

    bool IsGrounded()
    {
        // simple raycast down
        return Physics.Raycast(transform.position, Vector3.down, 1.1f);
    }
}
