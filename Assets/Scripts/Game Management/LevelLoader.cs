using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;
using UnityEngine.EventSystems; // Required for EventSystem cleanup

public class LevelLoader : MonoBehaviour
{
    public static LevelLoader Instance;
    public bool IsLoading => isLoading;

    [Header("Loading Screen")]
    [SerializeField] private GameObject loadingScreen;
    [SerializeField] private Slider progressBar;
    [SerializeField] private TMP_Text loadingText;
    [SerializeField] private TMP_Text percentText;

    [Header("Optional Video Background")]
    [SerializeField] private VideoPlayer videoPlayer;
    [SerializeField] private VideoClip[] backgroundVideos;

    [Header("Settings")]
    [SerializeField] private float minimumLoadingScreenTime = 1f;
    [SerializeField] private float progressSmoothSpeed = 5f;

    private float extraProgress = 0f;
    private float targetProgress = 0f;
    private float displayedProgress = 0f;
    private bool isLoading = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        // If we are currently in the process of a routine, isLoading should be true
        // Otherwise, default to fase.
        if (loadingScreen != null && loadingScreen.activeSelf)
        {
            isLoading = true;
        }
        else
        {
            isLoading = false;
        }
    }

    private void Update()
    {
        displayedProgress = Mathf.Lerp(displayedProgress, targetProgress, Time.deltaTime * progressSmoothSpeed);

        if (Mathf.Abs(displayedProgress - targetProgress) < 0.001f)
            displayedProgress = targetProgress;

        if (progressBar != null)
            progressBar.value = displayedProgress;

        if (percentText != null)
            percentText.text = Mathf.RoundToInt(displayedProgress * 100f) + "%";
    }

    public void ShowLoadingScreenImmediate()
    {
        if (loadingScreen != null)
        {
            loadingScreen.transform.SetAsLastSibling();
            loadingScreen.SetActive(true);
        }

        StartBackgroundVideo();
    }

    public void HideLoadingScreenImmediate()
    {
        StopBackgroundVideo();

        if (loadingScreen != null)
            loadingScreen.SetActive(false);
    }

    public void LoadLevel(string sceneName)
    {
        Debug.Log("LoadLevel called for: " + sceneName);
        Debug.Log("Loading screen reference is null? " + (loadingScreen == null));

        if (isLoading)
            return;

        StartCoroutine(LoadLevelRoutine(sceneName, null));
    }

    public IEnumerator LoadLevelRoutine(string sceneName, IEnumerator initializationSteps)
    {
        isLoading = true;

        if (loadingScreen != null)
        {
            loadingScreen.transform.SetAsLastSibling();
            loadingScreen.SetActive(true);
        }

        StartBackgroundVideo();

        extraProgress = 0f;
        targetProgress = 0f;
        displayedProgress = 0f;

        SetLoadingStep("Loading " + sceneName + "...");

        Scene currentScene = SceneManager.GetActiveScene();

        float timer = 0f;
        bool initDone = initializationSteps == null;

        AsyncOperation sceneLoad = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        sceneLoad.allowSceneActivation = false;

        if (initializationSteps != null)
        {
            StartCoroutine(RunInitialization(initializationSteps, () => initDone = true));
        }

        while (sceneLoad.progress < 0.9f || !initDone || timer < minimumLoadingScreenTime)
        {
            timer += Time.deltaTime;

            float sceneProgress = Mathf.Clamp01(sceneLoad.progress / 0.9f);
            float combinedProgress = Mathf.Clamp01((sceneProgress * 0.7f) + (extraProgress * 0.3f));

            UpdateUI(combinedProgress);
            yield return null;
        }

        SetLoadingStep("Activating " + sceneName + "...");
        UpdateUI(0.98f);

        sceneLoad.allowSceneActivation = true;

        while (!sceneLoad.isDone)
        {
            yield return null;
        }

        // --- NEW CLEANUP LOGIC STARTS HERE ---
        // As soon as the new scene is loaded, we look for duplicate EventSystems
        CleanDuplicateEventSystems();
        // --- NEW CLEANUP LOGIC ENDS HERE ---

        Scene newScene = SceneManager.GetSceneByName(sceneName);
        if (newScene.IsValid())
        {
            SceneManager.SetActiveScene(newScene);
        }

        if (currentScene.IsValid() && currentScene != newScene)
        {
            yield return SceneManager.UnloadSceneAsync(currentScene);
        }

        UpdateUI(1f);
        SetLoadingStep("Done");

        yield return new WaitForSeconds(0.15f);

        StopBackgroundVideo();

        if (loadingScreen != null)
            loadingScreen.SetActive(false);

        isLoading = false;
    }

    // New helper method to ensure cutscenes and UI aren't blocked by duplicate systems
    private void CleanDuplicateEventSystems()
    {
        EventSystem[] systems = Object.FindObjectsByType<EventSystem>(FindObjectsSortMode.None);
        if (systems.Length > 1)
        {
            // We keep the first one found (usually the one from the persistent root)
            // and destroy the others found in the newly loaded level.
            for (int i = 1; i < systems.Length; i++)
            {
                Debug.Log($"LevelLoader: Destroyed duplicate EventSystem in new scene: {systems[i].gameObject.scene.name}");
                Destroy(systems[i].gameObject);
            }
        }
    }

    private void StartBackgroundVideo()
    {
        if (videoPlayer == null)
            return;

        if (backgroundVideos != null && backgroundVideos.Length > 0)
        {
            int randomIndex = Random.Range(0, backgroundVideos.Length);
            videoPlayer.clip = backgroundVideos[randomIndex];
        }

        videoPlayer.time = 0;
        videoPlayer.isLooping = true;
        videoPlayer.Play();
    }

    private void StopBackgroundVideo()
    {
        if (videoPlayer != null)
        {
            videoPlayer.Stop();
        }
    }

    private IEnumerator RunInitialization(IEnumerator routine, System.Action onComplete)
    {
        yield return StartCoroutine(routine);
        onComplete?.Invoke();
    }

    public void SetLoadingStep(string stepText)
    {
        if (loadingText != null)
            loadingText.text = stepText;
    }

    public void SetExtraProgress(float value)
    {
        extraProgress = Mathf.Clamp01(value);
    }

    private void UpdateUI(float progress)
    {
        targetProgress = progress;
    }
}