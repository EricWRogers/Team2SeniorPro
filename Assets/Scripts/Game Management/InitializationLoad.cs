using UnityEngine;
using UnityEngine.SceneManagement;

public class InitializationLoad : MonoBehaviour
{
    bool hasLoaded = false;
    bool hasTimeElapsed = false;
    float time = 0.0f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        SceneManager.SetActiveScene(SceneManager.GetActiveScene());
        //SceneManager.LoadSceneAsync(1);
        
    }

    // Update is called once per frame
    void Update()
    {
       if(time >= 1.0f && hasTimeElapsed == false)
       {
            hasTimeElapsed = true;
            LevelLoader.Instance.LoadLevel("Main Menu");
            this.enabled = false;
       }
       time += Time.deltaTime;
    }

}
