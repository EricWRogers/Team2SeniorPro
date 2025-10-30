using UnityEngine;
using UnityEngine.EventSystems;

public class PanelUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("UI Panel Toggle")]
    public GameObject statsPanel;

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
