using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using Unity.VisualScripting;

public class PauseMenu : MonoBehaviour
{
    public static bool GameIsPaused = false;

    [Header("Options Toggles")]
    public Toggle sprintToggle;
    public Toggle crouchToggle;
    public Toggle audioToggle;

    [Tooltip("Leave empty to auto-find on the player.")]
    public NewThirdPlayerMovement move;

    [Header("Audio(s):")]
    public AudioSource SFXSource;
    public AudioClip pauseSFX;
    public AudioClip clickSFX;

    [Header("Menu and Script(s):")]
    public GameObject pauseMenu;
    public GameManager GM;

    [Header("Events")]
    [Tooltip("Scripts to disable when paused and enable when resumed")]
    public MonoBehaviour[] scriptsToToggle;

    private bool uiWired;

    private void Awake()
    {
        if (move == null)
            move = FindFirstObjectByType<NewThirdPlayerMovement>();

        if (GM == null)
        {
            GM = FindFirstObjectByType<GameManager>();
            if (GM == null)
                Debug.LogError("No GameManager found in scene!");
        }

        WireOptionsUI();
        RefreshOptionsUI();
        ForceClosePauseMenu();
    }

    private void OnEnable()
    {
        ForceClosePauseMenu();
    }

    void Update()
    {
        if (LevelLoader.Instance != null && LevelLoader.Instance.IsLoading)
            return;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (GameIsPaused) Resume();
            else Pause();
        }
    }
    public void Home()
    {
        PlaySound();
        CloseForSceneChange();
        GameManager.Instance.newMap("Main Menu", true);
    }

    public void HUB()
    {
        PlaySound();
        CloseForSceneChange();
        GameManager.Instance.newMap("Squirrel_HUB", true);
    }

    public void Resume()
    {
        pauseMenu.SetActive(false);
        Time.timeScale = 1f;
        GameIsPaused = false;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        ToggleScripts(true);
    }

    public void Pause()
    {
        // Extra safety: never open while loading
        if (LevelLoader.Instance != null && LevelLoader.Instance.IsLoading)
            return;

        pauseMenu.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Time.timeScale = 0f;
        GameIsPaused = true;
        ToggleScripts(false);

        RefreshOptionsUI();

        if (pauseSFX != null && SFXSource != null)
            SFXSource.PlayOneShot(pauseSFX);
    }

    public void ToggleAudio()
    {
        if (audioToggle == null || SoundManager.Instance == null) return;

        PlaySound();
        SoundManager.Instance.SetMusicMuted(!audioToggle.isOn);
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
        if (move != null)
        {
            if (sprintToggle != null)
                sprintToggle.SetIsOnWithoutNotify(move.sprintToggleMode);

            if (crouchToggle != null)
                crouchToggle.SetIsOnWithoutNotify(move.crouchToggleMode);
        }

        if (audioToggle != null && SoundManager.Instance != null)
        {
            audioToggle.SetIsOnWithoutNotify(!SoundManager.Instance.IsMusicMuted());
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
        CloseForSceneChange();
        GameManager.Instance.newMap(GameManager.Instance.GetCurrentScene(), false);
    }

    public void Quit()
    {
        PlaySound();
        Application.Quit();
        Debug.Log("You've quit the game!");
    }

    public void ResetData()
    {
        PlaySound();
        GM.ResetBerryData();
        Debug.Log("All progress reset. Berry data cleared.");
    }

    public void PlaySound()
    {
        if (clickSFX != null && SFXSource != null)
            SFXSource.PlayOneShot(clickSFX);
    }

    public void ForceClosePauseMenu()
    {
        GameIsPaused = false;

        if (pauseMenu != null)
            pauseMenu.SetActive(false);

        ToggleScripts(true);
    }

    public void CloseForSceneChange()
    {
        GameIsPaused = false;
        Time.timeScale = 1f;

        if (pauseMenu != null)
            pauseMenu.SetActive(false);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        ToggleScripts(true);
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