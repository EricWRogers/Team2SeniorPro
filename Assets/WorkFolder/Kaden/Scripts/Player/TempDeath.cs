using UnityEngine;
using UnityEngine.SceneManagement;

public class TempDeath : MonoBehaviour
{
    public PaintResource playerPaint;
    public float reloadDelay = 0.75f;

    void Start()
    {
        if (!playerPaint) playerPaint = FindObjectOfType<PaintResource>();
        if (playerPaint) playerPaint.OnPaintDepleted += HandleDeath;
    }
    void OnDestroy()
    {
        if (playerPaint) playerPaint.OnPaintDepleted -= HandleDeath;
    }
    void HandleDeath()
    {
        Invoke(nameof(Reload), reloadDelay);
    }
    void Reload()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        // TO DO  later, send player to upgrade stats screen instead
    }
}