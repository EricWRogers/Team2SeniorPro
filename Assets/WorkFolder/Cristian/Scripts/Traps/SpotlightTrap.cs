using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//using SuperPupSystems.Helper; // If your debuff or health system is here

public class SpotlightTrap : MonoBehaviour
{
    [Header("Targets")]
    public Transform playerTarget;
    public float detectionRadius = 15f;
    public LayerMask visionBlockMask; // Assign walls/obstacles here

    [Header("Spotlight Settings")]
    public Light spotLight;
    public Color playerColor = Color.white;

    public Color observingColor = Color.red;
    public float rotationSpeed = 2f;
    public float searchAngle = 45f;
    public float searchSpeed = 2f;

    [Header("Debuff Settings")]
    public float countdownTime = 3f; // How long it must see the player
    private float countdownTimer;
    private bool debuffGiven = false;

    [Header("Audio Settings")]
    public AudioSource audioSource;
    public AudioClip searchClip;
    public AudioClip playerClip;

    private enum SpotState { Searching, PlayerViewing }
    private SpotState currentState = SpotState.Searching;

    private float searchTimer = 0f;

    void Start()
    {
        if (spotLight == null)
            spotLight = GetComponentInChildren<Light>();
    }

    void Update()
    {
        if (playerTarget != null && Vector3.Distance(transform.position, playerTarget.position) <= detectionRadius)
        {
            if (CanSeePlayer())
            {
                currentState = SpotState.PlayerViewing;
                PlayerFocus();
                return;
            }
        }

        // Otherwise → Search mode
        currentState = SpotState.Searching;
        SearchMode();
    }

    
    private void PlayerFocus()
    {
        spotLight.color = observingColor;
        RotateTowards(playerTarget.position);

        PlayClip(playerClip);

        // Count down
        if (!debuffGiven)
        {
            countdownTimer -= Time.deltaTime;
            if (countdownTimer <= 0f)
            {
                ApplyRandomDebuff();
                debuffGiven = true;
            }
        }
    }

    //While in search mode add a state that allows the spotlight to lock onto the acorn as well if throwns as a sort of distraction
    //Also make sure to implement the mechanic where the spotlight doesnt actually have 360 vision somehow
    private void SearchMode()
    {
        spotLight.color = playerColor;

        PlayClip(searchClip);

        countdownTimer = countdownTime; // reset when not looking
        debuffGiven = false;

        searchTimer += Time.deltaTime * searchSpeed;
        float angle = Mathf.Sin(searchTimer) * searchAngle;
        Quaternion rotation = Quaternion.Euler(0f, angle, 0f);
        transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * rotationSpeed);
    }

    private void RotateTowards(Vector3 targetPos)
    {
        Vector3 dir = (targetPos - transform.position).normalized;
        Quaternion lookRot = Quaternion.LookRotation(dir, Vector3.up);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, Time.deltaTime * rotationSpeed);
    }

    private bool CanSeePlayer()
    {
        Vector3 dirToPlayer = (playerTarget.position - transform.position).normalized;
        float distToPlayer = Vector3.Distance(transform.position, playerTarget.position);

        if (Physics.Raycast(transform.position, dirToPlayer, out RaycastHit hit, distToPlayer, visionBlockMask))
        {
            // Hit a wall => can’t see player
            //update to where you have go be in a specific cone shaped view of the raycast before it sees you and doesnt just see u regardless of where u are
            return false;
        }
        return true;
    }

    private void ApplyRandomDebuff()
    {
        // Example debuff logic
        int debuffType = Random.Range(0, 3); // Pick between 3 debuffs
        switch (debuffType)
        {
            case 0:
                Debug.Log("Debuff: Slowed movement");
                // e.g., playerTarget.GetComponent<PlayerMovement>().ApplySlow(); //This will be default to always happen
                break;
            case 1:
                Debug.Log("Debuff: Reduced vision");
                // e.g., playerTarget.GetComponent<PlayerEffects>().ApplyDarkness();  //Add paranoia/sanity stacks plus one
                break;
            case 2:
                Debug.Log("Debuff: Health drain");
                //Health hp = playerTarget.GetComponent<Health>();
                //if (hp != null) hp.Damage(10);
                break;
        }
    }

    private void PlayClip(AudioClip clip, bool loop = true)
    {
        if (audioSource.clip == clip && audioSource.isPlaying)
            return;

        audioSource.Stop();
        audioSource.clip = clip;
        audioSource.loop = loop;
        if (clip != null)
            audioSource.Play();
    }
}