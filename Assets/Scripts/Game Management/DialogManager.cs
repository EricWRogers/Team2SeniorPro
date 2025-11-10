using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

public class DialogManager : MonoBehaviour
{
    public static DialogManager Instance { get; private set; }
    public Canvas dialogCanvas;
    public UnityEngine.UI.Image dialogPortrait;
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
        /*if (Input.GetMouseButtonUp(0)) //TODO: GET INPUT SYSTEM
        {
            if (dialogCanvas.gameObject.activeSelf)
            {
                HideDialog();
            }
            else
            {
                //Do nothing
            }
        }*/
    }
    public void ShowDialog(string dialog = "", Sprite _dialogPortrait = null, string characterName = "")
    {
        //Start of the Dialog String Function

        if (dialog != "")
        {
            if (dialog.StartsWith("$")) //check for string, if it starts with a dollar sign, then it's a localization key
            {
                dialog = dialog.Substring(1); //remove $ sign for localization lookup
                string LocalizedText = LocalizationSettings.StringDatabase.GetLocalizedString(dialog);
                dialogCanvasText.text = LocalizedText;
            }
            else //if no localization key, just show the string as is
            {
                dialogCanvasText.text = dialog;
            }

        }
        else //if nothing's given outright
        {
            Debug.LogError("No text provided, please pass input into ShowDialog");
            dialogCanvasText.text = "[No dialog text provided]";
        }

        //Start of the Character Name String Function

        if (characterName.StartsWith("$")) //check for string, if it starts with a dollar sign, then it's a localization key
        {
            characterName = characterName.Substring(1); //remove $ sign for localization lookup
            string LocalizedName = LocalizationSettings.StringDatabase.GetLocalizedString(characterName);
            characterNameText.text = LocalizedName;
        }

        if (characterName != "") //if name provided
        {
            characterNameText.text = characterName;
        }
        else //no name provided
        {
            characterNameText.text = "???";
        }

        //Start of the Dialog Portrait Sprite Function

        if (dialogPortrait != null)
        {
            dialogPortrait.sprite = _dialogPortrait;
        }
        else
        {
            dialogPortrait.GetComponent<UnityEngine.UI.Image>().sprite = _dialogPortrait;
        }
            

        dialogCanvas.gameObject.SetActive(true);
    }
    public void HideDialog()
    {
        dialogCanvas.gameObject.SetActive(false);
        //dialogCanvasText.text = "[No dialog text provided]"; //unload text
        //dialogCanvas.GetComponent<Sprite>().sprite = null;
    }
}
