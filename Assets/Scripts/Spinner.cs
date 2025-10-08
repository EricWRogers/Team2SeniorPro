using UnityEngine;

public class Spinner : MonoBehaviour
{
    float spinSpeed = 1.0f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(spinSpeed * Time.deltaTime, 0, 0);
    }
}
