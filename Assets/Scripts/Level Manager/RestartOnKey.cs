using UnityEngine;
using UnityEngine.InputSystem;

public class RestartOnKey : MonoBehaviour
{
    [Header("Restart Settings")]
    public float holdSeconds = 0.5f;

    private float holdTimer = 0f;

    // NEW INPUT SYSTEM
    private PlayerControlsB controls;
    private bool restartHeld;
    private bool didFullRestartThisHold;

    private void Awake()
    {
        controls = new PlayerControlsB();
    }

    private void OnEnable()
    {
        controls.Player.Restart.started += OnRestartStarted;   // button down
        controls.Player.Restart.canceled += OnRestartCanceled; // button up

        controls.Player.Enable();
    }

    private void OnDisable()
    {
        controls.Player.Restart.started -= OnRestartStarted;
        controls.Player.Restart.canceled -= OnRestartCanceled;

        controls.Player.Disable();
    }

    private void OnRestartStarted(InputAction.CallbackContext _)
    {
        restartHeld = true;
        didFullRestartThisHold = false;
        holdTimer = 0f;
    }

    private void OnRestartCanceled(InputAction.CallbackContext _)
    {
        // If we DIDN'T full-restart, treat this as a TAP
        if (!didFullRestartThisHold)
            TryRespawnToCheckpoint();

        restartHeld = false;
        holdTimer = 0f;
    }

    private void Update()
    {
        if (!restartHeld || didFullRestartThisHold) return;

        holdTimer += Time.unscaledDeltaTime;

        if (holdTimer >= holdSeconds)
        {
            didFullRestartThisHold = true; // set BEFORE calling, prevents edge cases
            FullRestart();
        }
    }

    private void TryRespawnToCheckpoint()
    {
        if (!RunCheckpointState.HasCheckpoint) return;

        var mover = FindFirstObjectByType<NewThirdPlayerMovement>();
        if (mover == null) return;

        mover.TeleportTo(RunCheckpointState.Position);

        var timer = FindFirstObjectByType<Timer>();
        if (timer != null)
            timer.SetTime(RunCheckpointState.SavedTime);
    }

    private void FullRestart()
    {
        Time.timeScale = 1f;

        // Clear checkpoint for fresh run
        RunCheckpointState.Clear();

        // Reset timer + wipe saved timer
        var timer = FindFirstObjectByType<Timer>();
        if (timer != null)
            timer.ResetTimerAndSaveData();

        GameManager.Instance.newMap(GameManager.Instance.GetCurrentScene(), true);
    }
}
