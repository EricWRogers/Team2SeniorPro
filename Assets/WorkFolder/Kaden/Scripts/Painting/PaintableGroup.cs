using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class PaintableGroup : MonoBehaviour
{
    [Header("Paintable renderers")]
    public List<Renderer> paintableRenderers = new();
    public bool autoCollectRenderers = true;

    [Header("Material / Shader")]
    public Shader paintShader;                 // "URP/PaintReveal"
    public Texture2D defaultOverlayTex;       
    public Color overlayColor = Color.white;

    [Header("Base texture fallback")]
    public Texture2D fallbackBaseTex;          // if the source mat has no tex
    public int solidColorTexSize = 4;          // size for auto solid-color tex 

    [Header("Mask setup")]
    public int maskResolution = 1024;          // per-renderer mask
    public Texture2D initialMask;              // if null -> white

    [Header("Colliders")]
    public bool addMeshColliders = true;
    public bool collidersConvex = false;

    // runtime: one mask per renderer
    readonly Dictionary<Renderer, RenderTexture> maskPerRenderer = new();

    public bool TryGetMask(Renderer r, out RenderTexture rt) => maskPerRenderer.TryGetValue(r, out rt);

    void Awake()
    {
        if (!paintShader) paintShader = Shader.Find("URP/PaintReveal");
        if (autoCollectRenderers && paintableRenderers.Count == 0)
            paintableRenderers.AddRange(GetComponentsInChildren<MeshRenderer>(true));

        foreach (var rend in paintableRenderers)
        {
            if (!rend) continue;

            // 1) Clone all sub-materials to paintShader while preserving base textures
            var srcMats = rend.sharedMaterials;
            if (srcMats == null || srcMats.Length == 0) continue;

            var newMats = new Material[srcMats.Length];
            for (int i = 0; i < srcMats.Length; i++)
            {
                var src = srcMats[i];
                var m = new Material(paintShader);

                // find a base texture on the original material (common prop names)
                Texture baseTex = null;
                string[] baseNames = { "_BaseMap", "_MainTex", "_BaseColorMap", "_BaseTex" };
                foreach (var n in baseNames)
                    if (src && src.HasProperty(n)) { baseTex = src.GetTexture(n); if (baseTex) break; }

                // if none, try to read a solid color and build a tiny texture
                if (!baseTex)
                {
                    Color col = Color.gray;
                    if (src)
                    {
                        if (src.HasProperty("_BaseColor")) col = src.GetColor("_BaseColor");
                        else if (src.HasProperty("_Color")) col = src.GetColor("_Color");
                    }
                    if (fallbackBaseTex) baseTex = fallbackBaseTex;
                    else baseTex = MakeSolidColor(col, solidColorTexSize);
                }

                m.SetTexture("_BaseTex", baseTex);
                if (defaultOverlayTex) m.SetTexture("_OverlayTex", defaultOverlayTex);
                m.SetColor("_OverlayColor", overlayColor);

                newMats[i] = m;
            }

            // 2) Create a unique mask RT for this renderer and assign to all sub-mats
            int w = maskResolution, h = maskResolution;
            if (initialMask) { w = initialMask.width; h = initialMask.height; }
            var rt = new RenderTexture(w, h, 0, RenderTextureFormat.ARGB32)
            {
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear
            };
            var prev = RenderTexture.active;
            Graphics.Blit(initialMask ? initialMask : Texture2D.whiteTexture, rt);
            RenderTexture.active = prev;

            for (int i = 0; i < newMats.Length; i++)
                newMats[i].SetTexture("_Mask_Texture", rt);

            rend.materials = newMats;                 // assign the full array
            maskPerRenderer[rend] = rt;

            // 3) Ensure UV raycasts: MeshCollider on the same GO
            if (addMeshColliders && !rend.TryGetComponent<MeshCollider>(out _))
            {
                var mf = rend.GetComponent<MeshFilter>();
                if (mf && mf.sharedMesh)
                {
                    var mc = rend.gameObject.AddComponent<MeshCollider>();
                    mc.sharedMesh = mf.sharedMesh;
                    mc.convex = collidersConvex; // keep false for accurate UVs
                }
            }

            // 4) Tag so painters can filter
            if (rend.gameObject.tag != "Cleanable")
                rend.gameObject.tag = "Cleanable";
        }
    }

    Texture2D MakeSolidColor(Color c, int size)
    {
        var t = new Texture2D(size, size, TextureFormat.RGBA32, false);
        t.wrapMode = TextureWrapMode.Repeat;
        t.filterMode = FilterMode.Bilinear;
        var px = new Color[size * size];
        for (int i = 0; i < px.Length; i++) px[i] = c;
        t.SetPixels(px);
        t.Apply();
        return t;
    }

    void OnDestroy()
    {
        foreach (var kvp in maskPerRenderer)
            if (kvp.Value) kvp.Value.Release();
        maskPerRenderer.Clear();
    }
}
