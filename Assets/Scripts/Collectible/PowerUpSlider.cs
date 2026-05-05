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
    public GameObject sliderRoot;
    public Slider slider;

    [Header("Powerup")]
    public PowerupType powerupType = PowerupType.Speed;

    [Header("Settings")]
    public bool hideWhenInactive = true;

    private void Start()
    {
        if (playerMovement == null)
            playerMovement = FindFirstObjectByType<NewThirdPlayerMovement>();

        if (sliderRoot == null && slider != null)
            sliderRoot = slider.gameObject;
    }

    private void Update()
    {
        if (playerMovement == null || slider == null) return;

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

        bool active = timeLeft > 0f && maxTime > 0f;

        if (sliderRoot != null && hideWhenInactive)
            sliderRoot.SetActive(active);

        if (!active)
        {
            slider.value = 0f;
            return;
        }

        slider.maxValue = maxTime;

        // If timeLeft is exactly maxTime, ensure the sldie stays at full
        if (timeLeft >= maxTime)
        {
            slider.value = maxTime;
        }
        else
        {
            slider.value = timeLeft;
        }
    }
}