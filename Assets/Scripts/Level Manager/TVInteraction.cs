using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;

public class TVInteraction : MonoBehaviour
{
    [Header("Camera Setup")]
    public Camera mainCam;
    public Camera tvCam; 
    public Transform tvViewPoint;
    

    [Header("UI/Settings")]
    public float transitionSpeed = 0.6f;
    public GameObject exitPromptObj; // Drag your Text/UI obj here
    public List<GameObject> objectsToDisable; // Optional: Drag any specific objects you want to disable during TV view
    public List<MonoBehaviour> scriptsToDisable; // Optional: Drag any specific scripts you want to disable during TV view

    private bool isViewing = false;
    private bool wasInteracted = false;

    // List to remember which scripts we disable to turn them back on
    private List<MonoBehaviour> disabledScripts = new List<MonoBehaviour>();
    
    void Start()
    {
        if (mainCam == null) mainCam = Camera.main;
        
        tvCam.gameObject.SetActive(false); // Ensure TV cam is off at start

        if (exitPromptObj != null) exitPromptObj.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isViewing && !wasInteracted)
        {
            EnterView(other.gameObject);
        }
    }
    
    void Update()
    {
        if (isViewing && Input.GetKeyDown(KeyCode.E))
        {
            ExitView();
        }
    }

    void EnterView(GameObject player)
    {
        isViewing = true;
        disabledScripts.Clear();

        // Dynamically find and disable all player scripts
        MonoBehaviour[] scripts = player.GetComponentsInChildren<MonoBehaviour>();
        foreach (var script in scripts)
        {
            // We only disable if it's currently enabled and NOT this script
            if (script.enabled && script != this)
            {
                script.enabled = false;
                disabledScripts.Add(script);
            }
        }
        
        // Hide any specific objects if needed
        ToggleExtraObjects(false);

        // Disable spefic scripts
        ToggleExtraScripts(false);

        // Setup the TV camera to start exactly where the player is looking
        tvCam.transform.position = mainCam.transform.position;
        tvCam.transform.rotation = mainCam.transform.rotation;

        // Disable main cam, enable TV
        mainCam.gameObject.SetActive(false);
        tvCam.gameObject.SetActive(true);

        // Target positions.
        // Move to the TV point, but offset the Z by 0.98 so we aren't inside the TV screen. Adjust as needed based on your model.
        Vector3 targetPos = tvViewPoint.position + tvViewPoint.forward; // Move back along the forward axis

        // Dotween transitions (Ignoring Time.timeScale): Move TV Cam to the TV view point
        tvCam.transform.DOMove(targetPos, transitionSpeed).SetUpdate(true).SetEase(Ease.OutQuad);

        tvCam.transform.DOLocalRotate(new Vector3(0, 180, 0), transitionSpeed).SetUpdate(true).SetEase(Ease.OutQuad);

        // Enable the exit prompt UI
        if (exitPromptObj != null) exitPromptObj.SetActive(true);

        // Freeze time
        DOTween.To(() => Time.timeScale, x => Time.timeScale = x, 0f, transitionSpeed).SetUpdate(true);
    }

    void ExitView()
    {
        wasInteracted = true;

        // Disable the exit prompt UI
        if (exitPromptObj != null) exitPromptObj.SetActive(false);

        // Reverse Camera transition: Move TV Cam back to the player's original position and rotation
        tvCam.transform.DOMove(mainCam.transform.position, transitionSpeed).SetUpdate(true);
        tvCam.transform.DORotateQuaternion(mainCam.transform.rotation, transitionSpeed).SetUpdate(true).OnComplete(() =>
        {
            // Switch back to main cam
            tvCam.gameObject.SetActive(false);
            mainCam.gameObject.SetActive(true);

            // Re-enable any specific objects if needed
            ToggleExtraObjects(true);

            // Re-enable specific scripts
            ToggleExtraScripts(true);

            // Re-enable previously disabled scripts
            foreach (var script in disabledScripts)
            {
                if (script != null) // Check if the script still exists
                {
                    script.enabled = true;
                }
                isViewing = false;
            }
        });

        // Unfreeze time
        DOTween.To(() => Time.timeScale, x => Time.timeScale = x, 1f, transitionSpeed).SetUpdate(true);
    }

    // Helper method to handle the list of objects (UI for example)
    void ToggleExtraObjects(bool state)
    {
        foreach (var obj in objectsToDisable)
        {
            if (obj != null) obj.SetActive(state);
        }
    }

    // Helper method to handle the list of scripts (if you want to disable specific ones instead of all player scripts)
    void ToggleExtraScripts(bool state)
    {
        foreach (MonoBehaviour script in scriptsToDisable)
        {
            if (script != null) script.enabled = state;
        }
    }
}
