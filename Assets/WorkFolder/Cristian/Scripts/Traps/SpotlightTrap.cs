using UnityEngine;

public class SpotlightTrap : MonoBehaviour
{
    [Header("Targets")]
    public Transform playerTarget;
    public float detectionRadius = 30f;
    public float visionAngle = 45f; // Cone of vision in degrees
    public LayerMask visionBlockMask; // Assign walls/obstacles here

    [Header("Spotlight Settings")]
    public Light spotLight;
    public Color normalColor = Color.white;
    public Color observingColor = Color.red;
    public Color curiousColor = Color.yellow;
    public float rotationSpeed = 2f;
    public float searchAngle = 270f;
    public float searchSpeed = 2f;

    [Header("Debuff Settings")]
    public float countdownTime = 3f;
    private float countdownTimer;
    private bool debuffGiven = false;

    [Header("Audio Settings")]
    public AudioSource audioSource;
    public AudioClip searchClip;
    public AudioClip playerClip;
    public AudioClip curiosityClip;

    private enum SpotState { Searching, Curious, PlayerViewing }
    private SpotState currentState = SpotState.Searching;

    private float searchTimer = 0f;
    private Transform curiosityTarget; // store nut distraction

    void Start()
    {
        if (spotLight == null)
            spotLight = GetComponentInChildren<Light>();
    }

    void Update()
    {
        // First check for player
        if (playerTarget != null && IsInDetectionRange(playerTarget, detectionRadius) && CanSee(playerTarget))
        {
            currentState = SpotState.PlayerViewing;
            PlayerFocus();
            return;
        }

        // If no player, check for throwable distraction
        curiosityTarget = FindNearestThrowable();
        if (curiosityTarget != null && IsInDetectionRange(curiosityTarget, detectionRadius) && CanSee(curiosityTarget))
        {
            currentState = SpotState.Curious;
            CuriosityFocus();
            return;
        }

        // Otherwise, search mode
        currentState = SpotState.Searching;
        SearchMode();
    }

    // --------------------------
    // STATES
    // --------------------------

    private void PlayerFocus()
    {
        spotLight.color = observingColor;
        RotateTowards(playerTarget.position);
        PlayClip(playerClip);

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

    private void CuriosityFocus()
    {
        spotLight.color = curiousColor;
        RotateTowards(curiosityTarget.position);
        PlayClip(curiosityClip);

        // Reset player debuff timer while distracted
        countdownTimer = countdownTime;
        debuffGiven = false;
    }

    private void SearchMode()
    {
        spotLight.color = normalColor;
        PlayClip(searchClip);

        countdownTimer = countdownTime;
        debuffGiven = false;

        // Simple oscillating search
        searchTimer += Time.deltaTime * searchSpeed;
        float angle = Mathf.Sin(searchTimer) * searchAngle;
        Quaternion rotation = Quaternion.Euler(0f, angle, 0f);
        transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * rotationSpeed);
    }

    // --------------------------
    // HELPERS
    // --------------------------

    private void RotateTowards(Vector3 targetPos)
    {
        Vector3 dir = (targetPos - transform.position).normalized;
        Quaternion lookRot = Quaternion.LookRotation(dir, Vector3.up);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, Time.deltaTime * rotationSpeed);
    }

    private bool IsInDetectionRange(Transform target, float range)
    {
        return Vector3.Distance(transform.position, target.position) <= range;
    }

    private bool CanSee(Transform target)
    {
        Vector3 dirToTarget = (target.position - transform.position).normalized;
        float distToTarget = Vector3.Distance(transform.position, target.position);

        // Check cone of vision
        float angleToTarget = Vector3.Angle(transform.forward, dirToTarget);
        if (angleToTarget > visionAngle * 0.5f) return false;

        // Check line of sight (walls etc.)
        if (Physics.Raycast(transform.position, dirToTarget, out RaycastHit hit, distToTarget, visionBlockMask))
        {
            return false; // blocked
        }

        return true;
    }

    private Transform FindNearestThrowable()
    {
        GameObject[] throwables = GameObject.FindGameObjectsWithTag("Acorn");
        Transform nearest = null;
        float minDist = Mathf.Infinity;

        foreach (var obj in throwables)
        {
            float dist = Vector3.Distance(transform.position, obj.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                nearest = obj.transform;
            }
        }
        return nearest;
    }

    private void ApplyRandomDebuff()
    {
        int debuffType = Random.Range(0, 3);
        switch (debuffType)
        {
            case 0:
                Debug.Log("Debuff: Slowed movement");
                break;
            case 1:
                Debug.Log("Debuff: Reduced vision");
                break;
            case 2:
                Debug.Log("Debuff: Health drain");
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
