using UnityEngine;

public class Key : MonoBehaviour
{
    public GameObject bars;

    public int keyNum = 0;
    public bool isCollected = false;
    public string keyAudio;

    float spinSpeed = 20.0f;
    Transform meshTransform;

    void Start()
    {
        meshTransform = transform;
    }

    void Update()
    {
        meshTransform.Rotate(0, 0, spinSpeed * Time.deltaTime);
    }

    public void OnTriggerEnter(Collider key)
    {
        if (key.CompareTag("Player") && !isCollected)
        {
            isCollected = true; // Set immediately to prevent multiple triggers
            
            if (bars != null) bars.SetActive(false);

            // Using static instance directly to play sound effect
            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.PlaySFX(keyAudio, 1);
            }
            else
            {
                // Tell the developer that the SoundManager instance is missing
                Debug.LogError("Key on {gameObject.name} can't find SoundManager.Instance!");
            }

            if (GetComponent<Renderer>() != null) GetComponent<Renderer>().enabled = false; // Make the key invisible in the world
            if (GetComponent<Collider>() != null) GetComponent<Collider>().enabled = false; // Disable the collider to prevent further triggers

            foreach (Transform child in transform)
            {
                child.gameObject.SetActive(false); // Disable all child objects to hide the entire key
            }
        }
    }
}
