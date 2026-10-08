using System.Collections.Generic;
using UnityEngine;

public class DinoRegistry : MonoBehaviour
{
    private static DinoRegistry instance;

    public static DinoRegistry Instance
    {
        get
        {
            if (instance == null)
            {
                var go = new GameObject("DinoRegistry");
                instance = go.AddComponent<DinoRegistry>();
                DontDestroyOnLoad(go);
            }
            return instance;
        }
    }

    private readonly Dictionary<string, DinoState> states = new Dictionary<string, DinoState>();

    public DinoState GetOrCreate(string id, System.Func<DinoState> factory)
    {
        if (!states.TryGetValue(id, out var state))
        {
            state = factory();
            states[id] = state;
        }
        return state;
    }

    private void Update()
    {
        float dt = Time.deltaTime;
        foreach (var state in states.Values)
            state.Tick(dt);
    }
}