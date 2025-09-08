using UnityEngine;

public class PlayerGroundDamage : MonoBehaviour
{
    public float dpsOnWhite = 10f;
    public LayerMask hazardGroundMask;  // Ground
    public float rayUp = 0.5f, rayDown = 2.0f;

    PaintResource paint;

    void Awake() { paint = GetComponent<PaintResource>(); }

    void Update()
    {
        Vector3 from = transform.position + Vector3.up * rayUp;

        // Only tick damage if we're actually over ground
        bool overGround = Physics.Raycast(from, Vector3.down, out var hit, rayDown + rayUp, hazardGroundMask);
        if (!overGround) return;

        // Safe if the grid says so
        var grid = GroundPaintGrid.Instance;
        bool safe = grid && grid.IsSafe(transform.position);

        if (!safe)
            paint.Damage(dpsOnWhite * Time.deltaTime);
    }
}
