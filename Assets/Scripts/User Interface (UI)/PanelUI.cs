using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class PanelUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("UI Panel Toggle")]
    public GameObject statsPanel;

    [Header("Text Elements")]
    public TMP_Text statsText;
    public TMP_Text timerText;

    [Header("Data Manager Reference")]
    public DataManager DM;

    void Start()
    {
        /*if (DM == null)
        {
            DM = Object.FindFirstObjectByType<DataManager>();
        }*/

        //DM.UpdateStats();
        //DM.UpdateTimer();
    }

    // Reveal the stats panel when the pointer hovers over the button area
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (statsPanel != null)
        {
            statsPanel.SetActive(true);
        }
    }

    // Hide the stats panel when the pointer exits the button area
    public void OnPointerExit(PointerEventData eventData)
    {
        if (statsPanel != null)
        {
            statsPanel.SetActive(false);
        }
    }
}
