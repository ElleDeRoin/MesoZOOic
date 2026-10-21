using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Dino/Gacha Odds", fileName = "GachaOdds")]
public class GachaOdds : ScriptableObject
{
    [Serializable]
    public class Weights
    {
        [Tooltip("Base Odds")]
        public float common = 70f;
        public float uncommon = 20f;
        public float rare = 8f;
        public float legendary = 2f;

        public float Get(Rarity rarity)
        {
            switch (rarity)
            {
                case Rarity.Common: return common;
                case Rarity.Uncommon: return uncommon;
                case Rarity.Rare: return rare;
                case Rarity.Legendary: return legendary;
                default: return 0f;
            }
        }
    }

    [Header("Free gacha")]
    public Weights regular = new Weights();

    [Header("Paid gacha")]
    public Weights paid = new Weights
    {
        common = 40f,
        uncommon = 30f,
        rare = 20f,
        legendary = 10f
    };

    public Weights GetWeights(bool isPaid) => isPaid ? paid : regular;
}