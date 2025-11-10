using System.Net.Sockets;
using Unity.VisualScripting;
using UnityEngine;

public class CutsceneManagement : MonoBehaviour
{
    bool advanceCutscene = false;
    bool cutscenePlaying = true;
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
        if (cutscene != null)
        {
            if (cutscenePlaying)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    if (currentIndex < cutscene.dialogLines.Count)
                    {
                        if (currentIndex + 1 < cutscene.dialogLines.Count) //only advance if there is another line
                        {
                            currentIndex++;
                        }
                        else
                        {
                            currentIndex = -1; //set to end
                        }
                        if (currentIndex != -1)
                        {
                            DialogManager.Instance.ShowDialog(cutscene.dialogLines[currentIndex], cutscene.characterSprites[currentIndex], cutscene.characterNames[currentIndex]);
                        }
                    }
                    if (currentIndex == -1) //forgot, haha, that the index starts at zero.
                    {
                        cutscenePlaying = false;
                        DialogManager.Instance.HideDialog();
                        //Re-enable player controls here
                        cutscene = null;
                        currentIndex = -1;
                    }
                }
                advanceCutscene = false;
            }
        }
    }
    public void PlayCutscene(CutsceneAsset cutscene)
    {
        this.cutscene = cutscene;
        currentIndex = 0;
        cutscenePlaying = true;
        advanceCutscene = false;
        DialogManager.Instance.ShowDialog(cutscene.dialogLines[currentIndex], cutscene.characterSprites[currentIndex], cutscene.characterNames[currentIndex]);

        //DialogManager.Instance.HideDialog();
        //Re-enable player controls here

    }

}
