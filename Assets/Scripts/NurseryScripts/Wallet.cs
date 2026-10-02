using System;
using TMPro;
using UnityEngine;

public class Wallet : MonoBehaviour
{
    public static Wallet Instance { get; private set; }

    [SerializeField] private TMP_Text moneyText;
    [SerializeField] private float startingMoney = 0f;

    public float Money { get; private set; }
    public event Action<float> Changed;

    private void Awake()
    {
        Instance = this;
        Money = startingMoney;
        Refresh();
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    public bool CanAfford(float amount) => Money >= amount;

    public void Add(float amount)
    {
        Money += amount;
        Refresh();
    }

    public bool TrySpend(float amount)
    {
        if (Money < amount) return false;
        Money -= amount;
        Refresh();
        return true;
    }

    private void Refresh()
    {
        if (moneyText != null)
            moneyText.text = "$" + Mathf.FloorToInt(Money).ToString("D4");
        Changed?.Invoke(Money);
    }
}