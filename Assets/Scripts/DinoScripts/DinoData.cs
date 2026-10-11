using UnityEngine;

[CreateAssetMenu(menuName = "Dino/Dino Data")]
public class DinoData : ScriptableObject
{
    [Header("Dino")]
    public string id;
    public string displayName;
    public Diet diet;
    public Sprite icon;
    public GameObject penPrefab;

    [Header("Stats")]
    public float happinessDecayRate = 0.5f;
    public float hungerDecayRate = 0.75f;
    public float hungerImpactOnHappiness = 1f;
    public float baseMoneyPerSecond = 1f;
    [Range(0f, 1f)] public float hungerMoneyFloor = 0.7f;

    public DinoState CreateState() => new DinoState
    {
        diet = diet,
        happiness = 100f,
        hunger = 100f,
        happinessDecayRate = happinessDecayRate,
        hungerDecayRate = hungerDecayRate,
        hungerImpactOnHappiness = hungerImpactOnHappiness,
        baseMoneyPerSecond = baseMoneyPerSecond,
        hungerMoneyFloor = hungerMoneyFloor
    };

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (string.IsNullOrEmpty(id)) id = name.ToLowerInvariant();
    }
#endif
}