using UnityEngine;
using UnityEngine.SceneManagement;

public class LoseScreen : MonoBehaviour
{
    public GameObject gameOverUI;
    public AudioSource music;
    public PauseMenu pauseMenu;
    public AudioSource SFXSource;
    public AudioClip clickSFX;
    public AudioClip loserSFX;

    public static bool GameIsPaused = false;

    public void GameOver()
    {
        gameOverUI.SetActive(true);
        Time.timeScale = 0f;
        GameIsPaused = true;

        if (pauseMenu != null)
        {
            pauseMenu.enabled = false;
            Debug.Log("Pause menu disabled");
        }

        if (music != null)
        {
            music.mute = true;
        }

        if (SFXSource != null && loserSFX != null)
        {
            SFXSource.PlayOneShot(loserSFX);
            Debug.Log("Played sound: " + loserSFX.name);
        }

    }
    public void Home()
    {
        PlaySound();
        Debug.Log("Loading Main Menu...");
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex - 1);
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
}
