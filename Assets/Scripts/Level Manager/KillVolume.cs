using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

public class KillVolume : MonoBehaviour
{
    public GameObject deathScreen;
    public TMP_Text deathCountdown;
    public GameManager GM;

    [Header("Audio")]
    public AudioSource SFXSource;
    public AudioClip deathSFX;
    public AudioClip deathFaceSFX;

    [SerializeField] private float countdownTime = 3f;

    private bool isDead = false;
    private bool isReloading = false;

    [Header("Death Face Animation")]
    public GameObject deathFaceUI;
    public Animator deathFaceAnimator;
    public float delayBeforeDeathFace = 0.5f;
    public float deathFaceDuration = 1.5f;

    [Header("Events")]
    [Tooltip("Scripts to disable when paused and enable when resumed")]
    public MonoBehaviour[] scriptsToToggle;
    public GameObject[] objectsToToggle;

    private void Start()
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
            Canvas[] allCanvases = Resources.FindObjectsOfTypeAll<Canvas>();
            foreach (Canvas c in allCanvases)
            {
                Transform t = c.transform.Find("Death Screen");
                if (t != null)
                {
                    deathScreen = t.gameObject;
                    break;
                }
            }
        }

        if (deathCountdown == null && deathScreen != null)
        {
            deathCountdown = deathScreen.GetComponentInChildren<TMP_Text>(true);
        }

        if (SFXSource == null) SFXSource = GetComponent<AudioSource>();
        if (deathFaceUI != null) deathFaceUI.SetActive(false);
        if (deathScreen != null) deathScreen.SetActive(false);

        isDead = false;
        isReloading = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (isDead || isReloading) return;
        if (LevelLoader.Instance != null && LevelLoader.Instance.IsLoading) return;

        isDead = true;
        isReloading = true;

        Time.timeScale = 0f;
        Toggle(false);

        StartCoroutine(DeathSequence());
    }

    private IEnumerator DeathSequence()
    {
        if (SFXSource != null && deathSFX != null)
        {
            SFXSource.PlayOneShot(deathSFX);
        }
        yield return new WaitForSecondsRealtime(delayBeforeDeathFace);

        if (deathFaceUI != null)
        {
            deathFaceUI.SetActive(true);

            if (SFXSource != null && deathFaceSFX != null)
            {
                SFXSource.PlayOneShot(deathFaceSFX);
            }
            
            if (deathFaceAnimator != null)
            {
                deathFaceAnimator.SetTrigger("DeathFace");
            }
        }

        yield return new WaitForSecondsRealtime(deathFaceDuration);

        if (deathFaceUI != null) deathFaceUI.SetActive(false);
        if (deathScreen != null) deathScreen.SetActive(true);

        float timeLeft = countdownTime;
        while (timeLeft > 0f)
        {
            if(deathCountdown != null)
                deathCountdown.text = Mathf.Ceil(timeLeft).ToString();

            yield return new WaitForSecondsRealtime(1f);
            timeLeft -= 1f;
        }

        ReloadScene();
    }

    private void ReloadScene()
    {
        Time.timeScale = 1f;

        if (deathScreen != null)
            deathScreen.SetActive(false);

        if (GameManager.Instance != null)
        {
            GameManager.Instance.newMap(GameManager.Instance.GetCurrentScene(), false);
        }
        else
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }

    private void Toggle(bool enable)
    {
        if (objectsToToggle != null)
        {
            foreach (var obj in objectsToToggle)
            {
                if (obj != null)
                    obj.SetActive(enable);
            }
        }

        if (scriptsToToggle != null)
        {
            foreach (var script in scriptsToToggle)
            {
                if (script != null)
                    script.enabled = enable;
            }
        }
    }
}