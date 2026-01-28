using UnityEngine;
using TMPro;
using UnityEngine.UI; // Add this for UI components

public class NestGoal : MonoBehaviour
{
    public GameObject WinScreen;
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

    [Header("Rank Displays")]
    public GameObject Srank;
    public GameObject Arank;
    public GameObject Brank;
    public GameObject Crank;

    public void ReturnToMain()
    {
        GameManager.Instance.newMap("Main Menu", true); //loads the main menu, resets collectibles so it doesnt add 0 to total
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Time.timeScale = 1f; // Resume the game
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
        PlayerSquirrel.SetActive(false);
        MainCam.SetActive(false);
        VictorySquirrel.SetActive(true);
        SFXSource.Play();
        WinScreen.SetActive(true);
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

        // Activates rank based on time
        if (timer != null && timer.timeRemaining < 60f)
        {
            if (Srank != null) Srank.SetActive(true);
        }
        if (timer != null && timer.timeRemaining > 60f && timer.timeRemaining <= 120f)
        {
            if (Arank != null) Arank.SetActive(true);
        }
        if (timer != null && timer.timeRemaining > 120f && timer.timeRemaining <= 180f)
        {
            if (Brank != null) Brank.SetActive(true);
        }
        if (timer != null && timer.timeRemaining > 180f && timer.timeRemaining <= 240f)
        {
            if (Crank != null) Crank.SetActive(true);
        }

        // Stop the timer
        timer.StopTimer();

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
