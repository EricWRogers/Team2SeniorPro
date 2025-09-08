using UnityEngine;
public class ContactKnockback : MonoBehaviour
{
    public float damagePerHit = 10f;
    public float knockbackForce = 8f;
    public float hitCooldown = 0.8f;

    float lastHitTime = -999f;

    void OnTriggerStay(Collider other)
    {
        if (Time.time < lastHitTime + hitCooldown) return;

        var paint = other.GetComponentInParent<PaintResource>() ?? other.GetComponent<PaintResource>();
        var rb = other.GetComponentInParent<Rigidbody>() ?? other.GetComponent<Rigidbody>();
        if (!paint || !rb) return;

        // Damage
        paint.Damage(damagePerHit);


        Vector3 dir = (other.transform.position - transform.position);
        dir.y = 0f;
        if (dir.sqrMagnitude > 0.0001f)
        {
            dir.Normalize();
            rb.AddForce(dir * knockbackForce, ForceMode.VelocityChange);
        }

        lastHitTime = Time.time;
    }
    
}
