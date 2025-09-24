using UnityEngine;

public class WorldSpace : MonoBehaviour
{
    public Transform target;           // the acorn Transform
    public Camera cam;                 // main camera
    public RectTransform markerRect;   
    public Vector2 screenPadding = new Vector2(32, 32);
    public float worldOffsetY = 0.6f;  // show above the acorn

    void Reset()
    {
        markerRect = GetComponent<RectTransform>();
        cam = Camera.main;
    }

    void LateUpdate()
    {
        if (!target || !cam || !markerRect) return;

        Vector3 worldPos = target.position + Vector3.up * worldOffsetY;
        Vector3 sp = cam.WorldToScreenPoint(worldPos);

       
        if (sp.z < 0f)
        {
            sp.x = Screen.width - sp.x;
            sp.y = Screen.height - sp.y;
            sp.z = 0.1f;
        }

        // Clamp to screen with padding
        float x = Mathf.Clamp(sp.x, screenPadding.x, Screen.width - screenPadding.x);
        float y = Mathf.Clamp(sp.y, screenPadding.y, Screen.height - screenPadding.y);

        markerRect.position = new Vector3(x, y, 0f);
    }
}
