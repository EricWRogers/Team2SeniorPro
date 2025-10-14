using UnityEngine;

public class Spinner : MonoBehaviour
{
    float spinSpeed = 20.0f;
    Transform meshTransform;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        meshTransform = transform;
    }

    // Update is called once per frame
    void Update()
    {
        meshTransform.Rotate(0, spinSpeed * Time.deltaTime, 0);
    }
}
