using UnityEngine;
using UnityEngine.Splines;
using System.Collections;

public class CorkBoard : MonoBehaviour
{
    [Header("References")]
    public GameObject player; // Player object with scripts
    public GameObject playerCam; // Player camera (Main Camera)
    public GameObject splineCamOBJ; // Spline camera object
    public GameObject corkBoardCanvas; // Corkboard UI Canvas
    public GameObject playerCanvas;
    public GameObject KeyCodeCanvas; // Key Canvas
    public SplineAnimate SplineCam; // Spline camera with spline animation component
    public MonoBehaviour pauseMenu; // Reference to pauseMenu script

    [Header("Settings/Keybinds")]
    public KeyCode interactKey = KeyCode.E; // Interaction Key (Default: E)

    private bool playerInRange = false;
    private bool inSequence = false;
    private bool BoardActive = false;

    void Start()
    {
        if (corkBoardCanvas != null) corkBoardCanvas.SetActive(false); // Hide at start
        if (splineCamOBJ != null) splineCamOBJ.SetActive(false);
        if (KeyCodeCanvas != null) KeyCodeCanvas.SetActive(false);
    }

    void Update()
    {
        // Press "E" to start sequence text
        if (playerInRange && !inSequence && !BoardActive)
            KeyCodeCanvas.SetActive(true);
        else
            KeyCodeCanvas.SetActive(false);

        // Player presses E while inside trigger
        if (playerInRange && !inSequence && !BoardActive && Input.GetKeyDown(interactKey))
        {
            StartCoroutine(StartCorkBoardSequence());
        }
    }

    private IEnumerator StartCorkBoardSequence()
    {
        inSequence = true;

        // Disable player scripts
        if (player != null)
        {
            foreach (var script in player.GetComponents<MonoBehaviour>())
            {
                script.enabled = false;
            }
        }

        // Disable pause menu
        if (pauseMenu != null)
        {
            pauseMenu.enabled = false;
        }

        // Switch cameras
        if (playerCam != null) playerCam.SetActive(false);
        if (splineCamOBJ != null) splineCamOBJ.SetActive(true);

        // Play spline forward
        if (SplineCam != null)
        {
            SplineCam.NormalizedTime = 0f;
            SplineCam.Play();

            while (SplineCam.ElapsedTime < SplineCam.Duration)
                yield return null;

        }

        // Enable CorkBoard Canvas after spline finishes
        if (corkBoardCanvas != null)
            corkBoardCanvas.SetActive(true);

        // Corkboard open
        BoardActive = true;

        // Disable KeyCode Text
        if (KeyCodeCanvas != null)
            KeyCodeCanvas.SetActive(false);

        // Disable Player Canvas
        if (playerCanvas != null)
            playerCanvas.SetActive(false);

        // Unlock mouse and show cursor
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        inSequence = false;
    }

    // Called from UI Exit button on CorkBoard Canvas
    public void ExitCorkBoard()
    {
        if (!inSequence)
            StartCoroutine(ExitCorkBoardSequence());
    }

    private IEnumerator ExitCorkBoardSequence()
    {
        inSequence = true;

        // Hide CorkBoard Canvas
        if (corkBoardCanvas != null)
        {
            corkBoardCanvas.SetActive(false);
        }

        // Corkboard closed
        BoardActive = false;

        // Enable Player Canvas
        if (playerCanvas != null)
            playerCanvas.SetActive(true);

        // Lock and hide mouse
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Play spline backwards
        if (SplineCam != null)
        {
            float reverseDuration = 2f;
            float elapsed = 0f;

            while (elapsed < reverseDuration )
            {
                elapsed += Time.deltaTime;
                SplineCam.NormalizedTime = Mathf.Lerp(1f, 0f, elapsed / reverseDuration);
                yield return null;
            }

            SplineCam.NormalizedTime = 0f;
        }

        // Switch cameras back
        if (playerCam != null) playerCam.SetActive(true);
        if (splineCamOBJ != null) splineCamOBJ.SetActive(false);

        // Re-enable player scripts
        if (player != null)
        {
            foreach (var script in player.GetComponents<MonoBehaviour>())
                script.enabled = true;
        }

        // Re-enable pause menu
        if (pauseMenu != null)
            pauseMenu.enabled = true;

        inSequence = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == player)
            playerInRange = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject == player)
        {
            playerInRange = false;
        }
    }

    // --- Level Loading ---
    public void Level_1() => LoadLevel("Level_1");
    public void Level_2() => LoadLevel("Level_2");
    public void Level_3() => LoadLevel("Level_3");
    public void Level_4() => LoadLevel("Level_4");


    private void LoadLevel(string levelName)
    {
        GameManager.Instance.newMap(levelName, true);
        Time.fixedDeltaTime = 1f / GameManager.Instance.frameRate;
    }
}
