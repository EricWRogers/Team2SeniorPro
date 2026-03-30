using UnityEngine;
using TMPro;
using SuperPupSystems.Helper;
using UnityEngine.SceneManagement;

public class KillVolume : MonoBehaviour
{
    // public Transform playerRespawn;  // usually same as BottomRespawn
    public GameObject deathScreen;
    public TMP_Text deathCountdown; 
    public GameManager GM;

    private float countdownTime = 3f; // time in seconds before respawn after death
    private bool isDead = false;

    [Header("Events")]
    [Tooltip("Scripts to disable when paused and enable when resumed")]
    public MonoBehaviour[] scriptsToToggle;
    public GameObject[] objectsToToggle;

    void Start()
    {
        if (GM == null)
        {
            GM = FindFirstObjectByType<GameManager>();
            if (GM == null)
            {
                Debug.LogError("No GameManager found in scene!");
            }
        }

        if (deathScreen == null)
        {
            // This finds ALL objects of type Canvas and searches inside them,
            // even if they are disabled.
            Canvas[] allCanvases = Resources.FindObjectsOfTypeAll<Canvas>();
            foreach (Canvas c in allCanvases)
            {
                // Search for a child named exactly "Death Screen"
                Transform t = c.transform.Find("Death Screen");
                if (t != null)
                {
                    deathScreen = t.gameObject;
                    break;
                }
            }
        }

        if (deathCountdown == null)
        {
            // Since the death screen is found, we can look inside for the text
            deathCountdown = deathScreen.GetComponentInChildren<TMP_Text>(true);
        }
    }

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
            deathCountdown.text = Mathf.Ceil(timeLeft).ToString();
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
