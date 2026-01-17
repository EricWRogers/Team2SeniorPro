using UnityEngine;

public class CollectibleScript : MonoBehaviour
{
    public int collectibleCheckpointNumber = 0; //the checkpoint number this collectible is associated with
    public Animator animator;
    public AudioSource SFXSource;
    public AudioClip collectSFX;
    void Start()
    {
        if (GameManager.Instance != null)
        {
            if (collectibleCheckpointNumber < GameManager.Instance.currentCheckpoint && GameManager.Instance.currentCheckpoint != -1) //if the player has passed a checkpoint and has respawned, destroy all collectibles that are prior to that checkpoint
            {
                Destroy(transform.parent.gameObject);
            }
        }
        else if (GameManager.Instance == null)
        {
            Debug.LogWarning("No GameManager found in scene!");
        }

    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Collectible touched by player");
            //SoundManager.Instance.PlaySFX(SoundManager.Instance.sfxSource.clip);
            SFXSource.PlayOneShot(collectSFX);
            animator.SetTrigger("BerryCollect");

            GameManager.Instance.collectibleCount++;
            Destroy(transform.parent.gameObject, 0.1f);
        }
    }
}
