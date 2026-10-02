using UnityEngine;

public enum FoodType { Veggie, Meat }

[CreateAssetMenu(menuName = "Zoo/Food", fileName = "NewFood")]
public class FoodData : ScriptableObject
{
    public string displayName;
    public Sprite icon;
    public FoodType type;


    public float hungerRestored = 25f;
    public float happinessBonus = 0f;

    public float price = 0f;
}