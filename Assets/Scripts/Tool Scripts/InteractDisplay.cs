using UnityEngine;


public class InteractDisplay : MonoBehaviour
{
    [Header("References")]
    public Collider meshCollider;
    public Canvas buttonDisplay;
    public CutsceneAsset cutsceneToPlay;
    
    [Header("Status")]
    public bool IsPlayerInRange = false;
    private bool isInteracting = false;

    // Internal references to the player components
    private Animator playerAnim;
    private Rigidbody playerRb;
    private GameObject playerRoot;

    void Awake()
    {
        if (meshCollider == null)
        {
            meshCollider = GetComponentInParent<Collider>();
        }
        if (buttonDisplay == null)
        {
            buttonDisplay = GetComponentInParent<Canvas>();
        }
        if (buttonDisplay != null)
        {
            buttonDisplay.enabled = false;
        }
    }
    void OnTriggerEnter(Collider other)
    {
        Debug.Log(other.gameObject.transform);
        if (other.gameObject.transform.tag == "Player")
        {
            buttonDisplay.enabled = true;
            IsPlayerInRange = true;

            playerRoot = other.gameObject.transform.root.gameObject;
            playerAnim = other.GetComponentInChildren<Animator>();
            playerRb = other.GetComponentInChildren<Rigidbody>();
        }
    }
    void OnTriggerExit(Collider other)
    {
        Debug.Log(other.gameObject.transform);
        if (other.gameObject.transform.tag == "Player")
        {
            buttonDisplay.enabled = false;
            IsPlayerInRange = false;

            StopTalking();

            // Clear references when player leaves range
            playerRoot = null;
            playerAnim = null;
            playerRb = null;
        }
    }

    void Update()
    {
        // Start Interaction
        if (IsPlayerInRange && Input.GetKeyDown(KeyCode.F) && !DialogManager.Instance.dialogCanvas.gameObject.activeSelf)
        {
           StartTalking();
        }

        // Auto-Stop Check
        if (isInteracting && !DialogManager.Instance.dialogCanvas.gameObject.activeSelf)
        {
            StopTalking();
        }
    }
    // Helper function to start the talking interaction
    private void StartTalking()
    {
        isInteracting = true;

        //DialogManager.Instance.ShowDialog("TutorialNPCDialog");
        CutsceneManagement.Instance.PlayCutscene(cutsceneToPlay);

        if (playerAnim != null) playerAnim.SetBool("isTalking", true);

        TogglePlayerControl(false);
    }

    // Helper function to reset the player to normal
    private void StopTalking()
    {
        isInteracting = false;
        if (playerAnim != null) playerAnim.SetBool("isTalking", false);

        TogglePlayerControl(true);
    }

    private void TogglePlayerControl(bool state)
    {
        if (playerRoot == null) return;

        // Disable/Enable specific movement scripts on the child
        MonoBehaviour[] allScripts = playerRoot.GetComponentsInChildren<MonoBehaviour>();
        foreach (var script in allScripts)
        {
            // We don't want to disable the Animator or this script;s ability to see the player
            string n = script.GetType().Name;
            if (n.Contains("New") || n.Contains("Climbing") || n.Contains("Grab") || n.Contains("Sliding"))
            {
                script.enabled = state;
            }
        }

        if (playerRb != null)
        {
            playerRb.linearVelocity = Vector3.zero;
            playerRb.isKinematic = !state;
        }
    }
}
