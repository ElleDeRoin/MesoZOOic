using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class DecorationSlot : MonoBehaviour
{
    private static readonly List<DecorationSlot> All = new List<DecorationSlot>();

    [SerializeField] private PropSize size = PropSize.Large;
    [SerializeField] private Renderer marker;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private bool inheritRotation = true;

    private Collider col;
    private GameObject instance;

    public PropSize Size => size;
    public DecorationData Current { get; private set; }

    public static float TotalHappinessPerSecond
    {
        get
        {
            float total = 0f;
            foreach (var slot in All)
                if (slot.Current != null) total += slot.Current.happinessPerSecond;
            return total;
        }
    }
    public bool IsEmpty => Current == null;

    private void Awake()
    {
        col = GetComponent<Collider>();
        if (marker == null) marker = GetComponent<Renderer>();
        SetMarkerVisible(false);
    }

    private void OnEnable() => All.Add(this);
    private void OnDisable() => All.Remove(this);

    public bool CanAccept(DecorationData data) => data != null && IsEmpty && data.size == size;

    public bool Place(DecorationData data)
    {
        if (!CanAccept(data)) return false;

        Vector3 pos = spawnPoint != null
            ? spawnPoint.position
            : new Vector3(col.bounds.center.x, col.bounds.min.y, col.bounds.center.z);
        Quaternion rot = inheritRotation ? transform.rotation : Quaternion.identity;

        instance = Instantiate(data.prefab, pos, rot);
        Current = data;
        return true;
    }

    public void Clear()
    {
        if (instance != null) Destroy(instance);
        instance = null;
        Current = null;
    }

    public void SetMarkerVisible(bool visible)
    {
        if (marker != null) marker.enabled = visible; 
    }

    public static void ShowMarkersFor(DecorationData data)
    {
        foreach (var s in All) s.SetMarkerVisible(s.CanAccept(data));
    }

    public static void HideAllMarkers() => ShowMarkersFor(null);
}