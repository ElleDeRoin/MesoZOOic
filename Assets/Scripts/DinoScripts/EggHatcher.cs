using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class EggHatcher
{
    public static List<DinoData> LastHatch { get; private set; } = new List<DinoData>();

    public static List<DinoData> Hatch(int eggCount)
    {
        var species = DinoCollection.Instance.AllSpecies.ToList();
        LastHatch = new List<DinoData>();

        for (int i = 0; i < eggCount; i++)
        {
            var pick = species[Random.Range(0, species.Count)];
            DinoCollection.Instance.Add(pick);
            LastHatch.Add(pick);
        }
        return LastHatch;
    }
}