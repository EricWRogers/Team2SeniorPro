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

    public HashSet<string> collectedBerryIDs = new HashSet<string>(); // To track collected berries by their unique IDs

    public int PermaBerryCount => collectedBerryIDs.Count; // Property to get the total count of unique berries collected

    // Method called whenever a berry is collected, passing its unique ID
    public void CollectBerry(string id)
    {
        // Check if we already have the specific berry's ID
        if (!collectedBerryIDs.Contains(id))
        {
            // Add it to the list
            collectedBerryIDs.Add(id); // Mark berry as collected

            // Track the count of berries 'per level' for the UI
            totalCollectibles = collectedBerryIDs.Count; // Update total collectibles count

            // SAVE IMMEDIATELY: Save the updated berry data to PlayerPrefs
            Debug.Log($"Collected New ID: {id}. Total: {totalCollectibles}");
            SaveBerryData();
        }
        else
        {
            Debug.LogWarning($"Duplicate Berry ID detected: {id}! Berry {id} was already collected.");
        }
    }

    // Method to check if a berry should exist
    public bool IsBerryCollected(string id)
    {
        return collectedBerryIDs.Contains(id); // Check if berry has already been collected
    }

    public void SaveBerryData()
    {
        // Convert the HashSet to an array, then to a single string separated by commas
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

            collectedBerryIDs.Clear(); // Clear current data before loading
            foreach (string id in splitIDs)
            {
                if (!string.IsNullOrEmpty(id))
                {
                    collectedBerryIDs.Add(id); // Add each ID back to the HashSet
                }
            }
            Debug.Log("Game Loaded: " + collectedBerryIDs.Count + " berries found.");
        }
    }

    // Testing method to clear saved data (not called in normal gameplay)
    [ContextMenu("Reset All Progress")] // This allows you to right-click the script in the Inspector to reset
    public void ResetBerryData()
    {
        PlayerPrefs.DeleteAll(); // Clear all saved data
        collectedBerryIDs.Clear(); // Clear the in-memory list as well
        Debug.Log("All progress reset. Berry data cleared.");
    }

    public void newMap(string _newMap, bool _resetCollectibles = false, bool _resetCheckpoint = true) //when loading a new map, it will add the current collectible count to the total. if true, it wont. use for cases of retry or quitting the level
    {
        currentCheckpoint = _resetCheckpoint ? currentCheckpoint = 0 : currentCheckpoint = 1; //reset checkpoint to 0 unless told not to

        if (_resetCollectibles)
        {
            totalCollectibles = totalCollectibles + collectibleCount;
        }
        collectibleCount = 0;
        
        SceneManager.LoadScene(_newMap);
        //LevelLoader.Instance.LoadLevel(_newMap);
    }

    public void SetCheckpoint(int _checkpointNumber, bool _force = false)
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
    public string GetCurrentScene()
    {
        return SceneManager.GetActiveScene().name;
    }
    public void respawnAtCheckpoint(int _checkpointNumber = -1) //if nothing is input, it will use the variable currentCheckpoint by default, so dont worry about calling without a number
    {
        if (_checkpointNumber >= 0)
        {
            SceneManager.LoadScene(GetCurrentScene());
        }
        else
        {
            SetCheckpoint(_checkpointNumber);
            SceneManager.LoadScene(GetCurrentScene());
        }
        
    }
    public float GetTime()
    {
        return 123.45f; //placeholder
    }

    public void Start()
    {
        //ResetBerryData(); // Clear saved data for testing purposes (remove this line in production)
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
        if (Instance == this)
        {
            LoadBerryData(); // Load berry data when the GameManager is initialized
        }
/*        #if UNITY_EDITOR
            if (SceneManager.GetActiveScene().name != "LoadingScene")
                {SceneManager.LoadSceneAsync("LoadingScene", LoadSceneMode.Additive);}
        #endif
  */      
    }
    
}
