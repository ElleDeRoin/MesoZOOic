using System.Collections.Generic;
using UnityEngine;

public enum Preference { Neutral, Favorite, Disliked }
public enum Rarity { Common, Uncommon, Rare, Legendary }

[CreateAssetMenu(menuName = "Dino/Dino Data")]
public class DinoData : ScriptableObject
{
    [Header("Dino")]
    public string id;
    public string displayName;
    public Diet diet;
    public Rarity rarity;
    public Sprite icon;
    public GameObject penPrefab;

    [Header("Stats")]
    public float happinessDecayRate = 0.5f;
    public float hungerDecayRate = 0.75f;
    public float hungerImpactOnHappiness = 1f;
    public float baseMoneyPerSecond = 1f;
    [Range(0f, 1f)] public float hungerMoneyFloor = 0.7f;

    [Header("Food Preferences")]
    public List<FoodData> favoriteFoods = new List<FoodData>();
    public List<FoodData> dislikedFoods = new List<FoodData>();
    public float favoriteFoodHungerMultiplier = 1.5f;
    public float favoriteFoodExtraHappiness = 10f;
    public float dislikedFoodHungerMultiplier = 0.5f;
    public float dislikedFoodHappinessPenalty = 5f;

    [Header("Decoration Preferences")]
    public List<DecorationData> favoriteDecorations = new List<DecorationData>();
    public List<DecorationData> dislikedDecorations = new List<DecorationData>();
    public float favoriteDecorationMultiplier = 2f;
   [Tooltip("Y'all can set this to a number below 1 for reduced bennefit and to a negative number if you want it to actively take away happiness")]
    public float dislikedDecorationMultiplier = 0.25f;

    public DinoState CreateState() => new DinoState
    {
        diet = diet,
        happiness = 100f,
        hunger = 100f,
        happinessDecayRate = happinessDecayRate,
        hungerDecayRate = hungerDecayRate,
        hungerImpactOnHappiness = hungerImpactOnHappiness,
        baseMoneyPerSecond = baseMoneyPerSecond,
        hungerMoneyFloor = hungerMoneyFloor,
        data = this
    };
    
    public Preference GetPreference(FoodData food)
    {
        if (food == null) return Preference.Neutral;
        if (favoriteFoods.Contains(food)) return Preference.Favorite;
        if (dislikedFoods.Contains(food)) return Preference.Disliked;
        return Preference.Neutral;
    }

    public float GetDecorationMultiplier(DecorationData deco)
    {
        switch (GetPreference(deco))
        {
            case Preference.Favorite: return favoriteDecorationMultiplier;
            case Preference.Disliked: return dislikedDecorationMultiplier;
            default: return 1f;
        }
    }

    public Preference GetPreference(DecorationData deco)
    {
        if (deco == null) return Preference.Neutral;
        if (favoriteDecorations.Contains(deco)) return Preference.Favorite;
        if (dislikedDecorations.Contains(deco)) return Preference.Disliked;
        return Preference.Neutral;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (string.IsNullOrEmpty(id)) id = name.ToLowerInvariant();
    }
#endif
}