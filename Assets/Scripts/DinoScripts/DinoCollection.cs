using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

[Serializable]
public class OwnedDino
{
    public string instanceId;
    public string dataId;
    public string nickname;
}
public class DinoCollection : MonoBehaviour
{

    private static DinoCollection instance;
    public static DinoCollection Instance
    {
        get
        {
            if (instance == null)
            {
                var go = new GameObject("DinoCollection");
                instance = go.AddComponent<DinoCollection>();
                DontDestroyOnLoad(go);
                instance.Load();
            }
            return instance;
        }
    }
    public const string StarterDinoId = "raptor";
    public event Action OnChanged;

    private readonly List<OwnedDino> owned = new List<OwnedDino>();
    private Dictionary<string, DinoData> catalog;
    public IReadOnlyList<OwnedDino> Owned => owned;
    public IReadOnlyCollection<DinoData> AllSpecies => Catalog.Values;

    private Dictionary<string, DinoData> Catalog
    {
        get
        {
            if (catalog == null)
                catalog = Resources.LoadAll<DinoData>("Dinos").ToDictionary(d => d.id);
            return catalog;
        }
    }


    public DinoData GetData(string dataId) => Catalog[dataId];

    public IEnumerable<OwnedDino> GetByDiet(Diet diet) =>
        owned.Where(o => GetData(o.dataId).diet == diet);

    public OwnedDino Add(DinoData data, string nickname = null)
    {
        var dino = new OwnedDino
        {
            instanceId = Guid.NewGuid().ToString(),
            dataId = data.id,
            nickname = nickname
        };

        owned.Add(dino);
        DinoRegistry.Instance.Register(dino.instanceId, data.CreateState());
        SaveSystem.Save();
        OnChanged?.Invoke();
        return dino;
    }
    public const int MaxNameLength = 16;

    public string GetDisplayName(OwnedDino dino)
    {
        if (dino == null) return "";

        if (!string.IsNullOrWhiteSpace(dino.nickname))
            return dino.nickname;

        return GetData(dino.dataId).displayName;
    }

    public static string CleanName(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return "";

        string clean = Regex.Replace(raw.Trim(), @"\s+", " ");
        return clean.Length > MaxNameLength ? clean.Substring(0, MaxNameLength).TrimEnd() : clean;
    }
    public bool Rename(OwnedDino dino, string newName)
    {
        if (dino == null) return false;

        OwnedDino target = owned.Find(o => o.instanceId == dino.instanceId);
        if (target == null) return false;

        string clean = CleanName(newName);
        target.nickname = clean;
        dino.nickname = clean;

        SaveSystem.Save();
        OnChanged?.Invoke();
        return true;
    }

    public bool Remove(OwnedDino dino)
    {
        if (dino == null) return false;

        int removed = owned.RemoveAll(o => o.instanceId == dino.instanceId);
        if (removed == 0) return false;

        DinoRegistry.Instance.Remove(dino.instanceId);
        SaveSystem.Save();
        OnChanged?.Invoke();
        return true;
    }

    private void Load()
    {
        SaveFile save = SaveSystem.Data;

        foreach (var d in save.dinos)
        {
            if (!Catalog.TryGetValue(d.dataId, out var species)) continue;

            owned.Add(d);
            
            DinoState state = species.CreateState();

            DinoStateSave saved = save.dinoStates.Find(s => s.instanceId == d.instanceId);
            if (saved != null)
            {
                state.happiness = saved.happiness;
                state.hunger = saved.hunger;
                state.cleanliness = saved.cleanliness;
                state.isDirty = saved.isDirty;
                state.dirtyCheckTimer = saved.dirtyCheckTimer;
            }

            DinoRegistry.Instance.Register(d.instanceId, state);
        }

        EnsureStarterDino();
    }


    public void EnsureStarterDino()
    {
        if (SaveSystem.StarterDinoGiven) return;

        if (!Catalog.TryGetValue(StarterDinoId, out DinoData starter))
        {
            Debug.LogError($"Coudn't find the starting raptor oops");
            return;
        }

        SaveSystem.StarterDinoGiven = true;
        Add(starter, "Dave");
    }

    public void ClearAll()
    {
        owned.Clear();
        OnChanged?.Invoke();
    }
}