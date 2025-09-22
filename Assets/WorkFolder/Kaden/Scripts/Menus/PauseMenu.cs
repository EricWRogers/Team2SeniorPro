using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    public static bool GameIsPaused = false;

    [Header("Audio(s):")]
    public AudioSource audioSource;
    public AudioClip buttonClip;
    public AudioSource sfxSource;
    public AudioClip pauseSFX;
    public AudioSource buttonSource;

    [Header("Menu and Script(s):")]
    public GameObject pauseMenu;

    public void Home()
    {
        Debug.Log("Loading Main Menu...");
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex - 1);
    }
    public void Resume()
    {
        pauseMenu.SetActive(false);
        Time.timeScale = 1f;
        GameIsPaused = false;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void PlaySound(AudioClip buttonClip)
    {
        if (buttonClip != null)
            buttonSource.PlayOneShot(buttonClip);
    }

    public void Restart()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }    
    public void Quit()
    {
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
        if (pauseSFX != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(pauseSFX);
        }
    }
}
