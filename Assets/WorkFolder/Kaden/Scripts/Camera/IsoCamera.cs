using UnityEngine;

public class IsoCamera : MonoBehaviour
{
    [Header("Target")]
    public Transform target;
    public string targetTag = "Player";
    public bool detachFromParentOnEnable = true;
    public float rebindInterval = 0.5f;

    [Header("Rig")]
    [Range(10f, 80f)] public float pitch = 33f;
    [Range(0f, 360f)] public float yaw = 0f;
    public float distance = 12f;
    public Vector3 targetOffset = Vector3.zero;
    public float followDamp = 10f;          // higher = quicker
    public float snapIfFartherThan = 25f;   // snap on big teleports (room change)

    [Header("Clamp (optional)")]
    public bool clampToBounds = false;
    public Vector2 xzMin = new Vector2(-100, -100);
    public Vector2 xzMax = new Vector2( 100,  100);

    float _nextRebind;

    void OnEnable()
    {
        if (detachFromParentOnEnable) transform.SetParent(null, true);
        TryRebind(true);
    }

    void LateUpdate()
    {
        // Auto-rebind if target missing/disabled
        if (!target || !target.gameObject.activeInHierarchy)
        {
            if (Application.isPlaying && Time.time >= _nextRebind) TryRebind(false);
            if (!target) return;
        }

        // Compute desired
        Quaternion rot = Quaternion.Euler(pitch, yaw, 0f);
        Vector3 focus = target.position + targetOffset;
        Vector3 desiredPos = focus - (rot * Vector3.forward) * distance;

        // Clamp (optional)
        if (clampToBounds)
        {
            desiredPos.x = Mathf.Clamp(desiredPos.x, xzMin.x, xzMax.x);
            desiredPos.z = Mathf.Clamp(desiredPos.z, xzMin.y, xzMax.y);
        }

        // Snap on large jumps; otherwise smooth
        if ((transform.position - desiredPos).sqrMagnitude > snapIfFartherThan * snapIfFartherThan)
        {
            transform.position = desiredPos;
        }
        else
        {
            float t = 1f - Mathf.Exp(-followDamp * Time.deltaTime);
            transform.position = Vector3.Lerp(transform.position, desiredPos, t);
        }

        // Look at focus
        Quaternion look = Quaternion.LookRotation((focus - transform.position).normalized, Vector3.up);
        float rt = 1f - Mathf.Exp(-followDamp * Time.deltaTime);
        transform.rotation = Quaternion.Slerp(transform.rotation, look, rt);
    }

    void TryRebind(bool snap)
    {
        var go = GameObject.FindGameObjectWithTag(targetTag);
        if (go) target = go.transform;
        _nextRebind = Application.isPlaying ? Time.time + rebindInterval : 0f;

        if (snap && target)
        {
            // place immediately at the correct spot 
            Quaternion rot = Quaternion.Euler(pitch, yaw, 0f);
            Vector3 focus = target.position + targetOffset;
            Vector3 desiredPos = focus - (rot * Vector3.forward) * distance;
            transform.position = desiredPos;
            transform.rotation = Quaternion.LookRotation((focus - transform.position).normalized, Vector3.up);
        }
    }

    
    public void SetClampFromRoot(Transform roomRoot, LayerMask includeLayers, float padding = 5f)
    {
        if (!roomRoot) { clampToBounds = false; return; }

        var rends = roomRoot.GetComponentsInChildren<Renderer>(false);
        bool init = false;
        Bounds b = new Bounds(roomRoot.position, Vector3.zero);
        foreach (var r in rends)
        {
            if (((1 << r.gameObject.layer) & includeLayers) == 0) continue;
            if (!init) { b = r.bounds; init = true; } else b.Encapsulate(r.bounds);
        }
        if (!init) { clampToBounds = false; return; }

        xzMin = new Vector2(b.min.x - padding, b.min.z - padding);
        xzMax = new Vector2(b.max.x + padding, b.max.z + padding);
        clampToBounds = true;
    }
}
