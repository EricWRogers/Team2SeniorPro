using System.Collections.Generic;
using UnityEngine;

public class FakeAcorn : MonoBehaviour
{
    public GameObject[] objectPrefabs;
    public float spawnInterval;
    public float spawnLimitXLeft = -10f;
    public float spawnLimitXRight = 10f;
    public float spawnPosY = 5f;
    public float spawnLimitZFront = -10f;
    public float spawnLimitZBack = 10f;

    void Start()
    {
        InvokeRepeating("SpawnRandomObject", 1f, 2f);
    }

    void SpawnRandomObject()
    {
        // choose a random prefab from array
        int randomIndex = Random.Range(0, objectPrefabs.Length);
        GameObject objectToSpawn = objectPrefabs[randomIndex];

        // Generate a random position
        Vector3 spawnPos = new Vector3(Random.Range(spawnLimitXLeft, spawnLimitXRight), spawnPosY, Random.Range(spawnLimitZFront, spawnLimitZBack));

        // Instantiate the chosen object at the random position
        Instantiate(objectToSpawn, spawnPos, objectToSpawn.transform.rotation);
    }
    
}
