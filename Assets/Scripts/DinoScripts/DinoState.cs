using System;
using UnityEngine;

[Serializable]
public class DinoState
{
    public Diet diet;
    public float happiness = 100f;
    public float hunger = 100f;

    public float happinessDecayRate;
    public float hungerDecayRate;
    public float hungerImpactOnHappiness;
    public float baseMoneyPerSecond;
    public float hungerMoneyFloor;

    public float decorationBonus;
    public float currentMoneyRate;

    public void Tick(float dt)
    {
        hunger = Mathf.Clamp(hunger - hungerDecayRate * dt, 0f, 100f);

        float hungerSeverity = 1f - (hunger / 100f);
        float extraDrain = hungerImpactOnHappiness * hungerSeverity;
        happiness = Mathf.Clamp(happiness + (decorationBonus - happinessDecayRate - extraDrain) * dt, 0f, 100f);

        float happinessFactor = happiness / 100f;
        float hungerFactor = Mathf.Lerp(hungerMoneyFloor, 1f, hunger / 100f);
        currentMoneyRate = baseMoneyPerSecond * happinessFactor * hungerFactor;

        if (Wallet.Instance != null)
            Wallet.Instance.Add(currentMoneyRate * dt);
    }
}