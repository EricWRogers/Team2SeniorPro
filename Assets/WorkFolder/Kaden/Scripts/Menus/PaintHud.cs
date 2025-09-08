using UnityEngine;
using UnityEngine.UI;

public class PaintHud : MonoBehaviour
{
    public PaintResource playerPaint;
    public PlayerCurrency playerCurrency;

    public Slider paintBar;          
    public TMPro.TMP_Text pigmentTMP; 

    void Start()
    {
        if (!playerPaint) playerPaint = FindObjectOfType<PaintResource>();
        if (!playerCurrency) playerCurrency = FindObjectOfType<PlayerCurrency>();

        if (playerPaint)
        {
            playerPaint.OnPaintChanged += HandlePaintChanged;
            // initialize
            HandlePaintChanged(playerPaint.currentPaint, playerPaint.maxPaint);
        }
        if (playerCurrency)
        {
            playerCurrency.OnPigmentChanged += HandlePigmentChanged;
            HandlePigmentChanged(playerCurrency.pigment);
        }
    }
    void OnDestroy()
    {
        if (playerPaint) playerPaint.OnPaintChanged -= HandlePaintChanged;
        if (playerCurrency) playerCurrency.OnPigmentChanged -= HandlePigmentChanged;
    }

    void HandlePaintChanged(float cur, float max)
    {
        if (paintBar) paintBar.value = max > 0f ? cur / max : 0f;
    }

    void HandlePigmentChanged(int amount)
    {
        string s = $"Pigment: {amount}";
        if (pigmentTMP) pigmentTMP.text = s;
    }
}
