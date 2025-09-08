using UnityEngine;

public class InkBlotChasing : MonoBehaviour
{
    public Transform target;            // assign Player or maybe auto find
    public float speed = 2.0f;

    public float chaseRadius = 10f;
    public float contactDamagePerSec = 12f;

    Rigidbody rb;
    PaintResource targetPaint;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
    }

    void Start()
    {
        if (!target) target = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (target) targetPaint = target.GetComponent<PaintResource>();
    }

    void Update()
    {
        if (!target) return;
         // Calculate the squared distance to the target
    Vector3 toTarget = target.position - transform.position;
    float distanceToTargetSqr = toTarget.sqrMagnitude;

    // Check if the target is within the chase radius
    if (distanceToTargetSqr <= chaseRadius * chaseRadius)
    {
        // Move towards the target
        toTarget.y = 0f; // Keep movement on the horizontal plane
        transform.position += toTarget.normalized * speed * Time.deltaTime;
    }
    }

    void OnTriggerStay(Collider other)
    {
        if (targetPaint != null && other.transform == target)
        {
            targetPaint.Damage(contactDamagePerSec * Time.deltaTime);
        }
    }
}
