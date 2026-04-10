using NUnit.Framework;
using UnityEngine;

public class Key : MonoBehaviour
{
    public GameObject bars;

    public int keyNum = 0;
    public bool isCollected = false;
    public Animator textAnimatior;
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

            if (textAnimatior != null) textAnimatior.SetTrigger("KeyCollect");

            Destroy(gameObject);
        }
    }
}
