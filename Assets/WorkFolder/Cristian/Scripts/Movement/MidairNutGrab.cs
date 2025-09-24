using UnityEngine;
using System.Collections;

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
    public float homingSpeed = 12f;      // how quickly player moves toward acorn
    public float homingDuration = 0.35f; // max time to attempt homing
    public float catchDistance = 1.0f;   // distance at which we force the pickup
    public float searchRadius = 15f;     // how far to look for a thrown acorn, edit for linger rng

    [Header("Slow Motion Settings")]
    public float slowMoTime = 2f;
    public float slowMoScale = 0.3f;

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
    private AudioClip slowMoSfx;

    void Update()
    {
        // When player presses jump midair...
        if (Input.GetKeyDown(KeyCode.Space) && !IsGrounded())
        {
            // If player already has a granted midair double jump, consume it :)
            if (canDoubleJump)
            {
                DoDoubleJump();
                return;
            }

            // otherwise attempt homing toward a recently-thrown acorn
            if (!isHoming)
            {
                CarryableAcorn target = FindNearestThrownAcorn(searchRadius);
                if (target != null)
                {
                    homingCoroutine = StartCoroutine(HomingToAcorn(target));
                }
            }
        }
    }

    void DoDoubleJump()
    {
        playerRb.linearVelocity = new Vector3(playerRb.linearVelocity.x, 0f, playerRb.linearVelocity.z);
        playerRb.AddForce(Vector3.up * doubleJumpForce, ForceMode.Impulse);
        canDoubleJump = false;
        ResetTime();
    }

    IEnumerator HomingToAcorn(CarryableAcorn target)
    {
        if (target == null) yield break;

        isHoming = true;
        if (tpm) tpm.freeze = true; // optional may remvoe: freeze normal movement controls

        float elapsed = 0f;
        // capture midair eligibility before we call PickUp (it will reset on pickup)
        bool targetIsMidairEligible = target.IsAvailableForMidairCatch();

        while (elapsed < homingDuration && target != null && !target.IsCarried)
        {
            Vector3 targetPos = target.rb.position;
            Vector3 dir = (targetPos - playerRb.position);
            float dist = dir.magnitude;

            if (dist <= catchDistance)
            {
                // Force pickup and bypass the normal short no-catch cooldown that DropAndThrow sets
                target.PickUp(carrySocket, ignoreCooldown: true);

                // Only trigger effects if pickup actually succeeded AND the pickup is a midair pickup
                // Use the midair eligibility we checked before calling PickUp
                if (target.IsCarried)
                {
                    // set player to carrying state
                    if (carryState != null) carryState.SetCarrying(true);

                    if (targetIsMidairEligible)
                    {
                        // grant slow-mo and midair double jump
                        ActivateSlowMo();
                        canDoubleJump = true;
                    }
                }

                break;
            }

            // Move player toward acorn smoothly
            // Using MovePosition keeps physics interaction consistent
            Vector3 nextPos = Vector3.MoveTowards(playerRb.position, targetPos, homingSpeed * Time.deltaTime);
            playerRb.MovePosition(nextPos);

            elapsed += Time.deltaTime;
            yield return null;
        }

        // done
        if (tpm) tpm.freeze = false;
        isHoming = false;
    }

    // find nearest acorn which is recently thrown and still available for midair catch
    
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
            Time.fixedDeltaTime = 0.02f * Time.timeScale; // physics stays synced
            isSlowingTime = true;

            // Sound
            if (slowMoSfx) AudioSource.PlayClipAtPoint(slowMoSfx, transform.position);

            // Animation
            if (animator) animator.SetTrigger("MidairGrab");

            // Particles
            if (grabParticles) grabParticles.Play /*Instantiate(grabParticles, transform.position, Quaternion.identity);*/();

            Invoke(nameof(ResetTime), slowMoTime);
        }
    }

    void ResetTime()
    {
        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f; // reset physics timestep
        isSlowingTime = false;
    }

    bool IsGrounded()
    {   
        return Physics.Raycast(transform.position, Vector3.down, 1.1f);
    }
}
