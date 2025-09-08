using UnityEngine;
using System;
using System.Collections;
public class PaintResource : MonoBehaviour
{
    public float maxPaint = 100f;
    public float currentPaint = 100f;

    public event Action<float,float> OnPaintChanged; // (current,max)
    public event Action OnPaintDepleted;
    public event Action<float> OnDamaged;  // amount
    public event Action<float> OnHealed;   // amount

    public LoseScreen loseScreen;

    bool _depletedRaised;

    void RaiseChanged() => OnPaintChanged?.Invoke(currentPaint, maxPaint);

    public void AddPaint(float amount)
    {
        if (amount <= 0f) return;
        currentPaint = Mathf.Clamp(currentPaint + amount, 0f, maxPaint);
        _depletedRaised = currentPaint <= 0f && _depletedRaised; 
        OnHealed?.Invoke(amount);
        RaiseChanged();
    }

    public void Damage(float amount)
    {
        if (amount <= 0f) return;
        currentPaint = Mathf.Clamp(currentPaint - amount, 0f, maxPaint);
        OnDamaged?.Invoke(amount);
        RaiseChanged();
        if (currentPaint <= 0f && !_depletedRaised)
        {
            _depletedRaised = true;
            OnPaintDepleted?.Invoke();
            loseScreen.GameOver();
        }
    }

    public bool TrySpend(float amount)
    {
        if (amount <= 0f) return true;

        if (currentPaint < amount)
        {
            
            currentPaint = 0f;
            RaiseChanged();
            if (!_depletedRaised) { _depletedRaised = true; OnPaintDepleted?.Invoke(); }
            return false;
        }

        currentPaint -= amount;
        RaiseChanged();
        return true;
    }
}