using UnityEngine;

public class BillboardCanvas : MonoBehaviour
{
    public Canvas billboardCanvas;
    public Transform cameraTransform;

    void Start()
    {
        if (billboardCanvas == null)
        {
            billboardCanvas = GetComponentInParent<Canvas>();
        }

        FindPlayerCamera();
    }

    void Update()
    {
        // Continuously look for the Main Camera if it wasn't ready during Start()
        if (cameraTransform == null)
        {
            FindPlayerCamera();
        }

        if (billboardCanvas != null && cameraTransform != null)
        {
            billboardCanvas.transform.LookAt(cameraTransform);
            billboardCanvas.transform.Rotate(0, 180, 0); // Flip to face the camera
        }
    }

    private void FindPlayerCamera()
    {
        GameObject playerCamObj = GameObject.Find("NewPlayerCam");
        if (playerCamObj != null)
        {
            cameraTransform = playerCamObj.transform;
            return;
        }

        if (Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
        }
    }
}