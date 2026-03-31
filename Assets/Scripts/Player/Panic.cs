using UnityEngine;
using UnityEngine.UI;

public class Panic : MonoBehaviour
{
    [Header("Panic Settings")]
    public float maxSanity = 1f; // Max sanity value 
    public float regenRate = 0.2f; // How fast sanity fills per second

    [Header("UI")]
    public Slider sanitySlider;

    private float currentSanity;

    void Start()
    {
        currentSanity = 0f; // Start Empty

        if (sanitySlider != null)
        {
           sanitySlider.minValue = 0f;
            sanitySlider.maxValue = maxSanity;
            sanitySlider.value = currentSanity; 
        }
        
    }

    void Update()
    {
        // Gradually fill the sanity over time
        if (currentSanity < maxSanity)
        {
            currentSanity += regenRate * Time.deltaTime;

            currentSanity = Mathf.Clamp(currentSanity, 0f, maxSanity);
        }

        // Update UI
        if (sanitySlider != null)
        {
            sanitySlider.value = currentSanity;
        }
    }

    public void DrainToZero()
    {
        currentSanity = 0f;
    }

    public bool IsFull()
    {
        return currentSanity >= maxSanity;
    }
}
