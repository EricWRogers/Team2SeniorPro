using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class RestartOnKey : MonoBehaviour
{
    [Header("Restart Settings")]
    [Tooltip("Hold this action (mapped in Input Actions) to restart.")]
    public string restartActionName = "Restart"; // the action name from your Input Actions

    [Tooltip("Hold the key for this long to restart. Set to 0 for instant.")]
    public float holdSeconds = 0f;

    private float holdTimer = 0f;

    // --- NEW INPUT SYSTEM ---
    private PlayerControlsB controls;
    private bool restartHeld;

    private ThirdPersonMovement tpm;

    private void Awake()
    {
        controls = new PlayerControlsB();
    }

    private void OnEnable()
    {
        // Bind the restart input
        controls.Player.Restart.performed += ctx => restartHeld = true;
        controls.Player.Restart.canceled += ctx => restartHeld = false;

        controls.Player.Enable();
    }

    private void OnDisable()
    {
        controls.Player.Disable();
    }

    private void Update()
    {
        // instant restart
        if (holdSeconds <= 0f)
        {
            if (restartHeld)
            {
                Restart();
                restartHeld = false; // prevent multiple triggers
            }
            return;
        }

        // hold to restart
        if (restartHeld)
        {
            holdTimer += Time.unscaledDeltaTime;
            if (holdTimer >= holdSeconds)
            {
                Restart();
                restartHeld = false;
                holdTimer = 0f;
            }
        }
        else
        {
            holdTimer = 0f;
        }
    }

    private void Restart()
    {
        Time.timeScale = 1f;
        GameManager.Instance.newMap(GameManager.Instance.GetCurrentScene(), true);
        tpm.ResetMovementState();
    }
}


