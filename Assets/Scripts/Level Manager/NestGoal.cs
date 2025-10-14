using UnityEngine;
using TMPro;
using UnityEngine.UI; // Add this for UI components

public class NestGoal : MonoBehaviour
{
    public GameObject WinScreen;
    public Timer timer;
    //public CollectibleScript collectibles;

    [Header("Text Elements")]
    public TMP_Text timerText;
    public TMP_Text statsText;

    [Header("Events")]
    [Tooltip("Scripts to disable when paused and enable when resumed")]
    public MonoBehaviour[] scriptsToToggle;

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
        GameManager.Instance.newMap(GameManager.Instance.currentScene, false); //reloads the current scene, does not reset collectibles so it adds to total
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Acorn")) return;
        Debug.Log("WIN! Acorn delivered to the nest.");
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
    
        /*
        // Update stats text
        if (winScoreText != null)
        {
            var collectCount = collectibles.currentScore;
            winScoreText.text = $"Items Collected: {collectCount}";
        }*/
    }
}
