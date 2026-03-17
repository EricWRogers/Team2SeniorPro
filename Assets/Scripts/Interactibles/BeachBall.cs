using UnityEngine;

public class BeachBall : MonoBehaviour
{
    public float bounceForce = 20;
    private Rigidbody ball;

    private void Start()
    {
        ball = GetComponent<Rigidbody>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
            {
            Rigidbody player = collision.gameObject.GetComponent<Rigidbody>();
            if (player != null)
            {
                Vector3 direction = player.angularVelocity.normalized;

                if (direction.magnitude > 0.1f)
                {
                    ball.AddForce(direction * bounceForce, ForceMode.Impulse);
                }

            }

        }
    }
}