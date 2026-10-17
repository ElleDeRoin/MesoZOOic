using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class EggHatcher
{
    public static List<DinoData> LastHatch { get; private set; } = new List<DinoData>();
    public static bool LastHatchWasPaid { get; private set; }

    public static List<DinoData> Hatch(int eggCount, bool paid = false)
    {
        var species = DinoCollection.Instance.AllSpecies.ToList();
        LastHatch = new List<DinoData>();
        LastHatchWasPaid = paid;

        for (int i = 0; i < eggCount; i++)
        {
            var pick = species[Random.Range(0, species.Count)];
            DinoCollection.Instance.Add(pick);
            LastHatch.Add(pick);
        }
        return LastHatch;
    }
}