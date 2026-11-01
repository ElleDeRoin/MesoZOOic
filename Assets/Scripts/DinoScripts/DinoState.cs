using System;
using UnityEngine;

[Serializable]
public class DinoState
{
    public const float MaxCleanliness = 100f;
    public const float CleanThreshold = MaxCleanliness * 2f / 3f;
    public const float DirtyThreshold = MaxCleanliness / 3f;

    public Diet diet;
    public float happiness = 100f;
    public float hunger = 100f;
    public float cleanliness = MaxCleanliness;
    public bool isDirty;
    public float dirtyCheckTimer;

    public float happinessDecayRate;
    public float hungerDecayRate;
    public float hungerImpactOnHappiness;
    public float baseMoneyPerSecond;
    public float hungerMoneyFloor;

    public float dirtyCheckInterval;
    public float dirtyChance;
    public float cleanlinessDecayRate;
    public float cleanHappinessPerSecond;
    public float dirtyHappinessPerSecond;

    public float decorationBonus;
    public float currentMoneyRate;

    [NonSerialized] public DinoData data;

    public float CleanlinessHappinessRate
    {
        get
        {
            if (cleanliness >= CleanThreshold) return cleanHappinessPerSecond;
            if (cleanliness >= DirtyThreshold) return 0f;
            return -dirtyHappinessPerSecond;
        }
    }

    public void Clean(float amount)
    {
        cleanliness = Mathf.Clamp(cleanliness + amount, 0f, MaxCleanliness);

        if (cleanliness >= MaxCleanliness)
        {
            isDirty = false;
            dirtyCheckTimer = 0f;
        }
    }

    private void UpdateDirtiness(float dt)
    {
        if (isDirty)
        {
            cleanliness = Mathf.Clamp(cleanliness - cleanlinessDecayRate * dt, 0f, MaxCleanliness);
            return;
        }

        if (dirtyCheckInterval <= 0f) return;

        dirtyCheckTimer += dt;
        if (dirtyCheckTimer >= dirtyCheckInterval)
        {
            dirtyCheckTimer = 0f;
            if (UnityEngine.Random.value < dirtyChance)
                isDirty = true;
        }
    }

    public void Tick(float dt)
    {
        UpdateDirtiness(dt);

        hunger = Mathf.Clamp(hunger - hungerDecayRate * dt, 0f, 100f);

        float hungerSeverity = 1f - (hunger / 100f);
        float extraDrain = hungerImpactOnHappiness * hungerSeverity;
        happiness = Mathf.Clamp(happiness + (decorationBonus + CleanlinessHappinessRate - happinessDecayRate - extraDrain) * dt, 0f, 100f);

        float happinessFactor = happiness / 100f;
        float hungerFactor = Mathf.Lerp(hungerMoneyFloor, 1f, hunger / 100f);
        currentMoneyRate = baseMoneyPerSecond * happinessFactor * hungerFactor;

        PlayerMoney.Add(currentMoneyRate * dt);
    }
}