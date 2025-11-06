using UnityEngine;


public class InteractDisplay : MonoBehaviour
{
    public Collider meshCollider;
    public Canvas buttonDisplay;
    public bool IsPlayerInRange = false;
    public CutsceneAsset cutsceneToPlay;
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
        }
    }
    void OnTriggerExit(Collider other)
    {
        Debug.Log(other.gameObject.transform);
        if (other.gameObject.transform.tag == "Player")
        {
            buttonDisplay.enabled = false;
            IsPlayerInRange = false;
        }
        /*if(DialogManager.Instance.dialogCanvas.gameObject.activeSelf)
        {
            DialogManager.Instance.HideDialog();
        }*/
    }
    void Update()
    {
        if (IsPlayerInRange && Input.GetKeyDown(KeyCode.E) && DialogManager.Instance.dialogCanvas.gameObject.activeSelf == false)
        {
            //DialogManager.Instance.ShowDialog("TutorialNPCDialog");
            CutsceneManagement.Instance.PlayCutscene(cutsceneToPlay);
        }
    }
}
