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
    private bool bonusesDirty = true;

    private void OnEnable()
    {
        DecorationStore.Changed += MarkBonusesDirty;
    }

    private void OnDisable()
    {
        DecorationStore.Changed -= MarkBonusesDirty;
    }

    private void MarkBonusesDirty() => bonusesDirty = true;

    public DinoState GetOrCreate(string id, System.Func<DinoState> factory)
    {
        if (!states.TryGetValue(id, out var state))
        {
            state = factory();
            states[id] = state;
            bonusesDirty = true;
        }
        return state;
    }

    private void Update()
    {
        if (bonusesDirty)
        {
            RecalculateDecorationBonuses();
            bonusesDirty = false;
        }

        float dt = Time.deltaTime;
        foreach (var state in states.Values)
            state.Tick(dt);
    }

    private void RecalculateDecorationBonuses()
    {
        foreach (var pair in states)
        {
            DinoState state = pair.Value;
            float bonus = 0f;

            foreach (var deco in DecorationStore.GetPlacedFor(pair.Key))
            {
                float multiplier = state.data != null
                    ? state.data.GetDecorationMultiplier(deco)
                    : 1f;

                bonus += deco.happinessPerSecond * multiplier;
            }

            state.decorationBonus = bonus;
        }
    }

    public void Register(string id, DinoState state)
    {
        states[id] = state;
        bonusesDirty = true;
    }

    public bool TryGet(string id, out DinoState state) => states.TryGetValue(id, out state);
}