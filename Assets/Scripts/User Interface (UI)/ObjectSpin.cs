using UnityEngine;

public class SpinObject : MonoBehaviour
{
    // Adjust this value in the Unity Inspector to change speed
    public float rotationSpeed = 100f;

    void Update()
    {
        // Vector3.forward represents the Z-axis (0, 0, 1)
        // Multiplying by Time.deltaTime ensures the spin is smooth across different frame rates
        transform.Rotate(Vector3.forward * rotationSpeed * Time.deltaTime);
    }
}
