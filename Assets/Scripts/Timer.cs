using UnityEngine;
using TMPro;

public class Timer : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private float totalTime = 60f;

    public LoseScreen LS;

    private float timeRemaining;
    private bool timeRunning = true;

    private void Start()
    {
        timeRemaining = totalTime;
        UpdateTimerDisplay();
    }
    private void Update()
    {
        if (timeRunning)
        {
            timeRemaining -= Time.deltaTime;
            if (timeRemaining <= 0 )
            {
                timeRemaining = 0;
                timeRunning = false;
                OnTimerEnds();
            }
            UpdateTimerDisplay();
        }
    }
    private void UpdateTimerDisplay()
    {
        int minutes = Mathf.FloorToInt(timeRemaining / 60f);
        int seconds = Mathf.FloorToInt(timeRemaining % 60f);
        timerText.text = $"{minutes:0}:{seconds:00}";
    }

    public string GetFormattedTime()
    {
        int minutes = Mathf.FloorToInt(timeRemaining / 60f);
        int seconds = Mathf.FloorToInt(timeRemaining % 60f);
        return $"{minutes:0}:{seconds:00}";
    }

    public void StopTimer()
    {
        timeRunning = false;
    }
    private void OnTimerEnds()
    {
        Debug.Log("Timer ended");
        //whatever loser concequences we want to add later
        LS.GameOver();
    }
}
