using UnityEngine;
using TMPro;
using UnityEngine.UI; // Add this for UI components

public class NestGoal : MonoBehaviour
{
    
    public GameObject MainCam;
    public GameObject PlayerSquirrel;
    public GameObject VictorySquirrel;
    public Timer timer;
    public AudioSource SFXSource;
    //public CollectibleScript collectibles;

    [Header("Text Elements")]
    public TMP_Text timerText;
    public TMP_Text statsText;

    [Header("Events")]
    [Tooltip("Scripts to disable when paused and enable when resumed")]
    public MonoBehaviour[] scriptsToToggle;

    [Header("UI Elements")]
    public GameObject WinScreen;
    public GameObject BerryUI;
    public GameObject TimerUI;

    [Header("Rank Displays")]
    public GameObject Srank;
    public GameObject Arank;
    public GameObject Brank;
    public GameObject Crank;

    string achievedRank = "D";

    void Awake()
    {
        if (MainCam == null)
        MainCam = GameObject.FindWithTag("MainCamera");

        if (PlayerSquirrel == null)
            PlayerSquirrel = GameObject.FindWithTag("Player");

        if (VictorySquirrel == null)
            VictorySquirrel = GameObject.FindWithTag("DancingPlayer");
        
        if (timer == null)
            timer = GameObject.FindWithTag("Canvas").GetComponent<Timer>();

        if (SFXSource == null)
            SFXSource = GameObject.FindWithTag("Canvas").GetComponent<AudioSource>();
    }

    public void ReturnToMain()
    {
        GameManager.Instance.newMap("Squirrel_HUB", true); //loads the burrow, resets collectibles so it doesnt add 0 to total
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Time.timeScale = 1f; // Resume the game

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.SetMusicMuted(true);
        }
    }

    public void Restart()
    {
        Time.timeScale = 1f;
        GameManager.Instance.newMap(GameManager.Instance.GetCurrentScene(), false); // you beat the level, so dont reset collectibles gotten...
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        Debug.Log("WIN! Acorn delivered to the nest.");

        // Scene Transition and UI updates
        PlayerSquirrel.SetActive(false);
        MainCam.SetActive(false);
        VictorySquirrel.SetActive(true);

        SFXSource.Play();

        WinScreen.SetActive(true);
        BerryUI.SetActive(false);
        TimerUI.SetActive(false);

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        Time.timeScale = 0f; // Pause the game

        // Deactivate specified objects
        if (scriptsToToggle != null)
        {
            foreach (var script in scriptsToToggle)
            {
                if (script != null)
                    script.enabled = false;
            }
        }

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.SetMusicMuted(true);
        }

        // Stop the timer
        timer.StopTimer();

        float finalTime = timer.GetElapsedTime();

        // Reset rank visuals
        Srank?.SetActive(false);
        Arank?.SetActive(false);
        Brank?.SetActive(false);
        Crank?.SetActive(false);

        // Rank calculation (clean else-if chain)
        if (finalTime <= 60f)
        {
            achievedRank = "S";
            Srank?.SetActive(true);
        }
        else if (finalTime > 60f && finalTime <= 120f)
        {
            achievedRank = "A";
            Arank?.SetActive(true);
        }
        else if (finalTime > 120f && finalTime <= 180f)
        {
            achievedRank = "B";
            Brank?.SetActive(true);
        }
        else if (finalTime > 180f && finalTime <= 240f)
        {
            achievedRank = "C";
            Crank?.SetActive(true);
        }
        else
        {
            achievedRank = "D";
        }

        // Save best stats
        string levelName = GameManager.Instance.GetCurrentScene();
        int berries = GameManager.Instance.collectibleCount;

        DataManager.Instance.SaveBestTime(levelName, finalTime);
        DataManager.Instance.SaveBestScore(levelName, berries);
        DataManager.Instance.SaveBestRank(levelName, achievedRank);

        // Update TMPro text with final time
        if (timerText != null && timer != null)
        {
            var timerScript = timer.GetComponent<Timer>();
            if (timerScript != null)
            {
                timerText.text = $"Time - {timer.GetFormattedTime()}" ;
            }
        }
    
        // Display total collectibles in statsText
        if (statsText != null)
        {
            statsText.text = $"Berries - {GameManager.Instance.collectibleCount}";
        }
    }
}
