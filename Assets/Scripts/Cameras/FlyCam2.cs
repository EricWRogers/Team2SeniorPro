using UnityEngine;
using UnityEngine.Splines;
using System.Collections;

public class FlyCam2 : MonoBehaviour
{
    [Header("References")]
    public GameObject player; // Player object with scripts
    public GameObject playerCanvas; // Player UI Canvas
    public GameObject playerCam; // Player camera (Main Camera)
    public GameObject splineCamOBJ; // Spline camera object
    public SplineAnimate SplineCam; // Spline camera with spline animation component
    public MonoBehaviour pauseMenu; // Reference to pauseMenu script

    public bool inSequence = false;
    private static bool hasPlayedFlyover_Level1 = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        string currentScene = GameManager.Instance.currentScene;

        if (currentScene == "Squirrel_HUB" && !hasPlayedFlyover_Level1)
        {
            hasPlayedFlyover_Level1 = true;
            
            if (splineCamOBJ != null)
                splineCamOBJ.SetActive(true);
        
            StartCoroutine(StartSequence());
        }

        if (playerCanvas != null) playerCanvas.SetActive(false); // Hide at start
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
}
