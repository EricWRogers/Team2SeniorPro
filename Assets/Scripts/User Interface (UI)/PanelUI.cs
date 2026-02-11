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

    [Header("Level Name")]
    public string levelName; // MUST match Scene Name

    [Header("Rank Badges")]
    public GameObject Srank;
    public GameObject Arank;
    public GameObject Brank;
    public GameObject Crank;
    public GameObject Drank;

    // Show panel on hover
    public void OnPointerEnter(PointerEventData eventData)
    {
        statsPanel?.SetActive(true);
        RefreshStats();
    }

    // Hide panel when leaving
    public void OnPointerExit(PointerEventData eventData)
    {
        statsPanel?.SetActive(false);
    }

    public void RefreshStats()
    {
        if (DataManager.Instance == null) return;

        float bestTime = DataManager.Instance.GetBestTime(levelName);
        int bestScore = DataManager.Instance.GetBestScore(levelName);
        string bestRank = DataManager.Instance.GetBestRank(levelName);

        // Update text
        if (timerText != null)
            timerText.text = "Best Time: " + DataManager.Instance.FormatTime(bestTime);

        if (statsText != null)
            statsText.text = "Best Berries: " + bestScore;

        // Disable all rank badges first
        Srank?.SetActive(false);
        Arank?.SetActive(false);
        Brank?.SetActive(false);
        Crank?.SetActive(false);
        Drank?.SetActive(false);

        // Activate correct badge
        switch (bestRank)
        {
            case "S": Srank?.SetActive(true); break;
            case "A": Arank?.SetActive(true); break;
            case "B": Brank?.SetActive(true); break;
            case "C": Crank?.SetActive(true); break;
            case "D": Drank?.SetActive(true); break;
        }
    }
}