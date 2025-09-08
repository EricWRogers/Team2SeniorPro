using UnityEngine;

public class SurfacePainter : MonoBehaviour
{
    [Header("Refs")]
    public Camera cam;                // assign Main Camera
    public Transform nozzle;          // the spray origin (a child at head/hand height)
    
    [Header("Brush")]
    public Texture2D brushTexture;    // black circle w/ soft alpha (PNG)
    [Range(0.1f, 50f)] public float brushSizePercent = 4f; // % of mask width
    public float maxSprayDistance = 15f;

    // Current target
    GameObject lastHitObj;
    RenderTexture maskRT;
    Material targetMat;

    void Reset()
    {
        if (!cam) cam = Camera.main;
    }

    void Update()
    {
        // 1) Aim – ray from camera to mouse to find UVs
        Ray mouseRay = cam.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(mouseRay, out RaycastHit aimHit, 100f))
        {
            // 2) If LMB is held, cast from nozzle toward hit point and paint if Cleanable
            if (Input.GetMouseButton(0))
            {
                Vector3 dir = (aimHit.point - nozzle.position).normalized;
                if (Physics.Raycast(nozzle.position, dir, out RaycastHit paintHit, maxSprayDistance))
                {
                    if (paintHit.collider.CompareTag("Cleanable"))
                    {
                        if (lastHitObj != paintHit.collider.gameObject)
                        {
                            lastHitObj = paintHit.collider.gameObject;
                            SetupTarget(lastHitObj);
                        }
                        if (maskRT)
                            PaintAtUV(paintHit.textureCoord);
                    }
                }
            }
        }
    }

    void SetupTarget(GameObject obj)
    {
        var mr = obj.GetComponent<MeshRenderer>();
        if (!mr) { maskRT = null; targetMat = null; return; }

        // this instantiates a per-object material (OK here)
        targetMat = mr.material;

        if (!targetMat.HasProperty("_Mask_Texture"))
        {
            Debug.LogWarning("Material has no _Mask_Texture property. Use URP/PaintReveal shader.", obj);
            maskRT = null;
            return;
        }

        // Try to clone the existing mask; if none, create white RT
        Texture maskTex = targetMat.GetTexture("_Mask_Texture");
        int w = 1024, h = 1024;
        if (maskTex != null) { w = maskTex.width; h = maskTex.height; }

        maskRT = new RenderTexture(w, h, 0, RenderTextureFormat.ARGB32);
        maskRT.wrapMode = TextureWrapMode.Clamp;
        maskRT.filterMode = FilterMode.Bilinear;

        // Fill RT with the current mask or pure white if null
        RenderTexture prev = RenderTexture.active;
        if (maskTex != null)
            Graphics.Blit(maskTex, maskRT);
        else
            Graphics.Blit(Texture2D.whiteTexture, maskRT);
        RenderTexture.active = prev;

        targetMat.SetTexture("_Mask_Texture", maskRT);
    }

    void PaintAtUV(Vector2 uv)
    {
        if (!brushTexture) { Debug.LogWarning("Assign a brushTexture (black circle with soft alpha)."); return; }

        RenderTexture prev = RenderTexture.active;
        RenderTexture.active = maskRT;

        GL.PushMatrix();
        GL.LoadPixelMatrix(0, maskRT.width, maskRT.height, 0);

        float px = maskRT.width  * uv.x;
        float py = maskRT.height * (1f - uv.y);

        float brushPx = Mathf.Max(2f, maskRT.width * (brushSizePercent / 100f));
        Rect rect = new Rect(px - brushPx * 0.5f, py - brushPx * 0.5f, brushPx, brushPx);

        // Draw brush: brush should be BLACK in the center with alpha falloff
        Graphics.DrawTexture(rect, brushTexture);

        GL.PopMatrix();
        RenderTexture.active = prev;
    }
}
