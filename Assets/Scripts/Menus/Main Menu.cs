using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void Play()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
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
