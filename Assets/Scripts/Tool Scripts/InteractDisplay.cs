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

    private Animator playerAnim;
    private Rigidbody playerRb;
    private GameObject playerRoot;

    void Awake()
    {
        if (meshCollider == null) meshCollider = GetComponentInParent<Collider>();
        if (buttonDisplay == null) buttonDisplay = GetComponentInParent<Canvas>();
        if (buttonDisplay != null) buttonDisplay.enabled = false;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (buttonDisplay != null) buttonDisplay.enabled = true;
            IsPlayerInRange = true;

            playerRoot = other.gameObject.transform.root.gameObject;
            playerAnim = other.GetComponentInChildren<Animator>();
            playerRb = other.GetComponentInChildren<Rigidbody>();
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (buttonDisplay != null) buttonDisplay.enabled = false;
            IsPlayerInRange = false;

            if (isInteracting) StopTalking();

            playerRoot = null;
            playerAnim = null;
            playerRb = null;
        }
    }

    void Update()
    {
        if (IsPlayerInRange && Input.GetKeyDown(KeyCode.F))
        {
            // Normal check execution
            bool isDialogActive = DialogManager.Instance != null && 
                                  DialogManager.Instance.dialogCanvas != null && 
                                  DialogManager.Instance.dialogCanvas.gameObject.activeSelf;

            if (!isInteracting && !isDialogActive)
            {
                StartTalking();
            }
        }
    }

    private void StartTalking()
    {
        isInteracting = true;
        if (buttonDisplay != null) buttonDisplay.enabled = false;

        if (cutsceneToPlay != null && CutsceneManagement.Instance != null)
        {
            CutsceneManagement.Instance.PlayCutscene(cutsceneToPlay);
        }
        else
        {
            Debug.LogWarning("InteractDisplay: CutsceneManagement.Instance or cutsceneToPlay is missing!");
        }

        if (playerAnim != null) playerAnim.SetBool("isTalking", true);
        TogglePlayerControl(false);
    }

    private void StopTalking()
    {
        isInteracting = false;
        if (playerAnim != null) playerAnim.SetBool("isTalking", false);
        if (IsPlayerInRange && buttonDisplay != null) buttonDisplay.enabled = true;
        TogglePlayerControl(true);
    }

    private void TogglePlayerControl(bool state)
    {
        if (playerRoot == null) return;

        MonoBehaviour[] allScripts = playerRoot.GetComponentsInChildren<MonoBehaviour>();
        foreach (var script in allScripts)
        {
            if (script == null) continue;
            string n = script.GetType().Name;
            if (n.Contains("New") || n.Contains("Climbing") || n.Contains("Grab") || n.Contains("Sliding"))
            {
                script.enabled = state;
            }
        }

        if (playerRb != null)
        {
            // Reset velocity while still dynamic
            playerRb.linearVelocity = Vector3.zero; 
            
            // turn on kinematic so the player freezes during dialogue
            playerRb.isKinematic = !state; 
        }
    }
}