using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public string currentScene;
    public int currentCheckpoint = 0;

    public void newMap(int _newMap)
    {
        currentCheckpoint = 0;
        SceneManager.LoadScene(_newMap);
    }
    public void getTime(int _elapsedTime)
    {

    }
    public void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
        if (QualitySettings.vSyncCount == 0)
        {
            Application.targetFrameRate = 60; //cap the fps
        }
        else
        {
            //Application.targetFrameRate = -1;
        }
        
    }
    
}
