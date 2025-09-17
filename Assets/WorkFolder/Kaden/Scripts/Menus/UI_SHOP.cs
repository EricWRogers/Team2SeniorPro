using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections;
using UnityEngine.UI;
public class UI_SHOP : MonoBehaviour
{
    private Transform container;
    private Transform shopItemTemplate;
    private PlayerCurrency playerCurrency;
    private bool canClick = true;
    [SerializeField] private float clickCooldown = 1.5f;

    public GameObject UPG;
    public GameObject UPG2;
    public GameObject UPGMAX;

    [Header("Upgrade Button")]
    public Button upgradeButton;

    [Header("Tooltip UI")]
    public GameObject tooltipPanel;
    public TMP_Text tooltipText;

    [Header("Click Feedback UI")]
    public TMP_Text feedbackText;
    public float feedbackDuration = 2f;

    private void Awake()
    {
        container = transform.Find("container");
        shopItemTemplate = container.Find("shopUpgTemplate");
        shopItemTemplate.gameObject.SetActive(true);

        playerCurrency = Object.FindFirstObjectByType<PlayerCurrency>();

        if (tooltipPanel != null)
        {
            tooltipPanel.SetActive(false);
        }

        if (upgradeButton != null)
        {
            AddTooltipEvents(upgradeButton.gameObject, GetUpgradeTooltip());
            upgradeButton.onClick.AddListener(Upgrade);
        }

        if (feedbackText != null)
        {
            feedbackText.gameObject.SetActive(false);
        }
    }

    public void Upgrade()
    {
        if (!canClick) return;  // stop if still cooling down
        StartCoroutine(ClickDelay()); // start cooldown

        if (playerCurrency == null) return;
        Debug.Log("Upgrade button pressed. Pigments: " + playerCurrency.pigment);

        // Try to unlock first upgrade
        if (!UPG.activeSelf && playerCurrency.pigment >= 1)
        {
            UPG.SetActive(true);
            playerCurrency.AddPigment(-1);
            ShowFeedback("<color=green>First upgrade unlocked!</color>");
            return;
        }
        // Try to unlock second upgrade
        if (!UPG2.activeSelf && playerCurrency.pigment >= 50)
        {
            UPG2.SetActive(true);
            playerCurrency.AddPigment(-50);
            ShowFeedback("<color=green>Second upgrade unlocked!</color>");
            return;
        }
        // Try to unlock max upgrade
        if (!UPGMAX.activeSelf && playerCurrency.pigment >= 100)
        {
            UPGMAX.SetActive(true);
            playerCurrency.AddPigment(-100);
            ShowFeedback("<color=green>Max upgrade unlocked!</color>");
            return;
        }

        // If nothing unlocked, show feedback
        ShowFeedback("<color=red>Not enough pigment!</color>");
    }

    private IEnumerator ClickDelay()
    {
        canClick = false;
        yield return new WaitForSeconds(clickCooldown);
        canClick = true;
    }


    private void AddTooltipEvents(GameObject obj, string message)
    {
        if (obj == null) return;

        EventTrigger trigger = obj.GetComponent<EventTrigger>();
        if (trigger == null)
        {
            trigger = obj.AddComponent<EventTrigger>();
        }

        // OnPointerEnter
        EventTrigger.Entry enter = new EventTrigger.Entry();
        enter.eventID = EventTriggerType.PointerEnter;
        enter.callback.AddListener((data) => ShowTooltip(message));
        trigger.triggers.Add(enter);

        // OnPointerExit
        EventTrigger.Entry exit = new EventTrigger.Entry();
        exit.eventID = EventTriggerType.PointerExit;
        exit.callback.AddListener((data) => HideTooltip());
        trigger.triggers.Add(exit);
    }

    private void ShowTooltip(string message)
    {
        if (tooltipPanel != null && tooltipText != null)
        {
            tooltipPanel.SetActive(true);
            tooltipText.text = message;
        }
    }

    private void HideTooltip()
    {
        if (tooltipPanel != null)
        {
            tooltipPanel.SetActive(false);
        }
    }

    private Coroutine feedbackCoroutine;

    private void ShowFeedback(string message)
    {
        if (feedbackText == null) return;

        // Stop any previous feedback coroutine
        if (feedbackCoroutine != null)
            StopCoroutine(feedbackCoroutine);

        feedbackText.gameObject.SetActive(true);
        feedbackCoroutine = StartCoroutine(SequentialFadeFeedbackCoroutine(message));
    }

    private IEnumerator SequentialFadeFeedbackCoroutine(string message)
    {
        string[] lines = message.Trim().Split('\n'); // <-- trim whitespace

        foreach (string line in lines)
        {
            if (string.IsNullOrEmpty(line)) continue; // skip empty lines

            feedbackText.text = line;
            feedbackText.alpha = 1f;

            float elapsed = 0f;
            while (elapsed < feedbackDuration)
            {
                elapsed += Time.deltaTime;
                feedbackText.alpha = Mathf.Lerp(1f, 0f, elapsed / feedbackDuration);
                yield return null;
            }

            feedbackText.alpha = 0f;

            yield return new WaitForSeconds(0.5f);
        }

        feedbackText.gameObject.SetActive(false);
    }

    private string GetUpgradeTooltip()
    {
        return "<u>Upgrades:</u>\n" +
           "<color=red>1 Pigment</color> → 1st\n\n" +
           "<color=#5DADE2>50 Pigments</color> → 2nd\n\n" +
           "<color=#006400>100 Pigments</color> → Max";
    }
}