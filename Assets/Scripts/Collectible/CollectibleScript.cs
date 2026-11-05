using UnityEngine;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class CollectibleScript : MonoBehaviour
{
    [Header("Collectible Settings")]
    [Tooltip("Type a manual ID here to guarantee it locks, or leave empty for auto-generation.")]
    public string berryID; 
    public bool countsTowardTotal = true;

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
        if (countsTowardTotal)
        {
            // // NOTE: Fallback generation if no manual ID is set. 
            // Always ensures it includes the scene name to avoid cross-scene or restart ID mismatches.
            if (string.IsNullOrEmpty(berryID))
            {
                GenerateId();
            }

            // // NOTE: Check if GameManager already has this berry recorded as collected.
            // If it does, vanish immediately upon loading/restarting the scene.
            if (GameManager.Instance != null)
            {
                if (GameManager.Instance.IsBerryCollected(berryID))
                {
                    DestroyCollectible();
                    return; 
                }
            }
        }
    }

    private void OnValidate()
    {
        // // NOTE: Automatically generates a stable ID in the inspector if left blank,
        // preventing manual setup errors in large levels.
        if (!Application.isPlaying) return;

        if (string.IsNullOrEmpty(berryID))
        {
            GenerateId();
            
            #if UNITY_EDITOR
            EditorUtility.SetDirty(this);
            #endif
        }
    }

    private void GenerateId()
    {
        int posX = Mathf.RoundToInt(transform.position.x);
        int posY = Mathf.RoundToInt(transform.position.y);
        int posZ = Mathf.RoundToInt(transform.position.z);
        
        // // NOTE: Standardized ID format combining Scene Name, Object Name, and Rounded Coordinates
        // This guarantees uniqueness and stops duplicate re-appearances on rapid restarts/deaths.
        berryID = $"{SceneManager.GetActiveScene().name}_{gameObject.name}_{posX}_{posY}_{posZ}";
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        Debug.Log("Collectible touched by player");

        // // NOTE: Record the berry as collected in the GameManager IMMEDIATELY 
        // before triggering particle/sound effects or destroying the object.
        if (GameManager.Instance != null)
        {
            if (countsTowardTotal)
            {
                GameManager.Instance.CollectBerry(berryID);
            }
            else
            {
                GameManager.Instance.collectibleCount++;
            }
        }

        TriggerEffects();
        DestroyCollectible();
    }

    private void TriggerEffects()
    {
        // // NOTE: Detach particles so they can finish playing even after the berry GameObject is destroyed
        if (collectParticles != null)
        {
            collectParticles.transform.parent = null;
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
    }

    private void DestroyCollectible()
    {
        // // NOTE: Safely destroys either the parent container layout or the berry itself
        if (transform.parent != null)
            Destroy(transform.parent.gameObject);
        else
            Destroy(gameObject);
    }
}