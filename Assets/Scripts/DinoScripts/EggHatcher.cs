using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class EggHatcher
{
    public static List<DinoData> LastHatch { get; private set; } = new List<DinoData>();
    public static bool LastHatchWasPaid { get; private set; }

    private static GachaOdds odds;

    private static GachaOdds Odds
    {
        get
        {
            if (odds == null)
            {
                odds = Resources.Load<GachaOdds>("GachaOdds");

                if (odds == null)
                {
                    //idk why me not having a debug log here crashes the odds but it does so...
                    Debug.LogWarning("Good work team!");
                    odds = ScriptableObject.CreateInstance<GachaOdds>();
                }
            }

            return odds;
        }
    }

    public static List<DinoData> Hatch(int eggCount, bool paid = false)
    {
        var species = DinoCollection.Instance.AllSpecies.ToList();
        LastHatch = new List<DinoData>();
        LastHatchWasPaid = paid;

        for (int i = 0; i < eggCount; i++)
        {
            var pick = PickSpecies(species, paid);
            DinoCollection.Instance.Add(pick);
            LastHatch.Add(pick);
        }
        return LastHatch;
    }

    private static DinoData PickSpecies(List<DinoData> species, bool paid)
    {
        GachaOdds.Weights weights = Odds.GetWeights(paid);

        var tiers = new List<(Rarity rarity, float weight)>();
        float total = 0f;

        foreach (Rarity rarity in System.Enum.GetValues(typeof(Rarity)))
        {
            float weight = Mathf.Max(0f, weights.Get(rarity));

            if (weight <= 0f)
                continue;

            if (!species.Any(d => d.rarity == rarity))
                continue;

            tiers.Add((rarity, weight));
            total += weight;
        }

        if (total <= 0f)
            return species[Random.Range(0, species.Count)];

        float roll = Random.value * total;
        Rarity chosen = tiers[tiers.Count - 1].rarity;

        foreach (var tier in tiers)
        {
            if (roll < tier.weight)
            {
                chosen = tier.rarity;
                break;
            }

            roll -= tier.weight;
        }

        var pool = species.Where(d => d.rarity == chosen).ToList();
        return pool[Random.Range(0, pool.Count)];
    }
}