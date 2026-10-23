using System;
using UnityEngine;

public class Wallet : MonoBehaviour
{
    public static Wallet Instance { get; private set; }

    [SerializeField] private float startingMoney = 100f;
    [SerializeField] private int startingAmber = 0;

    public float Money => PlayerMoney.Balance;
    public int Amber => PlayerMoney.Amber;
    public float StartingMoney => startingMoney;
    public int StartingAmber => startingAmber;

    public event Action<float> Changed;
    public event Action<int> AmberChanged;

    private void Awake()
    {
        Instance = this;
        PlayerMoney.EnsureInitialized(startingMoney, startingAmber);
    }

    private void OnEnable()
    {
        PlayerMoney.OnChanged += HandleMoneyChanged;
        PlayerMoney.OnAmberChanged += HandleAmberChanged;
    }

    private void OnDisable()
    {
        PlayerMoney.OnChanged -= HandleMoneyChanged;
        PlayerMoney.OnAmberChanged -= HandleAmberChanged;
    }

    private void Start()
    {
        Changed?.Invoke(Money);
        AmberChanged?.Invoke(Amber);
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    private void HandleMoneyChanged(float value) => Changed?.Invoke(value);
    private void HandleAmberChanged(int value) => AmberChanged?.Invoke(value);

    public bool CanAfford(float amount) => Money >= amount;
    public void Add(float amount) => PlayerMoney.Add(amount);
    public bool TrySpend(float amount) => PlayerMoney.TrySpend(amount);

    public bool CanAffordAmber(int amount) => Amber >= amount;
    public void AddAmber(int amount) => PlayerMoney.AddAmber(amount);
    public bool TrySpendAmber(int amount) => PlayerMoney.TrySpendAmber(amount);
}