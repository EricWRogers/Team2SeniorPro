using System.Collections;
using UnityEngine;

public class FakeAcorn : MonoBehaviour
{
    public GameObject[] objectPrefabs;
    public Panic panicScript;

    [Header("Spawn Settings")]
    public float spawnInterval = 2f; // Default
    public float spawnLimitXLeft = -10f;
    public float spawnLimitXRight = 10f;
    public float spawnPosY = 5f;
    public float spawnLimitZFront = -10f;
    public float spawnLimitZBack = 10f;

    [Header("Post-Spawn Animation")]
    public float delayBeforeAnim = 2f; // Time it sits before animating
    public float delayBeforeDestroy = 4f; // Time after animation before destruction
    
    void Start()
    {
        // Automatically find the script if not set in inspector
        if (panicScript == null)
        {
            panicScript = FindFirstObjectByType<Panic>();
        }
        else
        {
            Debug.LogWarning("Panic script reference not set on FakeAcorn. Attempting to find in scene.");
        }

        InvokeRepeating("SpawnRandomObject", 1f, spawnInterval);
    }

    void SpawnRandomObject()
    {
        // Safety check
        if (panicScript == null) return;

        // Logic: Only spawn if sanity is full
        if (!panicScript.IsFull())
        {
            return; // Don't spawn if sanity isn't full
        }

        // choose a random prefab from array
        int randomIndex = Random.Range(0, objectPrefabs.Length);
        GameObject objectToSpawn = objectPrefabs[randomIndex];

        // Generate a random position
        Vector3 spawnPos = new Vector3(Random.Range(spawnLimitXLeft, spawnLimitXRight), spawnPosY, Random.Range(spawnLimitZFront, spawnLimitZBack));

        // Instantiate the chosen object at the random position
        GameObject spawnedObject = Instantiate(objectToSpawn, spawnPos, objectToSpawn.transform.rotation);

        // Start the lifecycle coroutine for the spawned object
        StartCoroutine(AcornLifeCycle(spawnedObject));
    }
    
    IEnumerator AcornLifeCycle(GameObject acorn)
    {
        // Wait after spawning
        yield return new WaitForSeconds(delayBeforeAnim);

        if (acorn != null)
        {
            Animator anim = acorn.GetComponent<Animator>();
            if (anim != null)
            {
                anim.Play("GhostNutFade"); // Play the fade animation
            }
            else {
                Debug.LogWarning("No Animator found on spawned object. Skipping animation.");
            }
        }

        yield return new WaitForSeconds(delayBeforeDestroy);

        // Cleanup
        if (acorn != null)
        {
            Destroy(acorn); // Destroy the object after animation
        }
    }
}
