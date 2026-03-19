using UnityEngine;

public class JumpPad : MonoBehaviour
{
    [SerializeField] private float jumpForce = 15f;
    public Animator animator;
    public SoundManager SM;
    public string JPAudio;

    void Awake()
    {
        if (SM == null)
        {
            SM = FindFirstObjectByType<SoundManager>();
            if (SM == null)
            {
                Debug.LogError("No SoundManager found in scene!");
            }
        }
    }

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

                    animator.SetTrigger("Bounce");
                if (SM != null) {SM.PlaySFX(JPAudio, 1);}
            }
        }
    }
}
