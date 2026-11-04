using UnityEngine;
using UnityEngine.SceneManagement;


#if UNITY_EDITOR
using UnityEditor;
#endif

public class CollectibleScript : MonoBehaviour
{
    [Header("Collectible Settings")]
    public string berryID; // IMPORTANT: Unique ID for every berry, set in the inspector
    public bool countsTowardTotal = true; // Set to false for berries that shouldn't count toward the total (e.g., tutorial berries)

    [Header("References")]
    public Animator animator;
    public GameObject collectParticles;
    public string collectSound;

    void Awake()
    {
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
        if (countsTowardTotal) {
            // Fallback: If you forget to manually type an ID, generate a standarized, stable one.
            // Using integer rounding (Mathf.RoundToInt) ensures float precision issues within builds that won't mismatch ID strings.
            if (string.IsNullOrEmpty(berryID))
            {
                int x = Mathf.RoundToInt(transform.position.x);
                int y = Mathf.RoundToInt(transform.position.y);
                int z = Mathf.RoundToInt(transform.position.z);
                berryID = $"{SceneManager.GetActiveScene().name}_{gameObject.name}_{x}_{y}_{z}";
            }

            if (GameManager.Instance != null)
            {
                // NEW LOGIC: If the ID is in the collected list, vanish immediately
                if (GameManager.Instance.IsBerryCollected(berryID))
                {
                    Destroy(transform.parent.gameObject);
                    return; // Stop further processing if the berry has already been collected - prevents duplicate collection logic from Start()
                }
            }
        }
    }

    private void OnValidate()
    {
        // This function runs automatically whenever you change something in the inspector.
        // It ensures that every berry has a unique ID, even if you forget to set it.
        if (!Application.isPlaying) return;

        if (string.IsNullOrEmpty(berryID))
        {
            // Generate an ID that includes the object name and its position
            // This ensures two berries or more berries in the same scene have different IDs
            int x = Mathf.RoundToInt(transform.position.x);
            int y = Mathf.RoundToInt(transform.position.y);
            int z = Mathf.RoundToInt(transform.position.z);
            berryID = $"{gameObject.name}_{x}_{y}_{z}";
           
            #if UNITY_EDITOR
            EditorUtility.SetDirty(this);
            #endif
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

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySFX(collectSound, 1);
        }
        else
        {
            Debug.LogWarning("SoundManager instance not found. Cannot play collect sound.");
        }

        if (animator != null) 
        {
            animator.SetTrigger("BerryCollect");
        }

        // Record the berry as collected in the GameManager
        if (GameManager.Instance != null)
        {
            if (countsTowardTotal)
            {
                GameManager.Instance.CollectBerry(berryID);
            }
            else
            {
                // For berries that don't count toward the total, you might still want to track them
                GameManager.Instance.collectibleCount++;
            }
        }

        if (countsTowardTotal)
        {
            // NEW: Tell the GameManager which berry was grabbed;
            GameManager.Instance.CollectBerry(berryID);
        }
        else
        {
            // Just increment the temporary per-level counter
            // (or do nothing if you don't even want it in the UI)
            GameManager.Instance.collectibleCount++;
        }

        // Destroy immediately — no delay needed now
        Destroy(transform.parent.gameObject);
    }

}