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

    [Header("Continue Control")]
    [SerializeField] private GameObject readyPanel;
    [SerializeField] private Button nextButton;

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
    private bool continueRequested = false;

    public bool IsLoading => isLoading;

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

        if (nextButton != null)
        {
            nextButton.onClick.RemoveAllListeners();
            nextButton.onClick.AddListener(OnNextPressed);
        }
    }

    private void Start()
    {
        if (loadingScreen != null)
            loadingScreen.SetActive(false);

        if (readyPanel != null)
            readyPanel.SetActive(false);
    }

    private void Update()
    {
        displayedProgress = Mathf.Lerp(
            displayedProgress,
            targetProgress,
            Time.unscaledDeltaTime * progressSmoothSpeed
        );

        if (Mathf.Abs(displayedProgress - targetProgress) < 0.001f)
            displayedProgress = targetProgress;

        if (progressBar != null)
            progressBar.value = displayedProgress;

        if (percentText != null)
            percentText.text = Mathf.RoundToInt(displayedProgress * 100f) + "%";
    }

    public void LoadLevel(string sceneName)
    {
        LoadLevel(sceneName, true);
    }

    public void LoadLevel(string sceneName, bool useLoadingScreen)
    {
        if (isLoading)
            return;

        StartCoroutine(LoadLevelRoutine(sceneName, null, useLoadingScreen));
    }

    public IEnumerator LoadLevelRoutine(string sceneName, IEnumerator initializationSteps, bool useLoadingScreen = true)
    {
        isLoading = true;
        continueRequested = false;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        ForceCloseAllPauseMenus();

        string currentSceneName = SceneManager.GetActiveScene().name;
        bool reloadingSameScene = currentSceneName == sceneName;

        // Same-scene restart must use SINGLE or it will stack duplicates
        LoadSceneMode mode = reloadingSameScene ? LoadSceneMode.Single : LoadSceneMode.Additive;

        if (useLoadingScreen)
        {
            Time.timeScale = 0f;

            if (loadingScreen != null)
            {
                loadingScreen.transform.SetAsLastSibling();
                loadingScreen.SetActive(true);
            }

            if (readyPanel != null)
                readyPanel.SetActive(false);

            StartBackgroundVideo();

            extraProgress = 0f;
            targetProgress = 0f;
            displayedProgress = 0f;

            SetLoadingStep("Loading " + sceneName + "...");
        }
        else
        {
            Time.timeScale = 1f;

            if (readyPanel != null)
                readyPanel.SetActive(false);

            if (loadingScreen != null)
                loadingScreen.SetActive(false);

            StopBackgroundVideo();
        }

        Scene currentScene = SceneManager.GetActiveScene();

        float timer = 0f;
        bool initDone = initializationSteps == null;

        AsyncOperation sceneLoad = SceneManager.LoadSceneAsync(sceneName, mode);
        sceneLoad.allowSceneActivation = false;

        if (initializationSteps != null)
        {
            StartCoroutine(RunInitialization(initializationSteps, () => initDone = true));
        }

        while (sceneLoad.progress < 0.9f || !initDone || (useLoadingScreen && timer < minimumLoadingScreenTime))
        {
            if (useLoadingScreen)
            {
                timer += Time.unscaledDeltaTime;

                float sceneProgress = Mathf.Clamp01(sceneLoad.progress / 0.9f);
                float combinedProgress = Mathf.Clamp01((sceneProgress * 0.7f) + (extraProgress * 0.3f));
                UpdateUI(combinedProgress);
            }

            yield return null;
        }

        if (useLoadingScreen)
        {
            UpdateUI(1f);
            SetLoadingStep(sceneName + " ready");

            if (readyPanel != null)
                readyPanel.SetActive(true);

            while (!continueRequested)
            {
                yield return null;
            }

            SetLoadingStep("Starting " + sceneName + "...");
        }

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

        // Only unload manually if this was an additive transition to a different scene
        if (mode == LoadSceneMode.Additive && currentScene.IsValid() && currentScene.name != sceneName)
        {
            yield return SceneManager.UnloadSceneAsync(currentScene);
        }

        Time.timeScale = 1f;

        yield return null;
        yield return null;

        if (useLoadingScreen)
        {
            StopBackgroundVideo();

            if (readyPanel != null)
                readyPanel.SetActive(false);

            if (loadingScreen != null)
                loadingScreen.SetActive(false);
        }

        if (IsGameplayScene(sceneName))
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        isLoading = false;
    }

    public void OnNextPressed()
    {
        continueRequested = true;
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
            videoPlayer.Stop();
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

    private bool IsGameplayScene(string sceneName)
    {
        return sceneName == "Level_1"
            || sceneName == "Level_2"
            || sceneName == "Level_3"
            || sceneName == "Level_4"
            || sceneName == "Squirrel_HUB";
    }

    private void ForceCloseAllPauseMenus()
    {
        PauseMenu[] pauseMenus = FindObjectsByType<PauseMenu>(FindObjectsSortMode.None);

        foreach (var pm in pauseMenus)
        {
            if (pm != null)
                pm.ForceClosePauseMenu();
        }
    }
}