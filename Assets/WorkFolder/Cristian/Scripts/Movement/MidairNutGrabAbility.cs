using UnityEngine;
using System.Collections;
using TMPro;

public class MidairGrabAbility : MonoBehaviour
{
    [Header("References")]
    public PlayerCarryState carryState;
    public Rigidbody playerRb;
    [Tooltip("Where the thrown object should attach when picked up")]
    public Transform carrySocket;
    [Tooltip("movement script; will be frozen during homing if assigned")]
    public ThirdPersonMovement tpm;

    [Header("Homing Settings")]
    public float homingSpeed = 12f;
    public float homingDuration = 0.35f;
    public float catchDistance = 1.0f;
    public float searchRadius = 15f;

    [Header("Slow Motion Settings")]
    public float slowMoTime = 2f;
    public float slowMoScale = 0.3f;

    [Header("Nut Jump Settings")]
    public int maxNutJumps = 1;            // Base number of nut jumps
    public int currentNutJumps;           // Remaining nut jumps
    public float nutJumpCooldown = 3f;     // Cooldown duration
    private float nutJumpCooldownTimer = 0f;

    [Header("Double Jump Settings")]
    public float doubleJumpForce = 10f;

    private bool canDoubleJump = false;
    private bool isSlowingTime = false;
    private bool isHoming = false;
    private Coroutine homingCoroutine;

    [Header("Particles")]
    public ParticleSystem grabParticles;

    [Header("Animations")]
    public Animator animator;
    public AudioClip slowMoSfx;
    public AudioClip homingSfx;

    [Header("UI")]
    public TextMeshProUGUI nutJumpTimerText;

    void Start()
    {
        currentNutJumps = maxNutJumps; // initialize
    }

    void Update()
    {
        // Cooldown timer
        if (nutJumpCooldownTimer > 0f)
        {
            nutJumpCooldownTimer -= Time.deltaTime;
            if (nutJumpCooldownTimer <= 0f)
            {
                // Reset nut jumps when cooldown expires
                currentNutJumps = maxNutJumps;
            }
        }

        // Update TMP UI
        if (nutJumpTimerText)
        {
            if (nutJumpCooldownTimer > 0f)
                nutJumpTimerText.text = $"Nut Jump CD: {nutJumpCooldownTimer:F1}s";
            else
                nutJumpTimerText.text = "Nut Jump Ready!";
        }

        // Handle input
        if (Input.GetKeyDown(KeyCode.Space) && !IsGrounded())
        {
            if (canDoubleJump && currentNutJumps > 0)
            {
                DoNutJump();
                return;
            }

            if (!isHoming && currentNutJumps > 0)
            {
                CarryableAcorn target = FindNearestThrownAcorn(searchRadius);
                if (target != null)
                {
                    homingCoroutine = StartCoroutine(HomingToAcorn(target));
                }
            }
        }
    }

    void DoNutJump()
    {
        playerRb.linearVelocity = new Vector3(playerRb.linearVelocity.x, 0f, playerRb.linearVelocity.z);
        playerRb.AddForce(Vector3.up * doubleJumpForce, ForceMode.Impulse);

        canDoubleJump = false;
        currentNutJumps--;                  // Consume a nut jump
        nutJumpCooldownTimer = nutJumpCooldown; // Start cooldown

        ResetTime();
    }

    IEnumerator HomingToAcorn(CarryableAcorn target)
    {
        if (target == null) yield break;

        isHoming = true;
        if (tpm) tpm.freeze = true;

        float elapsed = 0f;
        bool targetIsMidairEligible = target.IsAvailableForMidairCatch();

        while (elapsed < homingDuration && target != null && !target.IsCarried)
        {
            Vector3 targetPos = target.rb.position;
            float dist = Vector3.Distance(playerRb.position, targetPos);

            if (dist <= catchDistance)
            {
                target.PickUp(carrySocket, ignoreCooldown: true);

                if (target.IsCarried && carryState != null) carryState.SetCarrying(true);

                if (targetIsMidairEligible)
                {
                    ActivateSlowMo();
                    canDoubleJump = true;
                }
                break;
            }

            Vector3 nextPos = Vector3.MoveTowards(playerRb.position, targetPos, homingSpeed * Time.deltaTime);
            playerRb.MovePosition(nextPos);

            elapsed += Time.deltaTime;
            yield return null;

            //if (homingSfx)
        }

        if (tpm) tpm.freeze = false;
        isHoming = false;
    }

    CarryableAcorn FindNearestThrownAcorn(float radius)
    {
        CarryableAcorn[] all = FindObjectsByType<CarryableAcorn>(FindObjectsSortMode.None);

        CarryableAcorn best = null;
        float bestDist = radius;
        Vector3 p = playerRb.position;

        foreach (var a in all)
        {
            if (!a.IsAvailableForMidairCatch()) continue;
            float d = Vector3.Distance(p, a.transform.position);
            if (d < bestDist)
            {
                bestDist = d;
                best = a;
            }
        }
        return best;
    }

    void ActivateSlowMo()
    {
        if (!isSlowingTime)
        {
            Time.timeScale = slowMoScale;
            Time.fixedDeltaTime = 0.02f * Time.timeScale;
            isSlowingTime = true;

            if (slowMoSfx) AudioSource.PlayClipAtPoint(slowMoSfx, transform.position); //makes the sfx play at the players position
            if (animator) animator.SetTrigger("MidairGrab");  //fix this
            if (grabParticles) grabParticles.Play();

            Invoke(nameof(ResetTime), slowMoTime);
        }
    }

    void ResetTime()
    {
        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f;
        isSlowingTime = false;
    }

    bool IsGrounded()
    {
        return Physics.Raycast(transform.position, Vector3.down, 1.1f);
    }

    // Call this from upgrades to increase max jumps
    public void UpgradeNutJumps(int amount)
    {
        maxNutJumps += amount;
        currentNutJumps = maxNutJumps;
    }
}
