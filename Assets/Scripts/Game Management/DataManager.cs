using UnityEngine;

public class DataManager : MonoBehaviour
{
    public static DataManager Instance { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this)
            Destroy(gameObject);
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    // ---------- BEST TIME ----------
    public void SaveBestTime(string levelName, float time)
    {
        float bestTime = PlayerPrefs.GetFloat(levelName + "_BestTime", float.MaxValue);

        if (time < bestTime)
        {
            PlayerPrefs.SetFloat(levelName + "_BestTime", time);
            PlayerPrefs.Save();
        }
    }

    public float GetBestTime(string levelName)
    {
        return PlayerPrefs.GetFloat(levelName + "_BestTime", -1f);
    }

    // ---------- BEST SCORE ----------
    public void SaveBestScore(string levelName, int score)
    {
        int bestScore = PlayerPrefs.GetInt(levelName + "_BestScore", 0);

        if (score > bestScore)
        {
            PlayerPrefs.SetInt(levelName + "_BestScore", score);
            PlayerPrefs.Save();
        }
    }

    public int GetBestScore(string levelName)
    {
        return PlayerPrefs.GetInt(levelName + "_BestScore", 0);
    }

    // ---------- BEST RANK ----------
    public void SaveBestRank(string levelName, string newRank)
    {
        string savedRank = PlayerPrefs.GetString(levelName + "_BestRank", "");

        if (IsBetterRank(newRank, savedRank))
        {
            PlayerPrefs.SetString(levelName + "_BestRank", newRank);
            PlayerPrefs.Save();
            Debug.Log("New Best Rank: " + newRank);
        }
    }

    public string GetBestRank(string levelName)
    {
        return PlayerPrefs.GetString(levelName + "_BestRank", "");
    }

    bool IsBetterRank(string newRank, string savedRank)
    {
        if (string.IsNullOrEmpty(savedRank)) return true;

        return RankValue(newRank) > RankValue(savedRank);
    }

    int RankValue(string rank)
    {
        switch (rank)
        {
            case "S": return 5;
            case "A": return 4;
            case "B": return 3;
            case "C": return 2;
            case "D": return 1;
            default: return 0;
        }
    }

    // ---------- FORMAT TIME ----------
    public string FormatTime(float time)
    {
        if (time < 0) return "--:--";

        int minutes = Mathf.FloorToInt(time / 60);
        int seconds = Mathf.FloorToInt(time % 60);

        return string.Format("{0:00}:{1:00}", minutes, seconds);
    }
}