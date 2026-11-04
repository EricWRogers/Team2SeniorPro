using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement; // Added to allow direct loading if GM is missing

public class MainMenu : MonoBehaviour
{
    [Header("Controller/UI")]
    public GameObject firstSelectedObject;

    private void OnEnable()
    {
        if (EventSystem.current != null && firstSelectedObject != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(firstSelectedObject);
        }
    }

    public void Play()
    {
        Debug.Log("Play button pressed, loading game...");

        if (SoundManager.Instance != null)
            SoundManager.Instance.SetMusicMuted(true);

        // BULLETPROOF CHECK: If the GameManager exists, use it. If not, bypass it entirely!
        if (GameManager.Instance != null)
        {
            GameManager.Instance.newMap("IntroScene", true);
        }
        else
        {
            Debug.LogWarning("GameManager not found in Main Menu. Falling back to direct scene load.");
            SceneManager.LoadScene("IntroScene");
        }
    }

    public void Options(GameObject optionsMenu)
    {
        optionsMenu.SetActive(true);

        UISelectOnEnable selector = optionsMenu.GetComponent<UISelectOnEnable>();
        if (selector != null)
            selector.Reselect();

        Debug.Log("Options menu opened");
    }

    public void Quit()
    {
        Application.Quit();
        Debug.Log("You've quit the game!");
    }
}
