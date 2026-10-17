
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public enum Diet { Herbivore, Carnivore, Omnivore }
public enum FeedResult { Ate, WrongDiet, Full }

public class DinoStats : MonoBehaviour
{
    public static readonly List<DinoStats> All = new List<DinoStats>();

    public Slider happinessSlider;
    public Slider hungerSlider;

    [Header("Diet")]
    public Diet diet = Diet.Herbivore;
    public float fullThreshold = 99f;
    public float wrongFoodHappinessPenalty = 5f;

    [Header("Animations")]
    public UnityEvent onAte;
    public UnityEvent onRejectedFood;

    private DinoState state;
    private string instanceId;

    public float happiness => state != null ? state.happiness : 0f;
    public float hunger => state != null ? state.hunger : 0f;
    public float currentMoneyRate =>
        state != null ? state.currentMoneyRate : 0f;

    private void OnEnable()
    {
        if (!All.Contains(this))
            All.Add(this);

        DecorationSlot.HappinessGenerated += OnDecorationHappiness;
    }

    private void OnDisable()
    {
        All.Remove(this);
        DecorationSlot.HappinessGenerated -= OnDecorationHappiness;
    }

    private void Update()
    {
        if (state == null) return;

        UpdateUI();
    }

    private void OnDecorationHappiness(float amount)
    {
        if (state == null) return;

        if (GameSession.SelectedDino == null) return;

        if (GameSession.SelectedDino.instanceId != instanceId)
            return;

        Play(amount);
    }

    private void UpdateUI()
    {
        if (happinessSlider != null)
            happinessSlider.value = state.happiness;

        if (hungerSlider != null)
            hungerSlider.value = state.hunger;
    }

    public void Initialize(string instanceId, DinoData data)
    {
        this.instanceId = instanceId;

        if (data != null)
            diet = data.diet;

        if (DinoRegistry.Instance != null)
            DinoRegistry.Instance.TryGet(instanceId, out state);

        UpdateUI();
    }

    public bool CanEat(FoodType type)
    {
        switch (diet)
        {
            case Diet.Herbivore:
                return type == FoodType.Veggie;

            case Diet.Carnivore:
                return type == FoodType.Meat;

            case Diet.Omnivore:
            default:
                return true;
        }
    }

    public FeedResult TryFeed(FoodData food)
    {
        if (state == null || food == null)
            return FeedResult.Full;

        if (!CanEat(food.type))
        {
            state.happiness = Mathf.Clamp(
                state.happiness - wrongFoodHappinessPenalty,
                0f,
                100f
            );

            onRejectedFood?.Invoke();
            UpdateUI();

            return FeedResult.WrongDiet;
        }

        if (state.hunger >= fullThreshold)
        {
            onRejectedFood?.Invoke();
            return FeedResult.Full;
        }

        state.hunger = Mathf.Clamp(
            state.hunger + food.hungerRestored,
            0f,
            100f
        );

        state.happiness = Mathf.Clamp(
            state.happiness + food.happinessBonus,
            0f,
            100f
        );

        onAte?.Invoke();
        UpdateUI();

        return FeedResult.Ate;
    }

    public void Play(float amount)
    {
        if (state == null) return;

        state.happiness = Mathf.Clamp(
            state.happiness + amount,
            0f,
            100f
        );

        UpdateUI();
    }
}
