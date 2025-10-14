using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public string currentScene;
    public int currentCheckpoint = 0;
    public int collectibleCount = 0;
    public int totalCollectibles = 0;
    public int frameRate = 60;

    public void newMap(string _newMap, bool _resetCollectibles = false) //when loading a new map, it will add the current collectible count to the total. if true, it wont. use for cases of retry or quitting the level
    {
        currentCheckpoint = 0;
        if (_resetCollectibles)
        {
            totalCollectibles = totalCollectibles + collectibleCount;
        }
        collectibleCount = 0;
        SceneManager.LoadScene(_newMap);
        currentScene = SceneManager.GetActiveScene().name;
    }
    public void getTime(int _elapsedTime)
    {

    }
    public void addCollectible()
    {
        collectibleCount++;
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
            Application.targetFrameRate = frameRate; //cap the fps
        }
        else
        {
            //Application.targetFrameRate = -1;
        }
        if (currentScene == "" || currentScene == null)
        {
            currentScene = SceneManager.GetActiveScene().name;
        }
        
    }
    
}
