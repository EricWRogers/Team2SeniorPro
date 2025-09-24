using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class PauseMenu : MonoBehaviour
{
    public static bool GameIsPaused = false;

    [Header("Audio(s):")]
    public AudioSource audioSource;
    public AudioSource SFXSource;
    public AudioClip pauseSFX;
    public AudioClip clickSFX;

    [Header("Menu and Script(s):")]
    public GameObject pauseMenu;

    public void Home()
    {
        PlaySound();
        Debug.Log("Loading Main Menu...");
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex - 1);
    }
    public void Resume()
    {
        PlaySound();
        StartCoroutine(WaitForPlay());
        pauseMenu.SetActive(false);
        Time.timeScale = 1f;
        GameIsPaused = false;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void Restart()
    {
        PlaySound();
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    public void Quit()
    {
        PlaySound();
        Application.Quit();
        Debug.Log("You've quit the game!");
    }

    public void ToggleAudio()
    {
        if (audioSource != null)
        {
            audioSource.mute = !audioSource.mute; // Toggles the mute state
        }
        else
        {
            Debug.LogWarning("AudioSource not assigned to AudioToggler script.");
        }
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
    void Pause()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        pauseMenu.SetActive(true);
        Time.timeScale = 0f;
        GameIsPaused = true;

        // Play clip once
        if (pauseSFX != null && SFXSource != null)
        {
            SFXSource.PlayOneShot(pauseSFX);
        }
    }

    private IEnumerator WaitForPlay()
    {
        yield return new WaitForSecondsRealtime(0.05f); // tiny delay so sound registers
    }
}
