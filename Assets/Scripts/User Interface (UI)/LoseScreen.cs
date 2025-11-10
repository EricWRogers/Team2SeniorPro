using UnityEngine;
using UnityEngine.SceneManagement;

public class LoseScreen : MonoBehaviour
{
    public GameObject gameOverUI;
    public GameObject Drank;
    public AudioSource music;
    public PauseMenu pauseMenu;
    public AudioSource SFXSource;
    public AudioClip clickSFX;
    public AudioClip loserSFX;
    public Timer timer;

    public static bool GameIsPaused = false;

    public void GameOver()
    {
        gameOverUI.SetActive(true);
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
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

        if (timer != null && timer.timeRemaining >= 300f)
        {
            if (Drank != null) Drank.SetActive(true);
        }

    }
    public void Home()
    {
        PlaySound();
        Debug.Log("Loading Main Menu...");
        Time.timeScale = 1f;
        GameManager.Instance.newMap("Main Menu", true); //loads the main menu, resets collectibles so it doesnt add 0 to total
    }

    public void Restart()
    {
        PlaySound();
        Time.timeScale = 1f;
        GameManager.Instance.newMap(GameManager.Instance.GetCurrentScene(), false); //reloads the current scene, does not reset collectibles so it adds to total
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
