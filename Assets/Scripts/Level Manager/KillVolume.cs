using UnityEngine;
using TMPro;
using SuperPupSystems.Helper;
using UnityEngine.SceneManagement;

public class KillVolume : MonoBehaviour
{
    // public Transform playerRespawn;  // usually same as BottomRespawn
    public GameObject deathScreen;
    public TMP_Text countdownText; 
    public GameManager GM;

    private float countdownTime = 3f; // time in seconds before respawn after death
    private bool isDead = false;

    [Header("Events")]
    [Tooltip("Scripts to disable when paused and enable when resumed")]
    public MonoBehaviour[] scriptsToToggle;
    public GameObject[] objectsToToggle;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isDead)
        {
            isDead = true;

            Toggle(false);
            deathScreen.SetActive(true);

            Time.timeScale = 0f; // pause the game
            StartCoroutine(DeathCountdown());
        }
        /*if (other.CompareTag("Acorn"))
        {
            other.GetComponent<CarryableAcorn>()?.RespawnToPoint();
        }
        else if (other.CompareTag("Player"))
        {
            var rb = other.attachedRigidbody;
            if (rb) { rb.linearVelocity = Vector3.zero; }
            other.transform.position = playerRespawn.position + Vector3.up * 0.5f;
        }*/
    }

    System.Collections.IEnumerator DeathCountdown()
    {
        float timeLeft = countdownTime;

        while (timeLeft > 0)
        {
            countdownText.text = Mathf.Ceil(timeLeft).ToString();
            yield return new WaitForSecondsRealtime(1f);
            timeLeft--;
        }

        ReloadScene();
    }

    void ReloadScene()
    {
        Time.timeScale = 1f; // resume the game
        SceneManager.LoadScene(GM.GetCurrentScene());

    }

    private void Toggle(bool enable)
    {
        if (scriptsToToggle == null) return;

        if (objectsToToggle == null) return;

         foreach (var obj in objectsToToggle)
        {
            if (obj != null)
                obj.SetActive(enable);
        }

        foreach (var script in scriptsToToggle)
        {
            if (script != null)
                script.enabled = enable;
        }
    }
}
