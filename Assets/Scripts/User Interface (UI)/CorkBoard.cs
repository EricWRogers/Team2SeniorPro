using UnityEngine;
using UnityEngine.Splines;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class CorkBoard : MonoBehaviour
{
    [Header("References")]
    public GameObject player; // Player object with scripts
    public GameObject playerCam; // Player camera
    public GameObject splineCamOBJ; // Spline camera object
    public GameObject corkBoardCanvas; // Corkboard UI Canvas
    public GameObject challengeCanvas; // Challenge Canvas
    public GameObject playerCanvas;
    public GameObject KeyCodeCanvas; // Prompt canvas
    public SplineAnimate SplineCam; // Spline camera animation
    public MonoBehaviour pauseMenu; // Reference to pause menu script

    [Header("Controller/UI")]
    public PlayerInput playerInput;
    public GameObject firstSelectedObject;

    private PlayerControlsB controls;
    private bool interactPressedThisFrame;

    private bool playerInRange = false;
    private bool inSequence = false;
    private bool BoardActive = false;

    private void Awake()
    {
        controls = new PlayerControlsB();
    }

    private void OnEnable()
    {
        if (controls == null)
            controls = new PlayerControlsB();

        controls.Player.Interact.started += OnInteractStarted;
        controls.Player.Enable();
    }

    private void OnDisable()
    {
        if (controls != null)
        {
            controls.Player.Interact.started -= OnInteractStarted;
            controls.Player.Disable();
        }
    }

    private void Start()
    {
        if (playerInput == null)
            playerInput = FindFirstObjectByType<PlayerInput>();

        if (corkBoardCanvas != null)
            corkBoardCanvas.SetActive(false);

        if (splineCamOBJ != null)
            splineCamOBJ.SetActive(false);

        if (KeyCodeCanvas != null)
            KeyCodeCanvas.SetActive(false);
    }

    private void Update()
    {
        if (playerInRange && !inSequence && !BoardActive)
            KeyCodeCanvas.SetActive(true);
        else
            KeyCodeCanvas.SetActive(false);

        if (playerInRange && !inSequence && !BoardActive && interactPressedThisFrame)
        {
            interactPressedThisFrame = false;
            StartCoroutine(StartCorkBoardSequence());
        }

        // Safety reset so it only acts like a single press
        interactPressedThisFrame = false;
    }

    private void OnInteractStarted(InputAction.CallbackContext _)
    {
        interactPressedThisFrame = true;
    }

    private IEnumerator StartCorkBoardSequence()
    {
        inSequence = true;

        // Disable player scripts
        if (player != null)
        {
            foreach (var script in player.GetComponents<MonoBehaviour>())
            {
                if (script != null)
                    script.enabled = false;
            }
        }

        // Disable pause menu
        if (pauseMenu != null)
            pauseMenu.enabled = false;

        // Switch cameras
        if (playerCam != null)
            playerCam.SetActive(false);

        if (splineCamOBJ != null)
            splineCamOBJ.SetActive(true);

        // Play spline forward
        if (SplineCam != null)
        {
            SplineCam.NormalizedTime = 0f;
            SplineCam.Play();

            while (SplineCam.ElapsedTime < SplineCam.Duration)
                yield return null;
        }

        // Show corkboard UI
        if (corkBoardCanvas != null)
            corkBoardCanvas.SetActive(true);

        if (challengeCanvas != null)
            challengeCanvas.SetActive(true);

        // Corkboard open
        BoardActive = true;

        if (KeyCodeCanvas != null)
            KeyCodeCanvas.SetActive(false);

        if (playerCanvas != null)
            playerCanvas.SetActive(false);

        // Switch to UI controls for controller navigation
        if (playerInput == null)
            playerInput = FindFirstObjectByType<PlayerInput>();

        if (playerInput != null)
            playerInput.SwitchCurrentActionMap("UI");

        // Unlock mouse / show cursor
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Wait one frame so UI is fully active before selecting
        yield return null;

        if (EventSystem.current != null && firstSelectedObject != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(firstSelectedObject);
        }

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

        // Hide corkboard UI
        if (corkBoardCanvas != null)
            corkBoardCanvas.SetActive(false);

        // Hide Challenge Canvas
        if (challengeCanvas != null)
        {
            challengeCanvas.SetActive(false);
        }


        // Corkboard closed
        BoardActive = false;

        if (playerCanvas != null)
            playerCanvas.SetActive(true);

        // Clear selected UI object
        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(null);

        // Lock mouse / hide cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Play spline backwards
        if (SplineCam != null)
        {
            float reverseDuration = 2f;
            float elapsed = 0f;

            while (elapsed < reverseDuration)
            {
                elapsed += Time.deltaTime;
                SplineCam.NormalizedTime = Mathf.Lerp(1f, 0f, elapsed / reverseDuration);
                yield return null;
            }

            SplineCam.NormalizedTime = 0f;
        }

        // Switch cameras back
        if (playerCam != null)
            playerCam.SetActive(true);

        if (splineCamOBJ != null)
            splineCamOBJ.SetActive(false);

        // Re-enable player scripts
        if (player != null)
        {
            foreach (var script in player.GetComponents<MonoBehaviour>())
            {
                if (script != null)
                    script.enabled = true;
            }
        }

        // Re-enable pause menu
        if (pauseMenu != null)
            pauseMenu.enabled = true;

        // Return to gameplay controls
        if (playerInput == null)
            playerInput = FindFirstObjectByType<PlayerInput>();

        if (playerInput != null)
            playerInput.SwitchCurrentActionMap("Player");

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
            playerInRange = false;
    }

    // --- Level Loading ---
    public void Level_1() => LoadLevel("Level_1");
    public void Level_2() => LoadLevel("Level_2");
    public void Level_3() => LoadLevel("Level_3");
    public void Level_4() => LoadLevel("Level_4");
    public void Level_1_Challenge() => LoadLevel("Level_1_Challenge");
    public void Level_2_Challenge() => LoadLevel("Level_2_Challenge");
    public void Level_3_Challenge() => LoadLevel("Level_3_Challenge");
    public void Level_4_Challenge() => LoadLevel("Level_4_Challenge");

    private void LoadLevel(string levelName)
    {
        GameManager.Instance.newMap(levelName, true);
        Time.fixedDeltaTime = 1f / GameManager.Instance.frameRate;
    }
}