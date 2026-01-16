using UnityEngine;
using UnityEngine.SceneManagement;

public class InitializationLoad : MonoBehaviour
{
    bool hasLoaded = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        SceneManager.SetActiveScene(SceneManager.GetActiveScene());
        SceneManager.LoadSceneAsync(1);
    }

    // Update is called once per frame
    void Update()
    {

    }
}
