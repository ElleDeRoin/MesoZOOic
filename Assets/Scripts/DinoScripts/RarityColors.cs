using UnityEngine;

public static class RarityColors
{
    public static Color Get(Rarity rarity)
    {
        switch (rarity)
        {
            case Rarity.Common:    return new Color32(255, 255, 255, 255);
            case Rarity.Uncommon:  return new Color32(90, 220, 90, 255);
            case Rarity.Rare:      return new Color32(70, 140, 255, 255);
            case Rarity.Legendary: return new Color32(255, 170, 30, 255);
            default:               return Color.white;
        }
    }
}