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

    [Header("Spline Speed Control")]
    public float baseSplineSpeed = 0.1f;  // normal speed
    public float speedMultiplier = 3f;    // boost when holding space

    [Header("Spline Ease-Out")]
    public float endEaseSpeed = 0.05f; // adjustable speed of the final ease

    public bool inSequence = false;
    private bool splinePlaying = false;
    private static bool hasPlayedFlyover_Level1 = false;
    private bool skipText = false;    // True when player wants to skip current text

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        string currentScene = GameManager.Instance.GetCurrentScene();

        if ((currentScene == "Level_1" || currentScene == "Level_2") && !hasPlayedFlyover_Level1)
        {
            hasPlayedFlyover_Level1 = true;
            
            if (splineCamOBJ != null)
                splineCamOBJ.SetActive(true);
        
            StartCoroutine(StartSequence());
        }
    }

    void Update()
    {
        // Skip cutscene
        if (inSequence && Input.GetKeyDown(KeyCode.Q))
        {
            SkipCutscene();
        }

        // Skip current text
        if (inSequence && Input.GetMouseButtonDown(0))
        {
            skipText = true;
        }

        if (splinePlaying && SplineCam != null)
        {
            float currentMultiplier = Input.GetKey(KeyCode.Space) ? speedMultiplier : 1f;

            // Normal spline advance
            float targetTime = SplineCam.NormalizedTime + baseSplineSpeed * currentMultiplier * Time.unscaledDeltaTime;

            // Ease out at the end
            float distanceToEnd = 1f - SplineCam.NormalizedTime;
            if (distanceToEnd < 0.2f) // last 20% of spline
            {
                // The closer we are to the end, the slower we move
                SplineCam.NormalizedTime += (1f - SplineCam.NormalizedTime) * endEaseSpeed;
            }
            else
            {
                SplineCam.NormalizedTime = targetTime;
            }

            // Stop at the end
            if (SplineCam.NormalizedTime >= 0.999f)
            {
                SplineCam.NormalizedTime = 1f;
                splinePlaying = false;
            }
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

        // Enable info text
        infoText.gameObject.SetActive(true);

        // Show messages with skip ability
        yield return StartCoroutine(ShowMessage("Welcome to the Forest!", 1.2f));
        yield return StartCoroutine(ShowMessage("Collect the key to unlock the gate ahead.", 1.2f));
        yield return StartCoroutine(ShowMessage("Press [Q] to skip Scene & [R] to Restart Level.", 1.2f));
        yield return StartCoroutine(ShowMessage("Press/Hold [SPACE] to speed up Scene.", 1.2f));

        infoText.gameObject.SetActive(false); // hide after all messages

        // Play spline manually
        if (SplineCam != null)
        {
            SplineCam.NormalizedTime = 0f;
            splinePlaying = true;  // NOW spline advances

            // Wait until we finish manually advancing in Update()
            yield return new WaitUntil(() => !splinePlaying);
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

        // Enable Player Canvas
        if (playerCanvas != null)
            playerCanvas.SetActive(true);

        // Re-enable pause menu
        if (pauseMenu != null)
            pauseMenu.enabled = true;

        // Lock mouse and hide cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        inSequence = false;
    }

    private void SkipCutscene()
    {
        // Immediately stop spline
        if (SplineCam != null)
        {
            SplineCam.NormalizedTime = 1f;
            inSequence = false;
        }

        // Hide text
        if (infoText != null)
            infoText.gameObject.SetActive(false);

        // End cutscene immediately
        EndSequence();
    }

    private IEnumerator ShowMessage(string message, float holdTime)
    {
        infoText.text = message;
        infoText.color = new Color(1, 1, 1, 0);

        float t = 0f;
        Color c = infoText.color;

        // Fade in (skip on click)
        while (t < 1f)
        {
            if (skipText) break; // immediately show full text
            t += Time.deltaTime;
            infoText.color = new Color(c.r, c.g, c.b, t);
            yield return null;
        }
        infoText.color = new Color(c.r, c.g, c.b, 1f);

        // Hold message (skip if mouse held)
        float hold = 0f;
        while (hold < holdTime)
        {
            if (skipText)
                break; // immediately move to fade out
            hold += Time.deltaTime;
            yield return null;
        }

        // Fade out (skip on click)
        t = 0f;
        while (t < 1f)
        {
            if (skipText) break;
            t += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, t);
            infoText.color = new Color(c.r, c.g, c.b, alpha);
            yield return null;
        }
        infoText.color = new Color(c.r, c.g, c.b, 0f);

        skipText = false; // reset for next message
    }

}
