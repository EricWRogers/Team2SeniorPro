using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class LoseScreen : MonoBehaviour
{
    public GameObject gameOverUI;
    public GameObject D_Rank;
    public Animator D_animator;
    public PauseMenu pauseMenu;
    public AudioSource SFXSource;
    public AudioClip clickSFX;
    public AudioClip loserSFX;
    public Timer timer;

    [Header("Times Up Animation")]
    public Animator timesUpAnimator;

    [Header("Delay Settings")]
    public float loseDelay = 1.5f;

    [Header("Controller/UI")]
    public PlayerInput playerInput;
    public GameObject firstSelectedObject;
    public MonoBehaviour[] gameplayScriptsToDisable;

    public static bool GameIsPaused = false;

    public void GameOver()
    {
        if (timesUpAnimator != null)
            timesUpAnimator.SetTrigger("TimesUp");
        
        // Wait to show the lose screen until after the "Times Up" animation plays
        Invoke(nameof(ShowLoseScreen), loseDelay);

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        Time.timeScale = 0f;
        GameIsPaused = true;

        ToggleGameplayScripts(false);

        if (playerInput == null)
            playerInput = FindFirstObjectByType<PlayerInput>();

        if (playerInput != null)
            playerInput.SwitchCurrentActionMap("UI");

        if (pauseMenu != null)
        {
            pauseMenu.enabled = false;
            Debug.Log("Pause menu disabled");
        }

        if (SoundManager.Instance != null)
            SoundManager.Instance.SetMusicMuted(true);

        if (SFXSource != null && loserSFX != null)
        {
            SFXSource.PlayOneShot(loserSFX);
            Debug.Log("Played sound: " + loserSFX.name);
        }

        if (EventSystem.current != null && firstSelectedObject != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(firstSelectedObject);
        }
        
    }

    public void ShowLoseScreen()
    {
        gameOverUI.SetActive(true);
        D_Rank.SetActive(true);

        if (D_animator != null)
            D_animator.SetTrigger("D_Display");
    }

    public void Home()
    {
        PlaySound();
        Time.timeScale = 1f;
        GameManager.Instance.newMap("Squirrel_HUB", true);
    }

    public void Restart()
    {
        PlaySound();
        Time.timeScale = 1f;
        GameManager.Instance.newMap(GameManager.Instance.GetCurrentScene(), false);

        if (SoundManager.Instance != null)
            SoundManager.Instance.SetMusicMuted(false);
    }

    public void Quit()
    {
        PlaySound();
        Application.Quit();
        Debug.Log("You've quit the game!");
    }

    public void Stats()
    {
        Debug.Log("Loading Stats...");
        SceneManager.LoadScene("Stats Scene");
    }

    private void PlaySound()
    {
        if (clickSFX != null && SFXSource != null)
        {
            SFXSource.PlayOneShot(clickSFX);
            Debug.Log("Played sound: " + clickSFX.name);
        }
        else
        {
            Debug.LogWarning("ButtonSource or ButtonClip is missing!");
        }
    }

    private void ToggleGameplayScripts(bool enable)
    {
        if (gameplayScriptsToDisable == null) return;

        foreach (var script in gameplayScriptsToDisable)
        {
            if (script != null)
                script.enabled = enable;
        }
    }
}