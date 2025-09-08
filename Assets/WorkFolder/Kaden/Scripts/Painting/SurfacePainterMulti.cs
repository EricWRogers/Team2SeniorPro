using UnityEngine;

public class SurfacePainterMulti : MonoBehaviour
{
    [Header("Aim & Spray")]
    public Camera cam;
    public Transform nozzle;
    public float maxSprayDistance = 15f;
    public LayerMask paintMask = ~0;       

    [Header("Brush (mask painting)")]
    public Texture2D brushTexture;         
    [Range(0.05f, 20f)] public float brushSizePercent = 3.5f;

    [Tooltip("Scale brush by renderer size so huge meshes don’t get giant strokes.")]
    public bool scaleBrushByRendererBounds = true;

    [Header("Paint resource")]
    public PaintResource paint;            // assign from Player
    public float paintCostPerSecond = 8f;  // drain while spraying

    [Header("Enemy damage")]
    public float enemyDps = 15f;
    public float enemyConeRange = 3.0f;
    [Range(1f, 90f)] public float enemyConeAngle = 30f;
    public LayerMask enemyMask;

    [Header("Ground safe discs (walkable)")]
    public GameObject safeDiscPrefab;      
    public float safeDiscRadius = 0.7f;
    public float safeDiscLifetime = 8f;
    public LayerMask groundMask;

    [Header("Ground marking (logic)")]
    public float groundSafeRadius = 0.7f;
    public float groundMarkInterval = 0.05f;   // throttle
    float _nextMarkTime;

    [Header("Aim")]
    public bool rotateNozzle = true;
    public float nozzleTurnSpeed = 20f;

    // runtime state
    Renderer activeRenderer;
    RenderTexture activeMask;
    PaintableGroup activeGroup;
    public bool IsSpraying { get; private set; }

    void Reset() { if (!cam) cam = Camera.main; }

    void Update()
    {
        IsSpraying = false;
        if (!Input.GetMouseButton(0)) return;
        if (!cam || !nozzle) return;

        // resource cost
        if (paint && !paint.TrySpend(paintCostPerSecond * Time.deltaTime))
            return;

        IsSpraying = true;
        DamageEnemiesCone(nozzle.position, nozzle.forward);

        // aim
        Ray mouseRay = cam.ScreenPointToRay(Input.mousePosition);
        if (!Physics.Raycast(mouseRay, out RaycastHit aimHit, 100f, paintMask))
            return;

        Vector3 dir = (aimHit.point - nozzle.position).normalized;

        if (rotateNozzle && nozzle)   //make paint spray in mouse direction
        {
            Vector3 look = (aimHit.point - nozzle.position);
            look.y = 0f; // keep level; remove if you want full 3D aim
            if (look.sqrMagnitude > 0.0001f)
            {
                Quaternion targetRot = Quaternion.LookRotation(look.normalized);
                nozzle.rotation = Quaternion.Slerp(nozzle.rotation, targetRot, Time.deltaTime * nozzleTurnSpeed);
            }
        }

        // spray ray
        var hits = Physics.RaycastAll(nozzle.position, dir, maxSprayDistance, paintMask);
        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));


        foreach (var h in hits)
        {
            var mc = h.collider as MeshCollider;
            var rend = h.collider.GetComponent<Renderer>();
            if (!mc || !rend) continue;

            // assign target
            if (rend != activeRenderer)
            {
                activeRenderer = rend;
                activeGroup = rend.GetComponentInParent<PaintableGroup>();
                activeMask = null;
                if (activeGroup) activeGroup.TryGetMask(rend, out activeMask);
            }

            // paint the mask
            if (activeMask) PaintAtUV(activeMask, h.textureCoord, rend);


            TryMarkGround(h);

            break; // only the nearest valid mesh
        }
    }

    void PaintAtUV(RenderTexture maskRT, Vector2 uv, Renderer rend)
    {
        if (!brushTexture) return;
        RenderTexture prev = RenderTexture.active;
        RenderTexture.active = maskRT;

        GL.PushMatrix();
        GL.LoadPixelMatrix(0, maskRT.width, maskRT.height, 0);

        float px = maskRT.width  * uv.x;
        float py = maskRT.height * (1f - uv.y);

        float brushPx = Mathf.Max(2f, maskRT.width * (brushSizePercent / 100f));
        if (scaleBrushByRendererBounds && rend)
        {
            // crude downscale for large meshes
            float largest = Mathf.Max(rend.bounds.size.x, rend.bounds.size.z, rend.bounds.size.y);
            brushPx *= Mathf.Clamp01(1f / Mathf.Max(largest, 0.001f)); // larger object -> smaller brush
            brushPx = Mathf.Clamp(brushPx, 1.5f, maskRT.width * 0.1f);
        }

        Rect rect = new Rect(px - brushPx * 0.5f, py - brushPx * 0.5f, brushPx, brushPx);
        Graphics.DrawTexture(rect, brushTexture);
        GL.PopMatrix();

        RenderTexture.active = prev;
    }

    void TryMarkGround(RaycastHit h)
    {
        var grid = GroundPaintGrid.Instance;
        if (!grid) return;
        if (Time.time < _nextMarkTime) return;
        _nextMarkTime = Time.time + groundMarkInterval;

        // If we hit ground directly
        if (((1 << h.collider.gameObject.layer) & grid.groundMask) != 0)
        {
            grid.MarkCircle(h.point, groundSafeRadius);
            return;
        }

        
        if (Physics.Raycast(h.point + Vector3.up * 2f, Vector3.down, out var down, 4f, grid.groundMask))
            grid.MarkCircle(down.point, groundSafeRadius);
    }


    void DamageEnemiesCone(Vector3 origin, Vector3 fwd)
    {
        if (enemyDps <= 0f) return;

        // the front half-cone
        Vector3 center = origin + fwd.normalized * (enemyConeRange * 0.5f);
        var hits = Physics.OverlapSphere(center, enemyConeRange, enemyMask, QueryTriggerInteraction.Collide);

        float cosLimit = Mathf.Cos(enemyConeAngle * Mathf.Deg2Rad * 0.5f);
        foreach (var c in hits)
        {
            var t = c.transform;
            Vector3 to = (t.position - origin); to.y = 0f;
            Vector3 nfwd = fwd; nfwd.y = 0f; nfwd.Normalize();

            if (to.sqrMagnitude < 0.0001f) continue;
            Vector3 nto = to.normalized;
            if (Vector3.Dot(nfwd, nto) < cosLimit) continue; // outside cone

            var hp = c.GetComponentInParent<InkBlotHealth>() ?? c.GetComponent<InkBlotHealth>();
            if (hp) hp.TakeSpray(enemyDps * Time.deltaTime);
        }
    }

    void SpawnSafeDisc(Vector3 point)
    {
        var disc = Instantiate(safeDiscPrefab, point + Vector3.up * 0.02f, Quaternion.identity);
        disc.transform.localScale = Vector3.one * (safeDiscRadius * 2f); // diameter
        if (safeDiscLifetime > 0f) Destroy(disc, safeDiscLifetime);
    }
}