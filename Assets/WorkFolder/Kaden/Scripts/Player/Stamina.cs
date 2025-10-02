using UnityEngine;
using UnityEngine.UI;

public class Stamina : MonoBehaviour
{
    [Header("Stamina Settings")]
    public float maxStamina = 1f; // Max stamina value 
    public float regenRate = 0.2f; // How fast stamina fills per second

    [Header("UI")]
    public Slider staminaSlider;

    private float currentStamina;

    void Start()
    {
        currentStamina = maxStamina; // Start Full
        staminaSlider.minValue = 0f;
        staminaSlider.maxValue = maxStamina;
        staminaSlider.value = currentStamina;
    }

    void Update()
    {
        // Gradually fill the stamina over time
        if (currentStamina < maxStamina)
        {
            currentStamina += regenRate * Time.deltaTime;
            currentStamina = Mathf.Clamp(currentStamina, 0f, maxStamina);
        }

        // Update UI
        staminaSlider.value = currentStamina;
    }

    public void DrainToZero()
    {
        currentStamina = 0f;
    }

    public bool IsFull()
    {
        // Allows a tiny tolerance to avoid getting stuck at 0.9999
        return currentStamina >= maxStamina - 0.01f;
    }
}
