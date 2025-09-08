using UnityEngine;
using UnityEngine.SceneManagement;

public class LoseScreen : MonoBehaviour
{
    public GameObject gameOverUI;
    public AudioSource music;
    public PauseMenu pauseMenu;
    public GameRestartOnDeplete gameRestart;
    public SurfacePainter surfacePainter;
    public SurfacePainterMulti surfacePainterM;
    
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

        if (gameRestart != null)
        {
            gameRestart.enabled = false;
            Debug.Log("Auto Reload disabled");
        }

        if (surfacePainter && surfacePainterM != null)
        {
            surfacePainter.enabled = false;
            surfacePainterM.enabled = false;
            Debug.Log("Painting disabled.");
        }

        if (music != null)
        {
            music.mute = true;
        }

    }
    public void Home()
    {
        Debug.Log("Loading Main Menu...");
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex - 1);
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

    public void Stats()
    {
        Debug.Log("Loading Stats...");
        SceneManager.LoadScene("Stats Scene");
    }
}
