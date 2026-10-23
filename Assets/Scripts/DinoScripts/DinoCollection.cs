using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class OwnedDino
{
    public string instanceId;
    public string dataId;
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

    public OwnedDino Add(DinoData data)
    {
        var dino = new OwnedDino { instanceId = Guid.NewGuid().ToString(), dataId = data.id };
        owned.Add(dino);
        DinoRegistry.Instance.Register(dino.instanceId, data.CreateState());
        SaveSystem.Save();
        OnChanged?.Invoke();
        return dino;
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
            }

            DinoRegistry.Instance.Register(d.instanceId, state);
        }
    }

    public void ClearAll()
    {
        owned.Clear();
        OnChanged?.Invoke();
    }
}