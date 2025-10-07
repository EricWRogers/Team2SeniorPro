using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class NestGoal : MonoBehaviour
{
    public GameObject WinScreen;

    [Header("Text Elements")]
    public TMP_Text timerText;
    public TMP_Text statsText;

    [Header("Objects To Deactivate")]
    public MonoBehaviour[] objectsToDeactivate;

    public void ReturnToMain()
    {
        SceneManager.LoadScene("Kaden's Scene");
        Cursor.visible = true;
        Time.timeScale = 1f; // Resume the game
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
        if (objectsToDeactivate != null)
        {
            foreach (var obj in objectsToDeactivate)
            {
                if (obj != null)
                    obj.enabled = false;
            }
        }

        // Stop the timer
        //timer.GetComponent<Timer>().enabled = false;

        // Update TMPro text with final time
        /*if (timerText != null && timer != null)
        {
            var timerScript = timer.GetComponent<Timer>();
            if (timerScript != null)
            {
                timerText.text = $"Time: {timerScript.GetCurrentTimeString()}";
            }
        }*/

        // Update stats text
        /*if (statsText != null)
        {
            var acornCount = FindObjectOfType<AcornCounter>()?.GetAcornCount() ?? 0;
            statsText.text = $"Acorns Collected: {acornCount}";
        }*/
    }
}
