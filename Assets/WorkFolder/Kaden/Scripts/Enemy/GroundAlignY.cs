using UnityEngine;

public class GroundAlignY : MonoBehaviour
{
    public float rayUp = 2f, rayDown = 10f;
    public LayerMask groundMask;
    public float footOffsetY = 0.05f;

    void LateUpdate()
    {
        Vector3 from = transform.position + Vector3.up * rayUp;
        if (Physics.Raycast(from, Vector3.down, out var hit, rayDown + rayUp, groundMask))
        {
            var p = transform.position;
            p.y = hit.point.y + footOffsetY;
            transform.position = p;
        }
    }
}
