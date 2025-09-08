using UnityEngine;

public class SafePad : MonoBehaviour
{
    [Tooltip("How far out (meters) we mark the ground as safe.")]
    public float radius = 2f;

    [Tooltip("If > 0, the pad will follow the player for this many seconds, marking as it goes.")]
    public float followSeconds = 0f;

    [Tooltip("Player to follow while 'followSeconds' > 0.")]
    public Transform followTarget;

    [Tooltip("Layers considered 'ground' for Y snapping. Leave 0 to skip snap.")]
    public LayerMask groundMask;

    [Tooltip("Lift the decal a tiny bit to avoid z-fighting.")]
    public float yOffset = 0.02f;

    float _t;

    void OnEnable()
    {
        _t = followSeconds;
        // Place at correct Y and mark immediately
        if (followTarget) transform.position = SnapToGround(followTarget.position);
        MarkHere();
    }

    void LateUpdate()
    {
        if (_t > 0f && followTarget)
        {
            _t -= Time.unscaledDeltaTime; // unaffected by pause
            transform.position = SnapToGround(followTarget.position);
            MarkHere();
        }
    }

    Vector3 SnapToGround(Vector3 p)
    {
        if (groundMask.value != 0 &&
            Physics.Raycast(p + Vector3.up * 3f, Vector3.down, out var hit, 10f, groundMask, QueryTriggerInteraction.Ignore))
        {
            p.y = hit.point.y + yOffset;
        }
        return p;
    }

    void MarkHere()
    {
        var grid = GroundPaintGrid.Instance;
        if (grid)
        {
            // Infinite duration = safe for the entire room
            grid.MarkCircle(transform.position, radius, float.PositiveInfinity);
        }
    }
}
