using UnityEngine;
using System;
using System.Collections.Generic;


public class SerializationManager : MonoBehaviour
{
    [Serializable]
    public class PlayerData
    {
        public List<int> levelCollectables; //should be based on level index
        public float overallTimeElapsed;
        public List<float> levelTimes; //based on level index (0-3 for 4 levels)
    }
    public void SaveToPlayerPrefs()
    {
        // Convert this GameState instance to a JSON string
        string json = JsonUtility.ToJson(this);

        // Save the converted JSON into the PlayerPrefs
        PlayerPrefs.SetString("PlayerData", json);
        PlayerPrefs.Save();
    }
    public static PlayerData CreateFromPlayerPrefs()
    {
        if (!PlayerPrefs.HasKey("PlayerData")){
            return null;}

        string json = PlayerPrefs.GetString("PlayerData");

        return JsonUtility.FromJson<PlayerData>(json);
    }
}
