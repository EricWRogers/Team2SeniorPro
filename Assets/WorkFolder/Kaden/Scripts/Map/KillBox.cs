using UnityEngine;
using UnityEngine.SceneManagement;
public class KillBox : MonoBehaviour
{
    public float reloadDelay = 0.2f;

    void Reset() { GetComponent<Collider>().isTrigger = true; }

    void OnTriggerEnter(Collider other)
    {
        if (!IsPlayer(other)) return;
        Invoke(nameof(Reload), reloadDelay);
    }

    bool IsPlayer(Collider c)
    {
        // works with child too
        return c.CompareTag("Player") ||
               c.GetComponentInParent<PaintResource>() != null ||
               c.GetComponent<PaintResource>() != null;
    }

    void Reload()
    {
        var idx = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(idx);
    }
}
