using UnityEngine;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class CollectibleScript : MonoBehaviour
{
    [Header("Collectible Settings")]
    [Tooltip("Type a manual ID here to guarantee it locks (e.g., 'Level1_Berry_01'). Recommended for stubborn berries!")]
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
        }
    }

    private void Start()
    {
        if (countsTowardTotal)
        {
            // If no manual ID was typed, fall back to auto-generating one
            if (string.IsNullOrEmpty(berryID))
            {
                GenerateId();
            }

            Debug.Log($"Checking Berry on Start. ID: {berryID}");

            // Check if GameManager already has this exact ID recorded as collected
            if (GameManager.Instance != null)
            {
                if (GameManager.Instance.IsBerryCollected(berryID))
                {
                    Debug.Log($"Berry {berryID} is already collected. Vanishing!");
                    DestroyCollectible();
                    return; 
                }
            }
        }
    }

    private void OnValidate()
    {
        if (!Application.isPlaying) return;

        // Only auto-generate if left completely blank
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
        // Fallback auto-generation using hierarchy path
        string hierarchyPath = GetHierarchyPath(transform);
        berryID = $"{SceneManager.GetActiveScene().name}_{hierarchyPath}";
    }

    private string GetHierarchyPath(Transform t)
    {
        string path = t.name;
        Transform parentTransform = t.parent;
        while (parentTransform != null)
        {
            path = parentTransform.name + "/" + path;
            parentTransform = parentTransform.parent;
        }
        return path;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        Debug.Log($"Collectible touched by player. Registering ID: {berryID}");

        // Record the berry as collected in the GameManager IMMEDIATELY
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

        if (animator != null) 
        {
            animator.SetTrigger("BerryCollect");
        }
    }

    private void DestroyCollectible()
    {
        if (transform.parent != null)
            Destroy(transform.parent.gameObject);
        else
            Destroy(gameObject);
    }
}