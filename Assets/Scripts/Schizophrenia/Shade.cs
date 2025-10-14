using UnityEngine;

/*public class Shade : MonoBehaviour
{
    public Transform player;
    public float stalkingSpeed = 2f;
    public float chaseSpeed = 5f;
    public float chaseRange = 10f;
    public float acceleration = 0.5f;

    private float currentSpeed;

    [Header("Audio")]
    public AudioClip scaryMusic;
    private AudioSource audioSource;

    [Header("Player Debuff")]
    public ThirdPersonMovement playerMovement; // Reference to player movement script
    public float slowRange = 8f;
    private float slowFactor;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        currentSpeed = stalkingSpeed;

        // start with no slowdown applied
        slowFactor = 1f;
        playerMovement.movementSlowMultiplier = slowFactor;
    }

    private void Update()
    {
        float distance = Vector3.Distance(transform.position, player.position);

        // Enemy follows player
        if (distance < chaseRange)
        {
            // Chase with acceleration
            currentSpeed = Mathf.Min(chaseSpeed, currentSpeed + acceleration * Time.deltaTime);
        }
        else
        {
            // Stalk slowly
            currentSpeed = stalkingSpeed;
        }

        transform.position = Vector3.MoveTowards(transform.position, player.position, currentSpeed * Time.deltaTime);

        // Apply slowdown effect to player
        if (distance < slowRange)
        {
            slowFactor = Mathf.Clamp01(distance / slowRange);
            playerMovement.movementSlowMultiplier = slowFactor;
        }
        else
        {
            playerMovement.movementSlowMultiplier = 1f; // normal speed
        }

        // Scary music volume scaling
        if (audioSource != null)
        {
            audioSource.volume = Mathf.Clamp01(1 - (distance / chaseRange));
        }

        audioSource = GetComponent<AudioSource>(); //fix this

        if (scaryMusic != null)
        {
            audioSource.clip = scaryMusic;
            audioSource.loop = true;   // keeps it looping
            audioSource.Play();
        }

        currentSpeed = stalkingSpeed;
    }

    private void OnTriggerEnter(Collider collision)
    {
        if (collision.CompareTag("Player"))
        {
            // Reset slowdown
            playerMovement.movementSlowMultiplier = 1f;

            // Silence audio
            if (audioSource != null) audioSource.Stop();

            // Despawn enemy
            Destroy(gameObject);

            // trigger schizophrenia debuff here with sanity bar 
        }
    }
}*/
