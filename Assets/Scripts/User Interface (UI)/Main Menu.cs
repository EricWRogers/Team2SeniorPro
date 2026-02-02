using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void Play()
    {
        GameManager.Instance.newMap("Squirrel_HUB", true); //loads the main hub scene, resets collectibles so it doesnt add 0 to total
        Debug.Log("Play button pressed, loading game...");

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.UnmuteMusicDelayed();
        }
    }

    public void Options()
    {
        Debug.Log("Options menu opened");
    }

    public void Quit()
    {
        Application.Quit();
        Debug.Log("You've quit the game!");
    }
}
