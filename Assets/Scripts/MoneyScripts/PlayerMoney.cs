using System;

public static class PlayerMoney
{
    public static float Balance { get; private set; }
    public static int Amber { get; private set; }

    public static event Action<float> OnChanged;
    public static event Action<int> OnAmberChanged;

    private static bool initialized;

    public static void EnsureInitialized(float startingMoney, int startingAmber)
    {
        if (initialized) return;
        initialized = true;
        Balance += startingMoney;
        Amber += startingAmber;
    }

    public static void Add(float amount)
    {
        Balance += amount;
        OnChanged?.Invoke(Balance);
    }

    public static bool TrySpend(float amount)
    {
        if (Balance < amount) return false;
        Balance -= amount;
        OnChanged?.Invoke(Balance);
        return true;
    }

    public static void AddAmber(int amount)
    {
        Amber += amount;
        OnAmberChanged?.Invoke(Amber);
    }

    public static bool TrySpendAmber(int amount)
    {
        if (Amber < amount) return false;
        Amber -= amount;
        OnAmberChanged?.Invoke(Amber);
        return true;
    }
}