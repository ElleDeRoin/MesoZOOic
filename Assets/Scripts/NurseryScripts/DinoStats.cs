using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DinoStats : MonoBehaviour
{
    [Header("UI References")]

    public Slider happinessSlider;

    public Slider hungerSlider;

    public TMPro.TMP_Text moneyText;

    [Header("Stat Ranges")]
    [Range(0f, 100f)] public float happiness = 100f;

    [Range(0f, 100f)] public float hunger = 100f;

    [Header("Decay Rates (points per second)")]
    public float happinessDecayRate = 0.5f;

    public float hungerDecayRate = 0.75f;

    public float hungerImpactOnHappiness = 1.0f;

    [Header("Money Generation")]
    public float baseMoneyPerSecond = 1.0f;

    [Range(0f, 1f)] public float hungerMoneyFloor = 0.7f;

    [Header("Runtime/Debugging")]
    public float money = 0f;
    [Tooltip("Current money earned per second, for debugging")]
    public float currentMoneyRate = 0f;

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
        hunger -= hungerDecayRate * dt;
        hunger = Mathf.Clamp(hunger, 0f, 100f);
    }

    private void TickHappiness(float dt)
    {
        float hungerSeverity = 1f - (hunger / 100f);

        float extraDrain = hungerImpactOnHappiness * hungerSeverity;

        happiness -= (happinessDecayRate + extraDrain) * dt;
        happiness = Mathf.Clamp(happiness, 0f, 100f);
    }

    private void TickMoney(float dt)
    {
        float happinessFactor = happiness / 100f;
        float hungerFactor = Mathf.Lerp(hungerMoneyFloor, 1f, hunger / 100f);

        currentMoneyRate = baseMoneyPerSecond * happinessFactor * hungerFactor;
        money += currentMoneyRate * dt;
    }

    private void UpdateUI()
    {
        if (happinessSlider != null) happinessSlider.value = happiness;
        if (hungerSlider != null) hungerSlider.value = hunger;
        if (moneyText != null) moneyText.text = "$" + Mathf.FloorToInt(money).ToString("D4");
    }


    public void Feed(float amount)
    {
        hunger = Mathf.Clamp(hunger + amount, 0f, 100f);
    }

    public void Play(float amount)
    {
        happiness = Mathf.Clamp(happiness + amount, 0f, 100f);
    }

    public bool TrySpend(float amount)
    {
        if (money < amount) return false;
        money -= amount;
        return true;
    }
}