using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;

public class RestartOnKey : MonoBehaviour
{
    [Header("Restart Settings")]
    public float holdSeconds = 0.5f;

    [Header("Sprite")]
    public Image restartSprite;
    public TextMeshProUGUI restartText;

    private float holdTimer = 0f;

    // NEW INPUT SYSTEM
    private PlayerControlsB controls;
    private bool restartHeld;
    private bool didFullRestartThisHold;

    private void Awake()
    {
        controls = new PlayerControlsB();
        restartSprite.fillAmount = 0f;
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
    // HOLDING
    if (restartHeld && !didFullRestartThisHold)
    {
        holdTimer += Time.unscaledDeltaTime;

        restartSprite.fillAmount = Mathf.Clamp01(holdTimer / holdSeconds);

        if (holdTimer >= holdSeconds)
        {
            restartSprite.fillAmount = 1f;
            didFullRestartThisHold = true;
            FullRestart();
        }
    }
    // NOT HOLDING → drain back to 0
    else
    {
        restartSprite.fillAmount = Mathf.MoveTowards(
            restartSprite.fillAmount,
            0f,
            Time.unscaledDeltaTime / holdSeconds
        );
    }

    // Syncs text alpha to current fill
    byte alpha = (byte)(restartSprite.fillAmount * 255f);

    Color32 c = restartText.color;
    c.a = alpha;
    restartText.color = c;
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
