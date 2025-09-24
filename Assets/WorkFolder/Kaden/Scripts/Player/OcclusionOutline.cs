using UnityEngine;

public class OcclusionOutline : MonoBehaviour
{
    public Transform spriteRoot;   
    public Renderer outlineRenderer;
    public LayerMask obstacleMask; 
    public float checkRadius = 0.2f;

    Transform cam;

    void Start()
    {
        cam = Camera.main ? Camera.main.transform : null;
        if (outlineRenderer) outlineRenderer.enabled = false;
    }

    void LateUpdate()
    {
        if (!cam || !spriteRoot || !outlineRenderer) return;

        Vector3 from = cam.position;
        Vector3 to = spriteRoot.position;
        Vector3 dir = (to - from);
        float dist = dir.magnitude;
        if (dist < 0.01f) { outlineRenderer.enabled = false; return; }

        dir /= dist;
        
        bool occluded = Physics.SphereCast(from, checkRadius, dir, out var hit, dist, obstacleMask);
        outlineRenderer.enabled = occluded;
    }
}
