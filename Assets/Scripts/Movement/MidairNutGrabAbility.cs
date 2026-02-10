using UnityEngine;
using System.Collections;
using TMPro;
using UnityEngine.InputSystem;
public class MidairGrabAbility : MonoBehaviour
{
    [Header("References")]
    public PlayerCarryState carryState;
    public Rigidbody playerRb;
    [Tooltip("Where the thrown object should attach when picked up")]
    public Transform carrySocket;
    [Tooltip("movement script; will be frozen during homing if assigned")]
    public NewThirdPlayerMovement tpm;

    [Header("Homing Settings")]
    public float homingSpeed = 12f;
    public float homingDuration = 0.35f;
    public float catchDistance = 1.0f;
    public float searchRadius = 15f;

    [Header("Slow Motion Settings")]
    public float slowMoTime = 2f;
    public float slowMoScale = 0.3f;

    [Header("Nut Jump Settings")]
    public int maxNutJumps = 1;
    public int currentNutJumps;
    public float nutJumpCooldown = 3f;
    private float nutJumpCooldownTimer;

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
    public Stamina staminaBar;

    //New Input System
    private PlayerControlsB controls;
    private bool jumpPressed;

    void Awake()
    {
        controls = new PlayerControlsB();

        // Handle Jump input
        controls.Player.Jump.performed += ctx => jumpPressed = true;
        controls.Player.Jump.canceled += ctx => jumpPressed = false;
    }

    void OnEnable() => controls.Player.Enable();
    void OnDisable() => controls.Player.Disable();

    void Start()
    {
        currentNutJumps = maxNutJumps;
    }

    void Update()
    {
        // Cooldown timer
        if (nutJumpCooldownTimer > 0f)
        {
            nutJumpCooldownTimer -= Time.deltaTime;
            if (nutJumpCooldownTimer <= 0f)
            {
                currentNutJumps = maxNutJumps; // reset jumps
            }
        }

        // TMP UI
        if (nutJumpTimerText)
        {
            nutJumpTimerText.text = nutJumpCooldownTimer > 0f
                ? $"Nut Jump CD: {nutJumpCooldownTimer:F1}s"
                : "Nut Jump Ready!";
        }

        // ✅ Handle Jump Input (keyboard + controller)
        if (jumpPressed && !IsGrounded())
        {
            // Consume input immediately
            jumpPressed = false;

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
        if (staminaBar != null && !staminaBar.IsFull())
            return;

        playerRb.linearVelocity = new Vector3(playerRb.linearVelocity.x, 0f, playerRb.linearVelocity.z);
        playerRb.AddForce(Vector3.up * doubleJumpForce, ForceMode.Impulse);

        canDoubleJump = false;
        currentNutJumps--;
        nutJumpCooldownTimer = nutJumpCooldown;

        // Drain stamina fully
        staminaBar?.DrainToZero();

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

                if (target.IsCarried && carryState != null)
                    carryState.SetCarrying(true);

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
        if (isSlowingTime) return;

        Time.timeScale = slowMoScale;
        Time.fixedDeltaTime = 0.02f * Time.timeScale;
        isSlowingTime = true;

        if (slowMoSfx) AudioSource.PlayClipAtPoint(slowMoSfx, transform.position);
        if (animator) animator.SetTrigger("MidairGrab");
        if (grabParticles) grabParticles.Play();

        Invoke(nameof(ResetTime), slowMoTime);
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

    public void UpgradeNutJumps(int amount)
    {
        maxNutJumps += amount;
        currentNutJumps = maxNutJumps;
    }
}
