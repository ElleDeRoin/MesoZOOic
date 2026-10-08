using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public enum Diet { Herbivore, Carnivore, Omnivore }
public enum FeedResult { Ate, WrongDiet, Full }

public class DinoStats : MonoBehaviour
{
    public static readonly List<DinoStats> All = new List<DinoStats>();
    [SerializeField] private string dinoId;

    public Slider happinessSlider;
    public Slider hungerSlider;

    [Header("Diet")]
    public Diet diet = Diet.Herbivore;
    public float fullThreshold = 99f;
    public float wrongFoodHappinessPenalty = 5f;

    [Header("Stat Rates")]
    [Range(0f, 100f)] public float initialHappiness = 100f;
    [Range(0f, 100f)] public float initialHunger = 100f;
    public float happinessDecayRate = 0.5f;
    public float hungerDecayRate = 0.75f;
    public float hungerImpactOnHappiness = 1.0f;

    [Header("Money Generation")]
    public float baseMoneyPerSecond = 1.0f;
    [Range(0f, 1f)] public float hungerMoneyFloor = 0.7f;

    [Header("Animations")]
    public UnityEvent onAte;
    public UnityEvent onRejectedFood;

    private DinoState state;

    public float happiness => state.happiness;
    public float hunger => state.hunger;
    public float currentMoneyRate => state.currentMoneyRate;

    private void OnEnable()
    {
        All.Add(this);

        state = DinoRegistry.Instance.GetOrCreate(dinoId, () => new DinoState
        {
            diet = diet,
            happiness = initialHappiness,
            hunger = initialHunger,
            happinessDecayRate = happinessDecayRate,
            hungerDecayRate = hungerDecayRate,
            hungerImpactOnHappiness = hungerImpactOnHappiness,
            baseMoneyPerSecond = baseMoneyPerSecond,
            hungerMoneyFloor = hungerMoneyFloor
        });
    }

    private void OnDisable() => All.Remove(this);

    private void Update()
    {
        state.decorationBonus = DecorationSlot.TotalHappinessPerSecond;
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (happinessSlider != null) happinessSlider.value = state.happiness;
        if (hungerSlider != null) hungerSlider.value = state.hunger;
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
            state.happiness = Mathf.Clamp(state.happiness - wrongFoodHappinessPenalty, 0f, 100f);
            onRejectedFood?.Invoke();
            return FeedResult.WrongDiet;
        }

        if (state.hunger >= fullThreshold)
        {
            onRejectedFood?.Invoke();
            return FeedResult.Full;
        }

        state.hunger = Mathf.Clamp(state.hunger + food.hungerRestored, 0f, 100f);
        state.happiness = Mathf.Clamp(state.happiness + food.happinessBonus, 0f, 100f);
        onAte?.Invoke();
        return FeedResult.Ate;
    }

    public void Play(float amount)
    {
        state.happiness = Mathf.Clamp(state.happiness + amount, 0f, 100f);
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (string.IsNullOrEmpty(dinoId) && gameObject.scene.IsValid())
            dinoId = System.Guid.NewGuid().ToString();
    }
#endif
}