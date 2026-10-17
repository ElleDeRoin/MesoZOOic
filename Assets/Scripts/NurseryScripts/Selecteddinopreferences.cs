public static class SelectedDinoPreferences
{
    public static DinoData GetData()
    {
        var owned = GameSession.SelectedDino;
        if (owned == null || DinoCollection.Instance == null)
            return null;

        return DinoCollection.Instance.GetData(owned.dataId);
    }

    public static Preference Get(FoodData food)
    {
        var data = GetData();
        return data != null ? data.GetPreference(food) : Preference.Neutral;
    }

    public static Preference Get(DecorationData deco)
    {
        var data = GetData();
        return data != null ? data.GetPreference(deco) : Preference.Neutral;
    }
}