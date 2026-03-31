using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;

public class LevelLoader : MonoBehaviour
{
    public static LevelLoader Instance;

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
        if (loadingScreen != null)
            loadingScreen.SetActive(false);
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