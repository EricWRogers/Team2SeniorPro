using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CollectibleScript : MonoBehaviour
{
    [Header("Collectible Settings")]
    public string berryID; // IMPORTANT: Unique ID for every berry, set in the inspector
    public bool countsTowardTotal = true; // Set to false for berries that shouldn't count toward the total (e.g., tutorial berries)

    [Header("References")]
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
        if (countsTowardTotal) {
            // Fallback: if you forget to set the berryID, use its position as an ID
            if (string.IsNullOrEmpty(berryID))
                berryID = SceneManager.GetActiveScene().name + "_" + gameObject.name + transform.position.ToString();

            if (GameManager.Instance != null)
            {
                // NEW LOGIC: If the ID is in the collected list, vanish immediately
                if (GameManager.Instance.IsBerryCollected(berryID))
                {
                    Destroy(transform.parent.gameObject);
                }
            }
        }
        else
        {
            Debug.LogWarning("No GameManager found in scene!");
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
            berryID = $"{gameObject.name}_{transform.position.x}_{transform.position.y}_{transform.position.z}";
           
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

        if (SM != null)
            {
                SM.PlaySFX(collectSound, 1);
            }

        if (animator != null) 
        {
            animator.SetTrigger("BerryCollect");
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