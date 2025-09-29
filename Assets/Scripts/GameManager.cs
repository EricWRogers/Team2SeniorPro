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
    
}
