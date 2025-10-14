using UnityEngine;
using UnityEngine.SceneManagement;

public class DoorHUB : MonoBehaviour
{
    // Make sure your player GameObject is tagged as "Player" and the door has a Collider component with "Is Trigger" checked.
    // This script should be attached to the door GameObject.
    private void OnTriggerEnter(Collider other)
    {
        // Check if the colliding object is the player
        if (other.CompareTag("Player"))
        {
            // Load the next scene (make sure to add the scene to the build settings)
            GameManager.Instance.newMap("Level_1");
        }
    }
}
