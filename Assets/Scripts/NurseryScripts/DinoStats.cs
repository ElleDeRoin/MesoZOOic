using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public enum Diet { Herbivore, Carnivore, Omnivore }
public enum FeedResult { Ate, WrongDiet, Full }

public class DinoStats : MonoBehaviour
{
    public static readonly List<DinoStats> All = new List<DinoStats>();

    private void OnEnable() => All.Add(this);
    private void OnDisable() => All.Remove(this);

    public Slider happinessSlider;
    public Slider hungerSlider;

    [Header("Diet")]
    public Diet diet = Diet.Herbivore;
    public float fullThreshold = 99f;
    public float wrongFoodHappinessPenalty = 5f;

    [Header("Stat Ranges")]
    [Range(0f, 100f)] public float happiness = 100f;
    [Range(0f, 100f)] public float hunger = 100f;
    public float happinessDecayRate = 0.5f;
    public float hungerDecayRate = 0.75f;
    public float hungerImpactOnHappiness = 1.0f;

    [Header("Money Generation")]
    public float baseMoneyPerSecond = 1.0f;
    [Range(0f, 1f)] public float hungerMoneyFloor = 0.7f;
    public float currentMoneyRate = 0f;

    [Header("Animations")]
    public UnityEvent onAte;
    public UnityEvent onRejectedFood;

    void Update()
    {
        float dt = Time.deltaTime;
        TickHunger(dt);
        TickHappiness(dt);
        TickMoney(dt);
        UpdateUI();
    }

    private void TickHunger(float dt)
    {
        hunger = Mathf.Clamp(hunger - hungerDecayRate * dt, 0f, 100f);
    }

    private void TickHappiness(float dt)
    {
        float hungerSeverity = 1f - (hunger / 100f);
        float extraDrain = hungerImpactOnHappiness * hungerSeverity;
        float decorationBonus = DecorationSlot.TotalHappinessPerSecond;

        happiness = Mathf.Clamp(happiness + (decorationBonus - happinessDecayRate - extraDrain) * dt, 0f, 100f);
    }

    private void TickMoney(float dt)
    {
        float happinessFactor = happiness / 100f;
        float hungerFactor = Mathf.Lerp(hungerMoneyFloor, 1f, hunger / 100f);

        currentMoneyRate = baseMoneyPerSecond * happinessFactor * hungerFactor;
        if (Wallet.Instance != null) Wallet.Instance.Add(currentMoneyRate * dt);
    }

    private void UpdateUI()
    {
        if (happinessSlider != null) happinessSlider.value = happiness;
        if (hungerSlider != null) hungerSlider.value = hunger;
    }

    public bool CanEat(FoodType type)
    {
        switch (diet)
        {
            case Diet.Herbivore: return type == FoodType.Veggie;
            case Diet.Carnivore: return type == FoodType.Meat;
            default: return true;
        }
    }

    public FeedResult TryFeed(FoodData food)
    {
        if (!CanEat(food.type))
        {
            happiness = Mathf.Clamp(happiness - wrongFoodHappinessPenalty, 0f, 100f);
            onRejectedFood?.Invoke();
            return FeedResult.WrongDiet;
        }

        if (hunger >= fullThreshold)
        {
            onRejectedFood?.Invoke();
            return FeedResult.Full;
        }

        hunger = Mathf.Clamp(hunger + food.hungerRestored, 0f, 100f);
        happiness = Mathf.Clamp(happiness + food.happinessBonus, 0f, 100f);
        onAte?.Invoke();
        return FeedResult.Ate;
    }

    public void Play(float amount)
    {
        happiness = Mathf.Clamp(happiness + amount, 0f, 100f);
    }
}