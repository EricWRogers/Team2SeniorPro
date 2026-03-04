using UnityEngine;
using System.Collections;
using UnityEngine.UI; 

public class PauseMenu : MonoBehaviour
{
    public static bool GameIsPaused = false;

    [Header("Options Toggles")]
    public Toggle sprintToggle;
    public Toggle crouchToggle;

    [Tooltip("Leave empty to auto-find on the player.")]
    public NewThirdPlayerMovement move;

    [Header("Audio(s):")]
    public AudioSource SFXSource;
    public AudioClip pauseSFX;
    public AudioClip clickSFX;

    [Header("Menu and Script(s):")]
    public GameObject pauseMenu;

    [Header("Events")]
    [Tooltip("Scripts to disable when paused and enable when resumed")]
    public MonoBehaviour[] scriptsToToggle;

    private bool uiWired;

    private void Awake()
    {
        // Auto-find movement if not assigned
        if (move == null)
            move = FindFirstObjectByType<NewThirdPlayerMovement>();

        WireOptionsUI();
        RefreshOptionsUI();
    }

    void Update()
    {
        // (Optional) convert this to the Input System later
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (GameIsPaused) Resume();
            else Pause();
        }
    }

    public void Resume()
    {
        StartCoroutine(WaitForPlay());
        pauseMenu.SetActive(false);
        Time.timeScale = 1f;
        GameIsPaused = false;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        ToggleScripts(true);
    }

    public void Pause()
    {
        pauseMenu.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Time.timeScale = 0f;
        GameIsPaused = true;
        ToggleScripts(false);

        // Always resync UI when opening menu (in case prefs changed elsewhere)
        RefreshOptionsUI();

        if (pauseSFX != null && SFXSource != null)
            SFXSource.PlayOneShot(pauseSFX);
    }

    private void WireOptionsUI()
    {
        if (uiWired) return;

        if (sprintToggle != null)
            sprintToggle.onValueChanged.AddListener(OnSprintToggleChanged);

        if (crouchToggle != null)
            crouchToggle.onValueChanged.AddListener(OnCrouchToggleChanged);

        uiWired = true;
    }

    private void RefreshOptionsUI()
    {
        if (move == null) return;

        // Prevent triggering OnValueChanged while we programmatically set values
        if (sprintToggle != null)
        {
            sprintToggle.SetIsOnWithoutNotify(move.sprintToggleMode);
        }

        if (crouchToggle != null)
        {
            crouchToggle.SetIsOnWithoutNotify(move.crouchToggleMode);
        }
    }

    private void OnSprintToggleChanged(bool on)
    {
        PlaySound();
        if (move != null) move.SetSprintToggleMode(on);
    }

    private void OnCrouchToggleChanged(bool on)
    {
        PlaySound();
        if (move != null) move.SetCrouchToggleMode(on);
    }

    public void Restart()
    {
        Time.timeScale = 1f;
        GameManager.Instance.newMap(GameManager.Instance.GetCurrentScene(), false);
    }

    public void Quit()
    {
        PlaySound();
        Application.Quit();
        Debug.Log("You've quit the game!");
    }

    public void PlaySound()
    {
        if (clickSFX != null && SFXSource != null)
            SFXSource.PlayOneShot(clickSFX);
    }

    private IEnumerator WaitForPlay()
    {
        yield return new WaitForSecondsRealtime(2.0f);
    }

    private void ToggleScripts(bool enable)
    {
        if (scriptsToToggle == null) return;

        foreach (var script in scriptsToToggle)
        {
            if (script != null)
                script.enabled = enable;
        }
    }
}