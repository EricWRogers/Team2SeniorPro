using UnityEngine;

public class KillVolume : MonoBehaviour
{
    public Transform playerRespawn;  // usually same as BottomRespawn
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Acorn"))
        {
            other.GetComponent<CarryableAcorn>()?.RespawnToBottom();
        }
        else if (other.CompareTag("Player"))
        {
            var rb = other.attachedRigidbody;
            if (rb) { rb.linearVelocity = Vector3.zero; }
            other.transform.position = playerRespawn.position + Vector3.up * 0.5f;
        }
    }
}
