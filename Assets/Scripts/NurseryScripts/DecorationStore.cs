using System.Collections.Generic;

public static class DecorationStore
{
    private static readonly Dictionary<string, DecorationData> placed = new Dictionary<string, DecorationData>();

    public static void Set(string key, DecorationData data) => placed[key] = data;
    public static void Remove(string key) => placed.Remove(key);
    public static bool TryGet(string key, out DecorationData data) => placed.TryGetValue(key, out data);
}