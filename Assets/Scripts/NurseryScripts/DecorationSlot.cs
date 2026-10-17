
using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class DecorationSlot : MonoBehaviour
{
    private static readonly List<DecorationSlot> All =
        new List<DecorationSlot>();

    public static event Action<float> HappinessGenerated;

    [SerializeField] private PropSize size = PropSize.Large;
    [SerializeField] private Renderer marker;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private bool inheritRotation = true;

    [Header("Happiness Ticks")]
    [SerializeField] private float minTickInterval = 3f;
    [SerializeField] private float maxTickInterval = 10f;

    [Tooltip("Chance to add happiness")]
    [SerializeField] private float chancePerHappiness = 2f;

    private Collider col;
    private GameObject instance;
    private float tickTimer;
    private float currentInterval;

    public PropSize Size => size;
    public DecorationData Current { get; private set; }

    public static float TotalHappinessPerSecond
    {
        get
        {
            float total = 0f;

            foreach (var slot in All)
            {
                if (slot != null && slot.Current != null)
                    total += slot.Current.happinessPerSecond;
            }

            return total;
        }
    }

    public bool IsEmpty => Current == null;

    private string StoreKey
    {
        get
        {
            string dinoId =
                GameSession.SelectedDino != null
                    ? GameSession.SelectedDino.instanceId
                    : "none";

            return dinoId + "/" + name;
        }
    }

    private void Awake()
    {
        col = GetComponent<Collider>();

        if (marker == null)
            marker = GetComponent<Renderer>();

        SetMarkerVisible(false);
    }

    private void OnEnable()
    {
        if (!All.Contains(this))
            All.Add(this);
    }

    private void OnDisable()
    {
        All.Remove(this);
    }

    private void Start()
    {
        if (DecorationStore.TryGet(StoreKey, out var saved))
            PlaceInternal(saved);
    }

    private void Update()
    {
        if (Current == null) return;

        tickTimer += Time.deltaTime;

        if (tickTimer < currentInterval)
            return;

        float interval = currentInterval;

        RollNextInterval();

        float hps = Current.happinessPerSecond;
        float chance = Mathf.Clamp01(hps * chancePerHappiness);

        if (chance <= 0f || UnityEngine.Random.value > chance)
            return;

        float amount = hps * interval / chance;

        HappinessGenerated?.Invoke(amount);
    }

    private void RollNextInterval()
    {
        tickTimer = 0f;

        currentInterval = UnityEngine.Random.Range(
            minTickInterval,
            maxTickInterval
        );
    }

    public bool CanAccept(DecorationData data)
    {
        return data != null && IsEmpty && data.size == size;
    }

    public bool Place(DecorationData data)
    {
        if (!PlaceInternal(data))
            return false;

        DecorationStore.Set(StoreKey, data);

        return true;
    }

    private bool PlaceInternal(DecorationData data)
    {
        if (!CanAccept(data))
            return false;

        Vector3 pos = spawnPoint != null
            ? spawnPoint.position
            : new Vector3(
                col.bounds.center.x,
                col.bounds.min.y,
                col.bounds.center.z
            );

        Quaternion rot = inheritRotation
            ? transform.rotation
            : Quaternion.identity;

        if (data.prefab == null)
        {
            Debug.LogWarning(
                $"Decoration '{data.name}' has no prefab assigned.",
                this
            );

            return false;
        }

        instance = Instantiate(data.prefab, pos, rot);
        Current = data;

        RollNextInterval();

        return true;
    }

    public void Clear()
    {
        if (instance != null)
            Destroy(instance);

        instance = null;
        Current = null;

        DecorationStore.Remove(StoreKey);

        tickTimer = 0f;
        currentInterval = 0f;
    }

    public void SetMarkerVisible(bool visible)
    {
        if (marker != null)
            marker.enabled = visible;
    }

    public static void ShowMarkersFor(DecorationData data)
    {
        foreach (var slot in All)
        {
            if (slot != null)
                slot.SetMarkerVisible(slot.CanAccept(data));
        }
    }

    public static void HideAllMarkers()
    {
        ShowMarkersFor(null);
    }
}
