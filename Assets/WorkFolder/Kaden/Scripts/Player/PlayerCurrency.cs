using UnityEngine;
using System;

public class PlayerCurrency : MonoBehaviour
{
    public int pigment = 0;
    public event Action<int> OnPigmentChanged;

    public void AddPigment(int amount)
    {
        pigment = Mathf.Max(0, pigment + amount);
        OnPigmentChanged?.Invoke(pigment);
    }
}