using UnityEngine;

public class RestartOnKey : MonoBehaviour
{
    [Header("Restart Settings")]
    public float holdSeconds = 0.5f; // set to whatever feels right

    private float holdTimer = 0f;

    // --- NEW INPUT SYSTEM ---
    private PlayerControlsB controls;
    private bool restartHeld;
    private bool didFullRestartThisHold;

    private void Awake()
    {
        controls = new PlayerControlsB();
    }

    private void OnEnable()
    {
        controls.Player.Restart.performed += ctx =>
        {
            restartHeld = true;
            didFullRestartThisHold = false;
            holdTimer = 0f;
        };

        controls.Player.Restart.canceled += ctx =>
        {
            // If we DIDN'T full-restart, treat this as a TAP
            if (!didFullRestartThisHold)
            {
                TryRespawnToCheckpoint();
            }

            restartHeld = false;
            holdTimer = 0f;
        };

        controls.Player.Enable();
    }

    private void OnDisable()
    {
        controls.Player.Disable();
    }

    private void Update()
    {
        if (!restartHeld || didFullRestartThisHold) return;

        holdTimer += Time.unscaledDeltaTime;

        if (holdTimer >= holdSeconds)
        {
            FullRestart();
            didFullRestartThisHold = true;
        }
    }

    /*private void TryRespawnToCheckpoint()
    {
        if (!RunCheckpointState.HasCheckpoint) return;

        var tagged = GameObject.FindWithTag("Player");
        if (!tagged) return;

        Vector3 targetPos = RunCheckpointState.Position;

        // Find the mover on this object or its parents (handles tag-on-child case)
        var mover = tagged.GetComponent<NewThirdPlayerMovement>();
        if (mover != null && mover.rb != null)
        {
            Vector3 targetPos = RunCheckpointState.Position + Vector3.up * 0.2f;

            var rb = mover.rb;

            // Stop motion
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            // Temporarily disable interpolation so it doesn't "smooth back"
            var oldInterp = rb.interpolation;
            rb.interpolation = RigidbodyInterpolation.None;

            // Teleport
            rb.position = targetPos;
            rb.rotation = Quaternion.identity; // OPTIONAL: remove if you want to keep rotation

            Physics.SyncTransforms();
            rb.WakeUp();

            // Restore interpolation
            rb.interpolation = oldInterp;

            // Reset mover states that can apply forces instantly
            mover.sliding = false;
            mover.crouching = false;
            mover.wallrunning = false;
            mover.climbing = false;
            mover.vaulting = false;
        }



        // Rewind timer
        var timer = FindFirstObjectByType<Timer>();
        if (timer) timer.SetTime(RunCheckpointState.SavedTime);
    }*/

    private void TryRespawnToCheckpoint()
    {
        if (!RunCheckpointState.HasCheckpoint) return;

        var mover = FindFirstObjectByType<NewThirdPlayerMovement>();
        if (mover == null) return;

        Vector3 targetPos = RunCheckpointState.Position;

        mover.TeleportTo(targetPos);

        var timer = FindFirstObjectByType<Timer>();
        if (timer) timer.SetTime(RunCheckpointState.SavedTime);
    }




    private void FullRestart()
    {
        Time.timeScale = 1f;

        // Clear checkpoint for fresh run
        RunCheckpointState.Clear();

        // Reset timer + wipe saved timer (if you’re still using PlayerPrefs there)
        var timer = FindFirstObjectByType<Timer>();
        if (timer) timer.ResetTimerAndSaveData();

        GameManager.Instance.newMap(GameManager.Instance.GetCurrentScene(), true);
    }
}
