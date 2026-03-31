using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Localization.Settings;
using System.Collections;

public class DialogManager : MonoBehaviour
{
    public static DialogManager Instance { get; private set; }
    public Canvas dialogCanvas;
    public UnityEngine.UI.Image dialogPortrait;
    public TMP_Text dialogCanvasText;
    public TMP_Text characterNameText;

    [Header("Typewriter Settings")]
    public float typingSpeed = 0.03f;
    public bool IsTyping { get; private set; }
    private Coroutine typingCoroutine;
    private string currentFullSentence;

    void Start()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        if (dialogCanvas == null) dialogCanvas = GetComponentInParent<Canvas>();
        
        dialogCanvas.gameObject.SetActive(false);
    }

    public void ShowDialog(string dialog = "", Sprite _dialogPortrait = null, string characterName = "")
    {
        // 1. Handle Localization and text prep
        string textToDisplay = dialog;
        if (!string.IsNullOrEmpty(dialog))
        {
            if (dialog.StartsWith("$"))
            {
                string key = dialog.Substring(1);
                textToDisplay = LocalizationSettings.StringDatabase.GetLocalizedString(key);
            }
        }
        else
        {
            textToDisplay = "[No dialog text provided]";
        }

        // 2. Handle Name Localization
        if (characterName.StartsWith("$"))
        {
            string nameKey = characterName.Substring(1);
            characterNameText.text = LocalizationSettings.StringDatabase.GetLocalizedString(nameKey);
        }
        else
        {
            characterNameText.text = string.IsNullOrEmpty(characterName) ? "???" : characterName;
        }

        // 3. Handle Portrait
        if (_dialogPortrait != null)
        {
            dialogPortrait.sprite = _dialogPortrait;
        }

        // 4. Start Typewriter Effect
        dialogCanvas.gameObject.SetActive(true);
        currentFullSentence = textToDisplay;

        if (typingCoroutine != null) StopCoroutine(typingCoroutine);
        typingCoroutine = StartCoroutine(TypeSentence(textToDisplay));
    }

    IEnumerator TypeSentence(string sentence)
    {
        IsTyping = true;
        dialogCanvasText.text = "";
        foreach (char letter in sentence.ToCharArray())
        {
            dialogCanvasText.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }
        IsTyping = false;
    }

    public void FinishSentenceEarly()
    {
        if (typingCoroutine != null) StopCoroutine(typingCoroutine);
        dialogCanvasText.text = currentFullSentence;
        IsTyping = false;
    }

    public void HideDialog()
    {
        if (typingCoroutine != null) StopCoroutine(typingCoroutine);
        IsTyping = false;
        dialogCanvas.gameObject.SetActive(false);
    }
}