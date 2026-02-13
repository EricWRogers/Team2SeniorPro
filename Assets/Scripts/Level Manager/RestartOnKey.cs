using UnityEngine;

public class RestartOnKey : MonoBehaviour
{
    [Header("Restart Settings")]
    public float holdSeconds = 0.5f; // set to whatever feels right

    private float holdTimer = 0f;

    //NEW INPUT SYSTEM
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
            // If we dont full restart, treat this as a TAP
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

        // Reset timer + wipe saved timer (if still using PlayerPrefs there)
        var timer = FindFirstObjectByType<Timer>();
        if (timer) timer.ResetTimerAndSaveData();

        GameManager.Instance.newMap(GameManager.Instance.GetCurrentScene(), true);
    }
}
