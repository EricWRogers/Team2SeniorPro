using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
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

    [Header("Audio(s)")]
    public AudioSource SFXSource;
    public AudioClip pauseSFX;
    public AudioClip clickSFX;

    [Header("Menu and Script(s)")]
    public GameObject pauseMenu;
    public GameManager GM;

    [Header("Controller / UI")]
    [Tooltip("PlayerInput component that owns the Player and UI action maps.")]
    public PlayerInput playerInput;

    [Tooltip("First UI object selected when the pause menu opens.")]
    public GameObject firstSelectedObject;

    [Header("Events")]
    [Tooltip("Scripts to disable when paused and enable when resumed.")]
    public MonoBehaviour[] scriptsToToggle;

    private bool uiWired;

    // New Input System
    private PlayerControlsB controls;
    private bool pausePressedThisFrame;

    private void Awake()
    {
        controls = new PlayerControlsB();

        if (move == null)
            move = FindFirstObjectByType<NewThirdPlayerMovement>();

        if (playerInput == null)
            playerInput = FindFirstObjectByType<PlayerInput>();

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
        if (controls == null)
            controls = new PlayerControlsB();

        // Pause action must exist in the Player action map.
        controls.Player.Pause.started += OnPauseStarted;
        controls.Player.Enable();

        ForceClosePauseMenu();
    }

    private void OnDisable()
    {
        if (controls != null)
        {
            controls.Player.Pause.started -= OnPauseStarted;
            controls.Player.Disable();
        }
    }

    private void Update()
    {
        if (LevelLoader.Instance != null && LevelLoader.Instance.IsLoading)
            return;

        if (pausePressedThisFrame)
        {
            pausePressedThisFrame = false;

            if (GameIsPaused)
                Resume();
            else
                Pause();
        }
    }

    private void OnPauseStarted(InputAction.CallbackContext _)
    {
        pausePressedThisFrame = true;
    }

    public void Home()
    {
        PlaySound();
        CloseForSceneChange();
        GameManager.Instance.newMap("Main Menu", true);
    }

    public void Resume()
    {
        if (pauseMenu != null)
            pauseMenu.SetActive(false);

        Time.timeScale = 1f;
        GameIsPaused = false;

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        // Return to gameplay action map.
        if (playerInput != null)
            playerInput.SwitchCurrentActionMap("Player");

        ToggleScripts(true);

        // Clear UI selection so menu focus is removed.
        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(null);
    }

    public void Pause()
    {
        // Safety: never open while loading
        if (LevelLoader.Instance != null && LevelLoader.Instance.IsLoading)
            return;

        if (pauseMenu != null)
            pauseMenu.SetActive(true);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Time.timeScale = 0f;
        GameIsPaused = true;

        // Disable gameplay scripts so player movement/interactions stop.
        ToggleScripts(false);

        // Switch to UI action map so controller can navigate the menu.
        if (playerInput != null)
            playerInput.SwitchCurrentActionMap("UI");

        RefreshOptionsUI();

        if (pauseSFX != null && SFXSource != null)
            SFXSource.PlayOneShot(pauseSFX);

        // Give controller focus to a button/toggle immediately.
        if (EventSystem.current != null && firstSelectedObject != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(firstSelectedObject);
        }
    }

    public void ToggleAudio()
    {
        if (audioToggle == null || SoundManager.Instance == null)
            return;

        PlaySound();
        SoundManager.Instance.SetMusicMuted(!audioToggle.isOn);
    }

    private void WireOptionsUI()
    {
        if (uiWired)
            return;

        if (sprintToggle != null)
            sprintToggle.onValueChanged.AddListener(OnSprintToggleChanged);

        if (crouchToggle != null)
            crouchToggle.onValueChanged.AddListener(OnCrouchToggleChanged);

        if (audioToggle != null)
            audioToggle.onValueChanged.AddListener(OnAudioToggleChanged);

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
            audioToggle.SetIsOnWithoutNotify(!SoundManager.Instance.IsMusicMuted());
    }

    private void OnSprintToggleChanged(bool on)
    {
        PlaySound();

        if (move != null)
            move.SetSprintToggleMode(on);
    }

    private void OnCrouchToggleChanged(bool on)
    {
        PlaySound();

        if (move != null)
            move.SetCrouchToggleMode(on);
    }

    private void OnAudioToggleChanged(bool _)
    {
        ToggleAudio();
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

        if (GM != null)
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
        Time.timeScale = 1f;

        if (pauseMenu != null)
            pauseMenu.SetActive(false);

        ToggleScripts(true);

        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(null);
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

        if (playerInput != null)
            playerInput.SwitchCurrentActionMap("Player");

        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(null);
    }

    private void ToggleScripts(bool enable)
    {
        if (scriptsToToggle == null)
            return;

        foreach (MonoBehaviour script in scriptsToToggle)
        {
            if (script != null)
                script.enabled = enable;
        }
    }
}