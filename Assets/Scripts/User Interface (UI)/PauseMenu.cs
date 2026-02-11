using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class PauseMenu : MonoBehaviour
{
    public static bool GameIsPaused = false;

    [Header("Audio(s):")]
    public AudioSource SFXSource;
    public AudioClip pauseSFX;
    public AudioClip clickSFX;

    [Header("Menu and Script(s):")]
    public GameObject pauseMenu;

    [Header("Events")]
    [Tooltip("Scripts to disable when paused and enable when resumed")]
    public MonoBehaviour[] scriptsToToggle;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (GameIsPaused)
            {
                Resume();
            }
            else
            {
                Pause();
            }
        }
    }

    public void Home()
    {
        PlaySound();
        Debug.Log("Loading Main Menu...");
        Time.timeScale = 1f;
        GameManager.Instance.newMap("Main Menu", true); //loads the main menu, resets collectibles so it doesnt add 0 to total

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.SetMusicMuted(true);
        }
    }

    public void HUB()
    {
        PlaySound();
        Debug.Log("Loading Burrow...");
        Time.timeScale = 1f;
        GameManager.Instance.newMap("Squirrel_HUB", true); //loads the burrow, resets collectibles so it doesnt add 0 to total
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

        // Play clip once
        if (pauseSFX != null && SFXSource != null)
        {
            SFXSource.PlayOneShot(pauseSFX);
        }
    }

    public void Restart()
    {
        Time.timeScale = 1f;
        GameManager.Instance.newMap(GameManager.Instance.GetCurrentScene(), false); //reloads the current scene, does not reset collectibles so it adds to total
    }
    
    public void Quit()
    {
        PlaySound();
        Application.Quit();
        Debug.Log("You've quit the game!");
    }

    public void ToggleAudio()
    {
        if (SoundManager.Instance == null)
        {
            Debug.LogWarning("No SoundManager found!");
            return;
        }

        bool mute = !SoundManager.Instance.IsMusicMuted();
        SoundManager.Instance.SetMusicMuted(mute);

        Debug.Log(mute ? "Music muted." : "Music unmuted.");
    }

    public void PlaySound()
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

    private IEnumerator WaitForPlay()
    {
        yield return new WaitForSecondsRealtime(2.0f); // tiny delay so sound registers
    }

    private void ToggleScripts(bool enable)
    {
        if (scriptsToToggle != null)
        {
            foreach (var script in scriptsToToggle)
            {
                if (script != null)
                    script.enabled = enable;
            }
        }
    }
}
