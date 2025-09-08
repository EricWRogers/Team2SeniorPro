using UnityEngine;

public class PigmentPickup : MonoBehaviour
{
    public int amount = 1;
    public float groundOffsetY = 0.05f;
    public LayerMask groundMask;

    void Start() { SnapToGround(); }

    void OnTriggerEnter(Collider other)
    {
        var cur = other.GetComponentInParent<PlayerCurrency>() ?? other.GetComponent<PlayerCurrency>();
        if (cur)
        {
            cur.AddPigment(amount);
            Destroy(gameObject);
        }
    }

    void SnapToGround()
    {
        Vector3 from = transform.position + Vector3.up * 2f;
        if (Physics.Raycast(from, Vector3.down, out var hit, 10f, groundMask))
            transform.position = hit.point + Vector3.up * groundOffsetY;
    }
}
