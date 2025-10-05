using UnityEngine;
using Unity.Cinemachine;

public class CameraZoom : MonoBehaviour
{
    [Header("References")]
    public CinemachineCamera vcam; // assign your 3rd person CinemachineCamera in Inspector

    [Header("Zoom Settings")]
    public float zoomSpeed = 10f;
    public float minFov = 30f;
    public float maxFov = 120f;

    private float currentFov;

    void Start()
    {
        if (vcam == null)
            vcam = GetComponent<CinemachineCamera>();

        currentFov = vcam.Lens.FieldOfView;
    }

    void Update()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");

        if (Mathf.Abs(scroll) > 0.01f)
        {
            currentFov -= scroll * zoomSpeed;
            currentFov = Mathf.Clamp(currentFov, minFov, maxFov);

            // Apply zoom to the Cinemachine camera lens
            vcam.Lens.FieldOfView = currentFov;
        }
    }
}
// This script allows zooming in and out using the mouse scroll wheel.