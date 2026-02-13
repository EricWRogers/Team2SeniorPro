using UnityEngine;
using TMPro;

public class Timer : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI timerText;

    public LoseScreen LS;

    public float timeRemaining;
    public bool timeRunning = true;

    private const string SaveTimeKey = "SavedTimerTime";
    private const string SaveCheckpointKey = "SavedCheckpointId";

    private int lastSavedCheckpointId = -1;

    private void Start()
    {
        LoadTimer();
    }

    private void Update()
    {
        if (!timeRunning) return;

        timeRemaining += Time.deltaTime;

        if (timeRemaining >= 300f)
        {
            timeRunning = false;
            OnTimerEnds();
        }

        UpdateTimerDisplay();
    }

    private void UpdateTimerDisplay()
    {
        if (!timerText) return;

        int minutes = Mathf.FloorToInt(timeRemaining / 60f);
        int seconds = Mathf.FloorToInt(timeRemaining % 60f);
        timerText.text = $"{minutes:0}:{seconds:00}";
    }

    private void OnTimerEnds()
    {
        Debug.Log("Timer ended");
        if (LS) LS.GameOver();
    }

    // Save time ONLY if this is a different checkpoint than last time
    public void SaveTimerForCheckpoint(int checkpointId)
    {
        if (checkpointId == lastSavedCheckpointId)
            return; // same checkpoint, ignore

        lastSavedCheckpointId = checkpointId;

        PlayerPrefs.SetFloat(SaveTimeKey, timeRemaining);
        PlayerPrefs.SetInt(SaveCheckpointKey, checkpointId);
        PlayerPrefs.Save();

        Debug.Log($"Timer saved at checkpoint {checkpointId}: {timeRemaining}");
    }

    public void LoadTimer()
    {
        timeRemaining = PlayerPrefs.GetFloat(SaveTimeKey, 0f);
        lastSavedCheckpointId = PlayerPrefs.GetInt(SaveCheckpointKey, -1);

        Debug.Log($"Timer loaded: {timeRemaining}, last checkpoint: {lastSavedCheckpointId}");
        UpdateTimerDisplay();
    }

    public void ResetTimerAndSaveData()
    {
        PlayerPrefs.DeleteKey(SaveTimeKey);
        PlayerPrefs.DeleteKey(SaveCheckpointKey);
        PlayerPrefs.Save();

        timeRemaining = 0f;
        timeRunning = true;
        lastSavedCheckpointId = -1;

        UpdateTimerDisplay();
        Debug.Log("Timer and checkpoint save data reset.");
    }

    public void SetTime(float newTime)
    {
        timeRemaining = newTime;
        // make sure UI updates immediately
        // (if UpdateTimerDisplay is private, keep this method inside Timer so it can call it)
        UpdateTimerDisplay();
    }

    public void StopTimer()
    {
        timeRunning = false;
    }

    public float GetElapsedTime()
    {
        return timeRemaining;
    }

    public string GetFormattedTime()
    {
        int minutes = Mathf.FloorToInt(timeRemaining / 60f);
        int seconds = Mathf.FloorToInt(timeRemaining % 60f);
        return $"{minutes:0}:{seconds:00}";
    }

}
