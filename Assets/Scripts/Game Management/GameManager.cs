using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }    
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
    }
    public string GetCurrentScene()
    {
        return SceneManager.GetActiveScene().name;
    }
    public void setCheckpoint(int _checkpointNumber, bool _force = false)
    {
        if (!_force && _checkpointNumber > currentCheckpoint) //only set the checkpoint if its a higher number than the current one
        {
            currentCheckpoint = _checkpointNumber;
        }
        else if (_force) //if force is true, set it no matter what
        {
            currentCheckpoint = _checkpointNumber;
        }
    }
    public void respawnAtCheckpoint(int _checkpointNumber = -1) //if nothing is input, it will use the variable currentCheckpoint by default, so dont worry about calling without a number
    {
        if (_checkpointNumber >= 0)
        {
            SceneManager.LoadScene(GetCurrentScene());
        }
        else
        {
            setCheckpoint(_checkpointNumber);
            SceneManager.LoadScene(GetCurrentScene());
        }
        
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
        
    }
    
}
