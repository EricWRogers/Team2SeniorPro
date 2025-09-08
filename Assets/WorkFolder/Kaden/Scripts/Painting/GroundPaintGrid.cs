using UnityEngine;
using System.Collections.Generic;

public class GroundPaintGrid : MonoBehaviour
{
    public static GroundPaintGrid Instance { get; private set; }

    [Header("Scope")]
    public Transform roomRoot;          // parent that holds all spawned chunks
    public LayerMask groundMask;        // Ground layer
    public float cellSize = 0.5f;       // grid resolution (meters)
    public bool useLifetime = true;
    public float defaultSafeLifetime = 8f;

    // computed from roomRoot at rebuild
    Bounds roomBounds;
    Vector3 originXZ;

    // key -> expireTime (Time.time + lifetime), or +Infinity for permanent
    readonly Dictionary<long, float> safeCells = new();

    void Awake()
    {
        if (Instance && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // --- Public API ---

    public void RebuildBounds()
    {
        roomBounds = CalculateBounds(roomRoot, groundMask);
        originXZ = new Vector3(roomBounds.min.x, 0f, roomBounds.min.z);
        safeCells.Clear();
    }

    public void ClearAll() => safeCells.Clear();

    public void MarkCircle(Vector3 worldPos, float radius, float lifetime = -1f)
    {
        if (lifetime < 0f) lifetime = defaultSafeLifetime;
        int cx = WX(worldPos.x), cz = WZ(worldPos.z);
        int r = Mathf.CeilToInt(radius / cellSize);
        float expire = useLifetime ? Time.time + lifetime : float.PositiveInfinity;

        for (int dz = -r; dz <= r; dz++)
        {
            for (int dx = -r; dx <= r; dx++)
            {
                if (dx * dx + dz * dz > r * r) continue;
                long k = Key(cx + dx, cz + dz);
                if (!safeCells.TryGetValue(k, out float old) || expire > old)
                    safeCells[k] = expire;
            }
        }
    }

    public bool IsSafe(Vector3 worldPos)
    {
        long k = Key(WX(worldPos.x), WZ(worldPos.z));
        if (safeCells.TryGetValue(k, out float t))
        {
            if (!useLifetime || t >= Time.time) return true;
            safeCells.Remove(k); // expired
        }
        return false;
    }

    // --- Helpers ---

    int WX(float x) => Mathf.FloorToInt((x - originXZ.x) / cellSize);
    int WZ(float z) => Mathf.FloorToInt((z - originXZ.z) / cellSize);
    static long Key(int x, int z) => ((long)x << 32) ^ (uint)z;

    static Bounds CalculateBounds(Transform root, LayerMask groundMask)
    {
        var renderers = root.GetComponentsInChildren<Renderer>(false);
        bool init = false;
        Bounds b = new Bounds(root.position, Vector3.zero);
        foreach (var r in renderers)
        {
            if (((1 << r.gameObject.layer) & groundMask) == 0) continue;
            if (!init) { b = r.bounds; init = true; } else b.Encapsulate(r.bounds);
        }
        if (!init) b = new Bounds(root.position, new Vector3(100, 10, 100));
        return b;
    }
}
