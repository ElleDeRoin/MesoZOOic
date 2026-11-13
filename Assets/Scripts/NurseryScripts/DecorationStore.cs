using System;
using System.Collections.Generic;

public static class DecorationStore
{
    private static readonly Dictionary<string, DecorationData> placed = new Dictionary<string, DecorationData>();
    
    public static IEnumerable<KeyValuePair<string, DecorationData>> GetAll() => placed;

    public static event Action Changed;

    public static void Set(string key, DecorationData data)
    {
        placed[key] = data;
        Changed?.Invoke();
    }

    public static void Remove(string key)
    {
        if (placed.Remove(key))
            Changed?.Invoke();
    }
    
    public static void ClearAll()
    {
        placed.Clear();
        Changed?.Invoke();
    }

    public static bool TryGet(string key, out DecorationData data) => placed.TryGetValue(key, out data);

    public static IEnumerable<DecorationData> GetPlacedFor(string dinoInstanceId)
    {
        string prefix = dinoInstanceId + "/";

        foreach (var pair in placed)
        {
            if (pair.Value != null && pair.Key.StartsWith(prefix, StringComparison.Ordinal))
                yield return pair.Value;
        }
    }
}