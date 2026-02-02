using UnityEngine;

public class CollectibleScript : MonoBehaviour
{
    public int collectibleCheckpointNumber = 0;
    public Animator animator;
    public AudioClip collectSFX;

    private void Start()
    {
        if (GameManager.Instance != null)
        {
            if (collectibleCheckpointNumber < GameManager.Instance.currentCheckpoint &&
                GameManager.Instance.currentCheckpoint != -1)
            {
                Destroy(transform.parent.gameObject);
            }
        }
        else
        {
            Debug.LogWarning("No GameManager found in scene!");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        Debug.Log("Collectible touched by player");

        // Play SFX safely
        AudioSource.PlayClipAtPoint(collectSFX, transform.position);

        // Play animation
        if (animator != null)
            animator.SetTrigger("BerryCollect");

        GameManager.Instance.collectibleCount++;

        // Destroy after animation trigger
        Destroy(transform.parent.gameObject, 0.1f);
    }
}