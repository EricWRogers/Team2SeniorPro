using UnityEngine;
using UnityEngine.EventSystems;

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
        GameManager.Instance.newMap("IntroScene", true);
        Debug.Log("Play button pressed, loading game...");

        if (SoundManager.Instance != null)
            SoundManager.Instance.SetMusicMuted(true);
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