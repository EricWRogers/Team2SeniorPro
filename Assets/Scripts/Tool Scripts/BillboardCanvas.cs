using UnityEngine;

public class BillboardCanvas : MonoBehaviour
{
    public Canvas billboardCanvas;
    public Transform cameraTransform;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (billboardCanvas == null)
        {
            billboardCanvas = GetComponentInParent<Canvas>();
        }
        cameraTransform = Camera.main.transform;
    }

    // Update is called once per frame
    void Update()
    {
        if (billboardCanvas != null && cameraTransform != null)
        {
            billboardCanvas.transform.LookAt(cameraTransform);
            billboardCanvas.transform.Rotate(0, 180, 0); //flip to face the camera
        }
    }

}
