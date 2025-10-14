// PUT THIS SCRIPT IN ANY TRIGGER MESH WITH A DEBUG RENDERING MESH TO HIDE IT WHILE RUNNING
using UnityEngine;
public class DebugMeshHide : MonoBehaviour
{
    void Awake()
    {
        if (Application.isPlaying)
        {
            MeshRenderer meshRenderer = GetComponentInParent<MeshRenderer>();
            meshRenderer.enabled = false;
        }
    }
}
