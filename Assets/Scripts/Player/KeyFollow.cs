using System;
using System.Collections.Generic;
using UnityEngine;

public class KeyFollow : MonoBehaviour
{
    [Header("References")]
    public Transform player; // Reference to the player's transform
    public Key key; // Reference to the Key script
    public GameObject spawnedKey; // Reference to the key GameObject

    [Header("Follow Settings")]
    public float followDelay = 0.5f;
    [Tooltip("Distance the key stays away from the follow point")]
    public float orbitRadius = 1.5f;
    [Tooltip("How fast the key spins around the player")]
    public float rotationSpeed = 100f;
    [Tooltip("Height offset from the player's feet")]
    public float verticalOffset = 1.0f;

    [Header("Logic")]
    public bool keyRequired = true; // Flag to determine if the key is required for the player to follow

    private List<Vector3> positionHistory = new List<Vector3>(); // List to store player's past positions
    private float currentRotationAngle = 0f; // Track the angle manually

    void Awake()
    {
        if (player == null)
        {
            player = GameObject.FindWithTag("Player").transform;
        }

        if (key == null && keyRequired)
        {
            key = GameObject.FindFirstObjectByType<Key>();
        }
        
        // if the key is not required, ignore the null reference as it is not needed for the script to function
        if (!keyRequired)
        {
            Debug.LogWarning("Key reference is null, but key is not required. Ignoring null reference.");
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Check if key is needed and ensure the key reference exists
        if (keyRequired && key != null)
        {
            // Check if the key has been collected and the spawnedKey is not already active
            if (key.isCollected && !spawnedKey.activeInHierarchy)
            {
                spawnedKey.SetActive(true); // Make the key visible in the world
            }
        }

        // rotate spawnedKey around player
        if (spawnedKey != null && spawnedKey.activeInHierarchy)
        {
            // Update the rotation angle
            currentRotationAngle += 100f * Time.deltaTime; // Rotate at 100 degrees per second
            FollowPlayer(); // If the key has been collected, start following the player and orbiting around them
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
            Vector3 targetBasePosition = positionHistory[0]; // Move to the oldest position in the history
            
           // Use the public variables to calculate the new position
            Vector3 offset = new Vector3(Mathf.Cos(currentRotationAngle * Mathf.Deg2Rad), 0, Mathf.Sin(currentRotationAngle * Mathf.Deg2Rad)) * orbitRadius;

            // Apply the offset + the vertical height
            spawnedKey.transform.position = targetBasePosition + offset + (Vector3.up * verticalOffset);

            positionHistory.RemoveAt(0); // Remove the oldest position
        }
    }
}
