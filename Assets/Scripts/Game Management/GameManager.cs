using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Linq;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public int currentCheckpoint = 0;
    public int collectibleCount = 0;
    public int totalCollectibles = 0;
    public int frameRate = 60;
    public SerializationManager serializationManager;

    public HashSet<string> collectedBerryIDs = new HashSet<string>();

    public int PermaBerryCount => collectedBerryIDs.Count;

    public void CollectBerry(string id)
    {
        if (!collectedBerryIDs.Contains(id))
        {
            collectedBerryIDs.Add(id);
            totalCollectibles = collectedBerryIDs.Count;

            Debug.Log($"Collected New ID: {id}. Total: {totalCollectibles}");
            SaveBerryData();
        }
        else
        {
            Debug.LogWarning($"Duplicate Berry ID detected: {id}! Berry {id} was already collected.");
        }
    }

    public bool IsBerryCollected(string id)
    {
        return collectedBerryIDs.Contains(id);
    }

    public void SaveBerryData()
    {
        string berryData = string.Join(",", collectedBerryIDs.ToArray());
        PlayerPrefs.SetString("SavedBerries", berryData);
        PlayerPrefs.Save();
        Debug.Log("Game Saved: " + berryData);
    }

    public void LoadBerryData()
    {
        if (PlayerPrefs.HasKey("SavedBerries"))
        {
            string berryData = PlayerPrefs.GetString("SavedBerries");
            string[] splitIDs = berryData.Split(",");

            collectedBerryIDs.Clear();
            foreach (string id in splitIDs)
            {
                if (!string.IsNullOrEmpty(id))
                {
                    collectedBerryIDs.Add(id);
                }
            }

            Debug.Log("Game Loaded: " + collectedBerryIDs.Count + " berries found.");
        }
    }

    [ContextMenu("Reset All Progress")]
    public void ResetBerryData()
    {
        PlayerPrefs.DeleteAll();
        collectedBerryIDs.Clear();
        Debug.Log("All progress reset. Berry data cleared.");
    }

    public void newMap(string _newMap, bool _resetCollectibles = false, bool _resetCheckpoint = true)
    {
        if (_resetCheckpoint)
        {
            currentCheckpoint = 0;
        }

        if (_resetCollectibles)
        {
            totalCollectibles += collectibleCount;
        }

        collectibleCount = 0;

        bool isRestartingCurrentScene = GetCurrentScene() == _newMap;
        bool skipLoadingScreenForRestart =
            isRestartingCurrentScene &&
            (_newMap == "Level_1" || _newMap == "Level_2" || _newMap == "Level_3" || _newMap == "Level_4");

        if (LevelLoader.Instance != null)
        {
            LevelLoader.Instance.LoadLevel(_newMap, !skipLoadingScreenForRestart);
        }
        else
        {
            Debug.LogWarning($"LevelLoader not found. Falling back to direct load for scene: {_newMap}");
            SceneManager.LoadScene(_newMap);
        }
    }

    
    public void SetCheckpoint(int _checkpointNumber, bool _force = false)
    {
        if (!_force && _checkpointNumber > currentCheckpoint)
        {
            currentCheckpoint = _checkpointNumber;
        }
        else if (_force)
        {
            currentCheckpoint = _checkpointNumber;
        }
    }

    public string GetCurrentScene()
    {
        return SceneManager.GetActiveScene().name;
    }

    public void respawnAtCheckpoint(int _checkpointNumber = -1)
    {
        if (_checkpointNumber >= 0)
        {
            SetCheckpoint(_checkpointNumber, true);
        }

        string currentScene = GetCurrentScene();

        if (LevelLoader.Instance != null)
        {
            LevelLoader.Instance.LoadLevel(currentScene);
        }
        else
        {
            Debug.LogWarning($"LevelLoader not found. Falling back to direct reload for scene: {currentScene}");
            SceneManager.LoadScene(currentScene);
        }
    }

    public float GetTime()
    {
        return 123.45f;
    }

    public void Start()
    {
        // ResetBerryData();
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
            return;
        }

        if (QualitySettings.vSyncCount == 0)
        {
            Application.targetFrameRate = frameRate;
        }

        LoadBerryData();
    }
}