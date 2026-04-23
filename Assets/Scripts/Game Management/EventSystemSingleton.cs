using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class EventSystemSingleton : MonoBehaviour
{
    private static EventSystemSingleton instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);

            // Clean immediately on boot
            CleanupDuplicates();
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        CleanupDuplicates();
    }

    private void CleanupDuplicates()
    {
        EventSystem[] systems = FindObjectsByType<EventSystem>(FindObjectsSortMode.None);

        foreach (var es in systems)
        {
            if (es.gameObject != gameObject)
            {
                Debug.Log("Destroying duplicate EventSystem: " + es.gameObject.name);
                Destroy(es.gameObject);
            }
        }
    }
}