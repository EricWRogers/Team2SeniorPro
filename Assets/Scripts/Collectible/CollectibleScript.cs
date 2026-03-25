using UnityEngine;

public class CollectibleScript : MonoBehaviour
{
    public int collectibleCheckpointNumber = 0;
    public Animator animator;
    public GameObject collectParticles;
    public SoundManager SM;
    public string collectSound;

    void Awake()
    {
        if (SM == null)
        {
            SM = FindFirstObjectByType<SoundManager>();
            if (SM == null)
            {
                Debug.LogError("No SoundManager found in scene!");
            }
        }

        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
            if (animator == null)
            {
                Debug.LogWarning("No Animator found on collectible or its children!");
            }
        }
    }

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

        // If particles are a child, detach them first
        if (collectParticles != null)
        {
            collectParticles.transform.parent = null; // Detach from collectible
            collectParticles.SetActive(true);

            ParticleSystem ps = collectParticles.GetComponent<ParticleSystem>();
            if (ps != null)
            {
                ps.Play();
                Destroy(collectParticles, ps.main.duration + ps.main.startLifetime.constantMax);
            }
            else
            {
                Destroy(collectParticles, 1f);
            }
        }

        if (SM != null)
            {
                SM.PlaySFX(collectSound, 1);
            }

        if (animator != null)
            animator.SetTrigger("BerryCollect");

        GameManager.Instance.collectibleCount++;

        // Destroy immediately — no delay needed now
        Destroy(transform.parent.gameObject);
    }

}