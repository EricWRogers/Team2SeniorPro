using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

public class DialogManager : MonoBehaviour
{
    public static DialogManager Instance { get; private set; }
    public Canvas dialogCanvas;
    public TMP_Text dialogCanvasText;
    public TMP_Text characterNameText;
    void Start()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        if (dialogCanvas == null)
        {
            dialogCanvas = GetComponentInParent<Canvas>();
        }
        dialogCanvas.gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonUp(0)) //TODO: GET INPUT SYSTEM
        {
            if (dialogCanvas.gameObject.activeSelf)
            {
                HideDialog();
            }
            else
            {
                //Do nothing
            }
        }
    }
    public void ShowDialog(string dialog = "", string characterName = "", Sprite dialogPortrait = null)
    {
        if (dialog != "")
        {
            string LocalizedText = LocalizationSettings.StringDatabase.GetLocalizedString(dialog);
            dialogCanvasText.text = LocalizedText;
        }
        else
        {
            Debug.LogError("No text provided, please pass input into ShowDialog");
            dialogCanvasText.text = "[No dialog text provided]";
        }
        if(characterName != "")
        {
            characterNameText.text = characterName;
        }
        else
        {
            characterNameText.text = "???";
        }
        //dialogCanvas.GetComponent<Sprite>().sprite = dialogPortrait;
        dialogCanvas.gameObject.SetActive(true);
    }
    public void HideDialog()
    {
        dialogCanvas.gameObject.SetActive(false);
        dialogCanvasText.text = "[No dialog text provided]"; //unload text
        //dialogCanvas.GetComponent<Sprite>().sprite = null;
    }
}
