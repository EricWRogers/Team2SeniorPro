using System.Collections.Generic;
using UnityEngine;

public class KeyFollow : MonoBehaviour
{
    public Transform player; // Reference to the player's transform
    public Key key; // Reference to the Key script
    public GameObject spawnedKey; // Reference to the key GameObject
    public float followDelay = 0.5f; // Delay before the key starts following the player
    private List<Vector3> positionHistory = new List<Vector3>(); // List to store player's past positions

    // Update is called once per frame
    void Update()
    {
        // Check if the key has been collected
        if (key.isCollected == true && !spawnedKey.activeInHierarchy)
        {
            spawnedKey.SetActive(true); // Make the key visible in the world
            FollowPlayer(); // If the key has been collected, start following the player
        }

        // rotate spawnedKey
        if (spawnedKey.activeInHierarchy)
        {
            spawnedKey.transform.Rotate(0, 50 * Time.deltaTime, 0); // Rotate the key around the Y-axis
        }
        
    }

    void FollowPlayer()
    {
        // Store player position in history
        positionHistory.Add(player.position - player.forward * 2f); // Store position slightly behind the player

        // Move to position from 'n' frames ago
        int index = Mathf.FloorToInt(followDelay * 60f); // Convert delay to frames (assuming 60 FPS)

        if (positionHistory.Count > index)
        {
            transform.position = positionHistory[0]; // Move to the oldest position in the history
            positionHistory.RemoveAt(0); // Remove the oldest position
        }
    }
}
