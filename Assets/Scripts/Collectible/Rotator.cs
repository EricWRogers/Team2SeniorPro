using UnityEngine;

public class Rotator : MonoBehaviour
{
    public float rotateSpeed = 20.0f;
    Transform meshTransform;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        meshTransform = transform;
    }

    // Update is called once per frame
    void Update()
    {
        // Rotate by Z axis
        meshTransform.Rotate(0, 0, rotateSpeed * Time.deltaTime);
    }
}
