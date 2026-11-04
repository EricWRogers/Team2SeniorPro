using UnityEngine;
using UnityEngine.UI;

public class PowerupSliderUI : MonoBehaviour
{
    public enum PowerupType
    {
        Speed,
        Jump
    }

    [Header("References")]
    public NewThirdPlayerMovement playerMovement;

    [Tooltip("IMPORTANT: This should be a CHILD object containing the art/visuals, NOT the object this script is attached to!")]
    public GameObject sliderVisualsContainer;
    public Slider slider;

    [Header("Powerup")]
    public PowerupType powerupType = PowerupType.Speed;

    [Header("Settings")]
    public bool hideWhenInactive = true;

    [Header("Polish & Animation Settings")]
    [Tooltip("How fast the slider bar moves to smoothly match the data time")]
    public float barDrainSpeed = 10f;
    [Tooltip("How fast the UI panel scales up and down when appearing/disappearing.")]
    public float animationPopSpeed = 8f;

    [Header("Low Time Flashing Alerts")]
    [Tooltip("The fill graphic component of the slide that changes color.")]
    public Image sliderFillImage;
    [Tooltip("The regular base color of the bar.")]
    public Color normalColor = Color.white;
    [Tooltip("The alert color the bar flashes to when low on time.")]
    public Color flashColor = Color.red;
    [Tooltip("At how many remaining seconds should the bar start flashing?")]
    public float criticalTimeThreshold = 1.2f;
    [Tooltip("How fast the color pulses back and forth.")]
    public float flashSpeed = 15f;

    private Vector3 targetScale = Vector3.zero;
    private Vector3 originalMaxScale = Vector3.one;

    private void Start()
    {
        // Guard against build order issues: actively look for the player if reference is empty
        if (playerMovement == null)
            playerMovement = FindFirstObjectByType<NewThirdPlayerMovement>();

        // If you didn't assign a visuals container, fallback safely to the slider's graphic components
        if (sliderVisualsContainer == null && slider != null)
        {
            sliderVisualsContainer = slider.gameObject;
        }

        // Initializing the scale to completely flat if it starts inactive
        if (sliderVisualsContainer != null)
        {
            originalMaxScale = sliderVisualsContainer.transform.localScale;
            sliderVisualsContainer.transform.localScale = Vector3.zero;
        }

        // Set baseline color
        if (sliderFillImage != null)
        {
            sliderFillImage.color = normalColor;
        }
    }

    private void Update()
    {
        // If we lost the player ference (like on scene load), try to find them again dynamically
        if (playerMovement == null || slider == null)
        {
            playerMovement = FindFirstObjectByType<NewThirdPlayerMovement>();
            if (playerMovement == null || slider == null) return;
        }

        if (slider == null) return;

        float timeLeft = 0f;
        float maxTime = 0f;

        switch (powerupType)
        {
            case PowerupType.Speed:
                timeLeft = playerMovement.speedBoostTimeLeft;
                maxTime = playerMovement.speedBoostMaxTime;
                break;

            case PowerupType.Jump:
                timeLeft = playerMovement.jumpBoostTimeLeft;
                maxTime = playerMovement.jumpBoostMaxTime;
                break;
        }

        bool active = timeLeft > 0f;

        // --- EFFECT 1: ANIMATION POP & LIFECYCLE MANAGEMENT ---
        if (hideWhenInactive && sliderVisualsContainer != null)
        {
            // Set target scales based on activity
            targetScale = active ? originalMaxScale : Vector3.zero;

            // Smoothly interpolate the current scale towards the target scale
            sliderVisualsContainer.transform.localScale = Vector3.Lerp(
                sliderVisualsContainer.transform.localScale,
                targetScale,
                animationPopSpeed * Time.deltaTime
            );

            // Turn on/off active states safely so it doesn't process unnecessarily when completely hidden
            if (!active && sliderVisualsContainer.transform.localScale.x < 0.01f)
            {
                if (sliderVisualsContainer.activeSelf) sliderVisualsContainer.SetActive(false);
                return; // Guard statement to save performance when fully closed
            }
            else
            {
                if (!sliderVisualsContainer.activeSelf) sliderVisualsContainer.SetActive(true);
            }
        }

        // --- EFFECT 2: SMOOTH BAR DRAINING ---
        slider.maxValue = maxTime;

        // Instead of setting slider.value immediately, we smoothly slide towards the true time left
        slider.value = Mathf.Lerp(slider.value, timeLeft, Time.deltaTime * barDrainSpeed);

        // --- EFFECT 3: COLOR FLASH ALERT ---
        if (sliderFillImage != null)
        {
            if (timeLeft <= criticalTimeThreshold)
            {
                // Pulse back and forth using a ping-pong sine wave
                float t = Mathf.PingPong(Time.time * flashSpeed, 1f);
                sliderFillImage.color = Color.Lerp(normalColor, flashColor, t);
            }
            else
            {
                // Revert to the normal color if time is safely topped off
                sliderFillImage.color = normalColor;
            }
        }
    }
}