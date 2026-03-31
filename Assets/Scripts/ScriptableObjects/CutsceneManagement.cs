using System.Net.Sockets;
using Unity.VisualScripting;
using UnityEngine;

public class CutsceneManagement : MonoBehaviour
{
    bool cutscenePlaying = false;
    public int currentIndex = -1;
    CutsceneAsset cutscene;
    public static CutsceneManagement Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Update()
    {
        if (cutscene == null || !cutscenePlaying) return;

        if (Input.GetMouseButtonDown(0))
        {
            // NEW LOGIC: Check if the text is still typing
            if (DialogManager.Instance.IsTyping)
            {
                // If it's typing, tell the manager to show the full sentence immediately
                DialogManager.Instance.FinishSentenceEarly();
            }
            else
            {
                // If the text is already finished, move to the next index
                AdvanceDialogue();
            }
        }
    }

    private void AdvanceDialogue()
    {
        currentIndex++;

        if (currentIndex >= cutscene.dialogLines.Count)
        {
            EndCutscene();
        }
        else
        {
            // Safety: Check if the other lists actually have an item at this index
            Sprite currentSprite = (currentIndex < cutscene.characterSprites.Count) 
                ? cutscene.characterSprites[currentIndex] 
                : null;

            string currentName = (currentIndex < cutscene.characterNames.Count) 
                ? cutscene.characterNames[currentIndex] 
                : "???";

            DialogManager.Instance.ShowDialog(
                cutscene.dialogLines[currentIndex], 
                currentSprite, 
                currentName
            );
        }
    }

    private void EndCutscene()
    {
        cutscenePlaying = false;
        DialogManager.Instance.HideDialog();
        cutscene = null;
        currentIndex = -1;
    }

    public void PlayCutscene(CutsceneAsset cutscene)
    {
        this.cutscene = cutscene;
        currentIndex = 0;
        cutscenePlaying = true;
        
        DialogManager.Instance.ShowDialog(
            cutscene.dialogLines[currentIndex], 
            cutscene.characterSprites[currentIndex], 
            cutscene.characterNames[currentIndex]
        );
    }
}