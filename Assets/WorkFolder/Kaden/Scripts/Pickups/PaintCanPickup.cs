using UnityEngine;

public class PaintCanPickup : MonoBehaviour
{
    public float restoreAmount = 25f;
    public float groundOffsetY = 0.05f;
    public LayerMask groundMask;

    void Start() { SnapToGround(); }

    void OnTriggerEnter(Collider other)
    {
        var pr = other.GetComponentInParent<PaintResource>() ?? other.GetComponent<PaintResource>();
        if (pr)
        {
            pr.AddPaint(restoreAmount);
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
