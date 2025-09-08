using UnityEngine;

public class PickupController : MonoBehaviour
{
    public float rotationSpeed = 50f;

    void Update()
    {
        transform.rotation *= Quaternion.Euler(x: 0f, y: rotationSpeed * Time.deltaTime, z: 0f);
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Player")
        {
            Destroy(gameObject);
        }
    }
}
