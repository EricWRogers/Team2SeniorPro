using UnityEngine;
using UnityEngine.SceneManagement;

public class RestartOnKey : MonoBehaviour
{
    [Tooltip("Key used to restart the level.")]
    public KeyCode restartKey = KeyCode.R;

    [Tooltip("hold the key for this long to restart. Set to 0 for instant.")]
    public float holdSeconds = 0f;

    float holdTimer = 0f;

    void Update()
    {
        // instant restart
        if (holdSeconds <= 0f)
        {
            if (Input.GetKeyDown(restartKey))
                Restart();
            return;
        }

        // hold-to-restart
        if (Input.GetKey(restartKey))
        {
            holdTimer += Time.unscaledDeltaTime;
            if (holdTimer >= holdSeconds)
                Restart();
        }
        else
        {
            holdTimer = 0f;
        }
    }

    void Restart()
    {
        
        Time.timeScale = 1f;

        
        var scene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(scene.buildIndex, LoadSceneMode.Single);
    }
}
