using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;

public class RestartOnKey : MonoBehaviour
{
    [Header("Restart Settings")]
    public float holdSeconds = 0.5f;

    [Header("UI")]
    public Image restartSprite;
    public TextMeshProUGUI restartText;

    private float holdTimer = 0f;

    private PlayerControlsB controls;
    private bool restartHeld;
    private bool didFullRestartThisHold;
    private bool isRestarting;

    private void Awake()
    {
        controls = new PlayerControlsB();

        if (restartSprite != null)
            restartSprite.fillAmount = 0f;

        SetTextAlpha(0f);
    }

    private void OnEnable()
    {
        controls.Player.Restart.started += OnRestartStarted;
        controls.Player.Restart.canceled += OnRestartCanceled;
        controls.Player.Enable();
    }

    private void OnDisable()
    {
        controls.Player.Restart.started -= OnRestartStarted;
        controls.Player.Restart.canceled -= OnRestartCanceled;
        controls.Player.Disable();
    }

    private void Update()
    {
        // Block restart UI while loading
        if (LevelLoader.Instance != null && LevelLoader.Instance.IsLoading)
        {
            ResetHoldVisualsImmediate();
            return;
        }

        if (restartHeld && !didFullRestartThisHold && !isRestarting)
        {
            holdTimer += Time.unscaledDeltaTime;

            if (restartSprite != null)
                restartSprite.fillAmount = Mathf.Clamp01(holdTimer / holdSeconds);

            if (holdTimer >= holdSeconds)
            {
                if (restartSprite != null)
                    restartSprite.fillAmount = 1f;

                didFullRestartThisHold = true;
                isRestarting = true;
                FullRestart();
            }
        }
        else
        {
            if (restartSprite != null)
            {
                restartSprite.fillAmount = Mathf.MoveTowards(
                    restartSprite.fillAmount,
                    0f,
                    Time.unscaledDeltaTime / Mathf.Max(holdSeconds, 0.01f)
                );
            }
        }

        if (!restartHeld && restartSprite != null && restartSprite.fillAmount <= 0f)
        {
            restartSprite.fillAmount = 0f;
            isRestarting = false;
        }

        float alpha = restartSprite != null ? restartSprite.fillAmount : 0f;
        SetTextAlpha(alpha);
    }

    private void OnRestartStarted(InputAction.CallbackContext _)
    {
        if (isRestarting) return;
        if (LevelLoader.Instance != null && LevelLoader.Instance.IsLoading) return;

        restartHeld = true;
        didFullRestartThisHold = false;
        holdTimer = 0f;
    }

    private void OnRestartCanceled(InputAction.CallbackContext _)
    {
        if (LevelLoader.Instance != null && LevelLoader.Instance.IsLoading)
        {
            ResetHoldVisualsImmediate();
            return;
        }

        // Tap = checkpoint respawn only if a full restart did not happen
        if (!didFullRestartThisHold && !isRestarting)
        {
            TryRespawnToCheckpoint();
        }

        restartHeld = false;
        holdTimer = 0f;
    }

    private void TryRespawnToCheckpoint()
    {
        if (!RunCheckpointState.HasCheckpoint) return;

        var mover = FindFirstObjectByType<NewThirdPlayerMovement>();
        if (mover == null) return;

        var rb = mover.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        mover.TeleportTo(RunCheckpointState.Position);

        var timer = FindFirstObjectByType<Timer>();
        if (timer != null)
            timer.SetTime(RunCheckpointState.SavedTime);

        ResetHoldVisualsImmediate();
    }

    private void FullRestart()
    {
        Time.timeScale = 1f;

        RunCheckpointState.Clear();

        var timer = FindFirstObjectByType<Timer>();
        if (timer != null)
            timer.ResetTimerAndSaveData();

        if (GameManager.Instance != null)
        {
            GameManager.Instance.newMap(GameManager.Instance.GetCurrentScene(), true);
        }

        ResetHoldVisualsImmediate();
    }

    private void ResetHoldVisualsImmediate()
    {
        restartHeld = false;
        holdTimer = 0f;
        didFullRestartThisHold = false;

        if (restartSprite != null)
            restartSprite.fillAmount = 0f;

        SetTextAlpha(0f);
    }

    private void SetTextAlpha(float normalizedAlpha)
    {
        if (restartText == null) return;

        byte alpha = (byte)(Mathf.Clamp01(normalizedAlpha) * 255f);
        Color32 c = restartText.color;
        c.a = alpha;
        restartText.color = c;
    }
}