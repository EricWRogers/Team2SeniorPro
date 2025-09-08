using UnityEngine;
using UnityEngine.SceneManagement;

public class GameRestartOnDeplete : MonoBehaviour
{
    public PaintResource playerPaint;
    public float delay = 0.75f;

    void Start()
    {
        if (!playerPaint) playerPaint = FindObjectOfType<PaintResource>();
        if (playerPaint) playerPaint.OnPaintDepleted += Handle;
    }
    void OnDestroy()
    {
        if (playerPaint) playerPaint.OnPaintDepleted -= Handle;
    }
    void Handle() { Invoke(nameof(Reload), delay); }
    void Reload() { SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex); }
}
