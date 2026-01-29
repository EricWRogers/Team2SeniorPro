using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class LevelLoader : MonoBehaviour
{
    public static LevelLoader Instance { get; private set; }
    bool hasLoaded = false;
    public GameObject loadingScreen;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
        if (loadingScreen == null)
        {
            loadingScreen = GameObject.Find("LoadingCanvas");
        }
    }

    void Update()
    {
        if (!hasLoaded && SceneManager.GetSceneByBuildIndex(1).isLoaded)
        {
            hasLoaded = true;
            SceneManager.SetActiveScene(SceneManager.GetSceneByBuildIndex(1));
        }
    }

    public void LoadLevel(string levelName)
    {
        //UnloadLevel();


        loadingScreen.SetActive(true);
        StartCoroutine(LoadAsyncScene(levelName));
    }

    public void UnloadLevel()
    {
        SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene().buildIndex);
        Resources.UnloadUnusedAssets();
    }

    IEnumerator LoadAsyncScene(string sceneName)
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);

        asyncLoad.allowSceneActivation = false;

        while (!asyncLoad.isDone)
        {
            float progressBar = Mathf.Clamp01(asyncLoad.progress / 0.9f);
            //progressBar.value = progress;
            Debug.Log("" + progressBar);
            //SceneManager.
            
            if (asyncLoad.progress >= 0.9f)
            {
                asyncLoad.allowSceneActivation = true;
                SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene().buildIndex);
                loadingScreen.SetActive(false);
                
            }

            yield return null;
        }
    }

}
