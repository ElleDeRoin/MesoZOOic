using System;
using UnityEngine;

public class Wallet : MonoBehaviour
{
    public static Wallet Instance { get; private set; }

    [SerializeField] private float startingMoney = 0f;

    public float Money { get; private set; }
    public event Action<float> Changed;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        Money = startingMoney;
        Changed?.Invoke(Money);
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    public bool CanAfford(float amount) => Money >= amount;

    public void Add(float amount)
    {
        Money += amount;
        Changed?.Invoke(Money);
    }

    public bool TrySpend(float amount)
    {
        if (Money < amount) return false;

        Money -= amount;
        Changed?.Invoke(Money);

        return true;
    }
}