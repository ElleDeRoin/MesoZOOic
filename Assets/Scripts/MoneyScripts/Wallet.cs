using System;
using UnityEngine;

public class Wallet : MonoBehaviour
{
    public static Wallet Instance { get; private set; }

    [SerializeField] private float startingMoney = 0f;
    [SerializeField] private int startingAmber = 0;

    public float Money { get; private set; }
    public int Amber { get; private set; }

    public event Action<float> Changed;
    public event Action<int> AmberChanged;

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
        Amber = startingAmber;
        Changed?.Invoke(Money);
        AmberChanged?.Invoke(Amber);
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

    public bool CanAffordAmber(int amount) => Amber >= amount;

    public void AddAmber(int amount)
    {
        Amber += amount;
        AmberChanged?.Invoke(Amber);
    }

    public bool TrySpendAmber(int amount)
    {
        if (Amber < amount) return false;

        Amber -= amount;
        AmberChanged?.Invoke(Amber);
        return true;
    }
}