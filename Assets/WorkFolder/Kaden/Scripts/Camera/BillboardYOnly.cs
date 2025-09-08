using UnityEngine;

public class BillboardYOnly : MonoBehaviour
{
    Transform cam;
    void Start() { cam = Camera.main ? Camera.main.transform : null; }
    void LateUpdate()
    {
        if (!cam) return;
        Vector3 toCam = cam.position - transform.position;
        toCam.y = 0f;
        if (toCam.sqrMagnitude < 0.0001f) return;
        transform.rotation = Quaternion.LookRotation(toCam.normalized, Vector3.up);
    }
}
