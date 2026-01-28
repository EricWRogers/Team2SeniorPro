using UnityEngine;
using UnityEngine.Splines;
using System.Collections;
using TMPro;

public class FlyCam2 : MonoBehaviour
{
    [Header("References")]
    public GameObject player; // Player object with scripts
    public GameObject playerCanvas; // Player UI Canvas
    public GameObject playerCam; // Player camera (Main Camera)
    public GameObject splineCamOBJ; // Spline camera object
    public SplineAnimate SplineCam; // Spline camera with spline animation component
    public MonoBehaviour pauseMenu; // Reference to pauseMenu script
    public TMP_Text infoText; // Reference to info text UI element

    public bool inSequence = false;
    private static bool hasPlayedFlyover_Level1 = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        string currentScene = GameManager.Instance.GetCurrentScene();

        if (currentScene == "Level_1" && !hasPlayedFlyover_Level1)
        {
            hasPlayedFlyover_Level1 = true;
            
            if (splineCamOBJ != null)
                splineCamOBJ.SetActive(true);
        
            StartCoroutine(StartSequence());
        }
    }

    private IEnumerator StartSequence()
    {
        if (inSequence) yield break;
        inSequence = true;

        // Disable player scripts
        if (player != null)
        {
            foreach (var script in player.GetComponents<MonoBehaviour>())
                script.enabled = false;

        }

        // Disable Player Canvas
        if (playerCanvas != null)
            playerCanvas.SetActive(false);

        // Disable pause menu
        if (pauseMenu != null)
            pauseMenu.enabled = false;
        

        // Switch cameras
        if (playerCam != null) playerCam.SetActive(false);
        if (splineCamOBJ != null) splineCamOBJ.SetActive(true);

        // Message 1
        infoText.gameObject.SetActive(true);
        infoText.text = "Welcome to the Forest!";
        infoText.color = new Color(1, 1, 1, 0); // start invisible

        yield return StartCoroutine(FadeText(infoText, 0f, 1f, 1f)); // fade in
        yield return new WaitForSeconds(2f);                            // hold
        yield return StartCoroutine(FadeText(infoText, 1f, 0f, 1f)); // fade out

        // Message 2
        infoText.text = "Collect the key to unlock the gate ahead.";
        yield return StartCoroutine(FadeText(infoText, 0f, 1f, 1f));
        yield return new WaitForSeconds(2f);
        yield return StartCoroutine(FadeText(infoText, 1f, 0f, 1f));

        // Message 3
        infoText.text = "Press [R] to skip the Cutscene OR Restart.";
        yield return StartCoroutine(FadeText(infoText, 0f, 1f, 1f));
        yield return new WaitForSeconds(2f);
        yield return StartCoroutine(FadeText(infoText, 1f, 0f, 1f));

        infoText.gameObject.SetActive(false); // hide after all messages


        // Play spline forward and wait til finished
        if (SplineCam != null)
        {
            SplineCam.NormalizedTime = 0f;
            SplineCam.Play();

            yield return new WaitUntil(() => !SplineCam.IsPlaying);
            SplineCam.StopAllCoroutines();

        }

        // Unlock mouse and show cursor
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        EndSequence();
    }

    private void EndSequence()
    {
        // Switch cameras back
        if (playerCam != null) playerCam.SetActive(true);
        if (splineCamOBJ != null) splineCamOBJ.SetActive(false);

        // Re-enable player scripts
        if (player != null)
        {
            foreach (var script in player.GetComponents<MonoBehaviour>())
                script.enabled = true;
        }

        // Disable Player Canvas
        if (playerCanvas != null)
            playerCanvas.SetActive(true);

        // Re-enable pause menu
        if (pauseMenu != null)
            pauseMenu.enabled = true;

        inSequence = false;
    }

    private IEnumerator FadeText(TMP_Text text, float startAlpha, float endAlpha, float duration)
    {
        float t = 0f;
        Color c = text.color;

        while (t < duration)
        {
            t += Time.deltaTime;
            float a = Mathf.Lerp(startAlpha, endAlpha, t / duration);
            text.color = new Color(c.r, c.g, c.b, a);
            yield return null;
        }

        text.color = new Color(c.r, c.g, c.b, endAlpha);
    }

}
