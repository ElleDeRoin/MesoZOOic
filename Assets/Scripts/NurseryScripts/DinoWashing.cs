using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class DinoWashing : MonoBehaviour
{
    public static DinoWashing Active { get; private set; }

    public static bool IsWashing { get; set; }

    [Header("rooot")]
    [SerializeField] private Transform dinoRoot;

    [SerializeField] private GameObject[] dirtSpots;
    [SerializeField] private int minSpots = 2;
    [SerializeField] private float secondsToWashSpot = 0.6f;
    [Range(0.05f, 1f)]
    [SerializeField] private float shrinkTo = 0.4f;

    [Header("Bubbles 030")]
    [SerializeField] private Sprite[] bubbleSprites;
    [SerializeField] private int bubbleCount = 15;
    [SerializeField] private float bubbleLifetime = 1.5f;
    [SerializeField] private float bubbleRiseSpeed = 0.5f;
    [SerializeField] private float bubbleSway = 0.1f;
    [SerializeField] private Vector2 bubbleSizeRange = new Vector2(0.2f, 0.45f);
    [SerializeField] private int sortingOrder = 10;

    [Header("Shine!!!")]
    [SerializeField] private Sprite[] shineSprites;
    [SerializeField] private int shineCount = 6;
    [SerializeField] private float shineDuration = 0.8f;
    [SerializeField] private float shineMaxDelay = 0.6f;
    [SerializeField] private Vector2 shineSizeRange = new Vector2(0.2f, 0.4f);
    [SerializeField] private float shineSpin = 90f;

    [Header("Audio")]
    [SerializeField] private AudioSource cleanAudioSource;
    [SerializeField] private AudioClip fullyCleanClip;
    [Range(0f, 1f)]
    [SerializeField] private float fullyCleanVolume = 1f;

    [Header("Events")]
    public UnityEvent onWashed;

    private class Spot
    {
        public GameObject go;
        public Transform t;
        public Renderer[] renderers;
        public Vector3 baseScale;
        public float health = 1f;
        public bool visible;
    }

    private readonly List<Spot> spots = new List<Spot>();
    private readonly List<GameObject> bubbles = new List<GameObject>();

    private DinoStats dino;
    private bool synced;
    private bool wasDirty;
    private float lastCleanliness = DinoState.MaxCleanliness;
    private float decayAccum;

    private void Awake()
    {
        if (dinoRoot == null) dinoRoot = transform;
        if (cleanAudioSource == null) cleanAudioSource = GetComponent<AudioSource>();

        dino = GetComponent<DinoStats>();
        if (dino == null) dino = GetComponentInChildren<DinoStats>(true);
        if (dino == null) dino = GetComponentInParent<DinoStats>();

        BuildSpots();
    }
    private void BuildSpots()
    {
        var found = new List<GameObject>();

        if (dirtSpots != null && dirtSpots.Length > 0)
        {
            foreach (GameObject g in dirtSpots)
                if (g != null) found.Add(g);
        }
        else

        {
            foreach (Transform child in dinoRoot.GetComponentsInChildren<Transform>(true))
            {
                if (child == dinoRoot) continue;
                if (child.name.StartsWith("Dirt", System.StringComparison.OrdinalIgnoreCase))
                    found.Add(child.gameObject);
            }
        }

        if (found.Count == 0)
            Debug.LogWarning("No dirt found oops");

        foreach (GameObject g in found)
        {
            var spot = new Spot
            {
                go = g,
                t = g.transform,
                renderers = g.GetComponentsInChildren<Renderer>(true),
                baseScale = g.transform.localScale
            };

            spots.Add(spot);
            HideSpot(spot);
        }
    }

    private void OnEnable()
    {
        Active = this;
    }

    private void OnDisable()
    {
        if (Active == this) Active = null;
    }

    private void OnDestroy()
    {
        foreach (GameObject b in bubbles)
            if (b != null) Destroy(b);
    }

    private void Update()
    {
        if (dino == null || spots.Count == 0) return;

        bool dirty = dino.isDirty;

        if (!synced)
        {
            synced = true;
            wasDirty = dirty;
            lastCleanliness = dino.cleanliness;
            if (dirty) Reveal(SpotCountForCleanliness(dino.cleanliness));
            return;
        }

        if (dirty && !wasDirty)
        {
            decayAccum = 0f;
            Reveal(Mathf.Min(minSpots, spots.Count));
        }
        else if (!dirty && wasDirty)
        {
            HideAll();
            decayAccum = 0f;
        }
        wasDirty = dirty;

        if (dirty)
        {
            GrowDirt();

            if (VisibleCount() == 0)
                Reveal(Mathf.Min(minSpots, spots.Count));
        }

        lastCleanliness = dino.cleanliness;
    }
    private int VisibleCount()
    {
        int n = 0;
        foreach (Spot s in spots)
            if (s.visible) n++;
        return n;
    }

    private void ShowSpot(Spot s)
    {
        s.visible = true;
        s.health = 1f;
        s.t.localScale = s.baseScale;
        s.go.SetActive(true);
    }

    private void HideSpot(Spot s)
    {
        s.visible = false;
        s.health = 1f;
        s.t.localScale = s.baseScale;
        s.go.SetActive(false);
    }

    private void HideAll()
    {
        foreach (Spot s in spots)
            HideSpot(s);
    }

    private void Reveal(int count)
    {
        var hidden = new List<Spot>();
        foreach (Spot s in spots)
            if (!s.visible) hidden.Add(s);

        for (int i = 0; i < count && hidden.Count > 0; i++)
        {
            int pick = Random.Range(0, hidden.Count);
            ShowSpot(hidden[pick]);
            hidden.RemoveAt(pick);
        }
    }
    private int SpotCountForCleanliness(float cleanliness)
    {
        float dirtiness = 1f - cleanliness / DinoState.MaxCleanliness;
        int min = Mathf.Min(minSpots, spots.Count);
        return Mathf.Clamp(Mathf.CeilToInt(Mathf.Lerp(min, spots.Count, dirtiness)), 1, spots.Count);
    }

    private void GrowDirt()
    {
        float c = dino.cleanliness;
        if (c < lastCleanliness)
            decayAccum += lastCleanliness - c;

        int min = Mathf.Min(minSpots, spots.Count);
        float step = DinoState.MaxCleanliness / Mathf.Max(1, spots.Count - min);

        while (decayAccum >= step)
        {
            decayAccum -= step;
            if (VisibleCount() < spots.Count) Reveal(1);
        }
    }

    public void Wash(Vector2 sprayScreenPoint, Camera cam, float radiusPixels, float dt)
    {
        for (int i = spots.Count - 1; i >= 0; i--)
        {
            Spot s = spots[i];
            if (!s.visible) continue;
            if (!SprayTouchesSpot(s, cam, sprayScreenPoint, radiusPixels)) continue;

            s.health -= dt / secondsToWashSpot;

            float h = Mathf.Clamp01(s.health);
            s.t.localScale = s.baseScale * Mathf.Lerp(shrinkTo, 1f, h);

            if (s.health <= 0f)
            {
                HideSpot(s);

                if (OnSpotWashed())
                    return;
            }
        }
    }
    private bool SprayTouchesSpot(Spot s, Camera cam, Vector2 point, float radius)
    {
        Bounds b = default;
        bool has = false;

        foreach (Renderer r in s.renderers)
        {
            if (r == null || !r.enabled) continue;

            if (!has) { b = r.bounds; has = true; }
            else b.Encapsulate(r.bounds);
        }

        if (!has)
        {
            Vector3 sp = cam.WorldToScreenPoint(s.t.position);
            return sp.z > 0f && Vector2.Distance(point, sp) <= radius;
        }

        if (!ProjectBounds(b, cam, out Rect rect))
            return false;

        float dx = Mathf.Max(rect.xMin - point.x, 0f, point.x - rect.xMax);
        float dy = Mathf.Max(rect.yMin - point.y, 0f, point.y - rect.yMax);
        return dx * dx + dy * dy <= radius * radius;
    }

    private bool OnSpotWashed()
    {
        int left = VisibleCount();

        if (left == 0)
        {
            dino.Clean(DinoState.MaxCleanliness);
            SpawnBubbles();
            SpawnShine();
            PlayFullyCleanSound();
            onWashed?.Invoke();
            TutorialEvents.Raise(TutorialAction.DinoWashed);
            return true;
        }

        dino.Clean((DinoState.MaxCleanliness - dino.cleanliness) / (left + 1));
        return false;
    }

    private void PlayFullyCleanSound()
    {
        if (cleanAudioSource == null || fullyCleanClip == null) return;

        cleanAudioSource.PlayOneShot(fullyCleanClip, fullyCleanVolume);
    }
    private static float WorldSizeToScale(Sprite sprite, float worldWidth)
    {
        float natural = sprite != null ? sprite.bounds.size.x : 1f;
        return worldWidth / Mathf.Max(natural, 0.0001f);
    }

    private static bool ProjectBounds(Bounds b, Camera cam, out Rect rect)
    {
        rect = default;

        Vector2 min = new Vector2(float.MaxValue, float.MaxValue);
        Vector2 max = new Vector2(float.MinValue, float.MinValue);
        bool any = false;

        for (int i = 0; i < 8; i++)
        {
            Vector3 corner = new Vector3(
                (i & 1) == 0 ? b.min.x : b.max.x,
                (i & 2) == 0 ? b.min.y : b.max.y,
                (i & 4) == 0 ? b.min.z : b.max.z);

            Vector3 sp = cam.WorldToScreenPoint(corner);
            if (sp.z <= 0f) continue;

            min = Vector2.Min(min, sp);
            max = Vector2.Max(max, sp);
            any = true;
        }

        if (!any) return false;

        rect = Rect.MinMaxRect(min.x, min.y, max.x, max.y);
        return true;
    }

    private bool TrySampleSurface(Camera cam, Rect rect, out RaycastHit result)
    {
        result = default;

        for (int attempt = 0; attempt < 30; attempt++)
        {
            Vector2 p = new Vector2(
                Random.Range(rect.xMin, rect.xMax),
                Random.Range(rect.yMin, rect.yMax));

            if (!Physics.Raycast(cam.ScreenPointToRay(p), out RaycastHit hit, Mathf.Infinity))
                continue;

            if (!hit.collider.transform.IsChildOf(dinoRoot))
                continue;

            result = hit;
            return true;
        }

        return false;
    }
    private bool TryGetScreenRect(Camera cam, out Rect rect)
    {
        rect = default;

        Collider[] colliders = dinoRoot.GetComponentsInChildren<Collider>();
        if (colliders.Length == 0) return false;

        Bounds b = colliders[0].bounds;
        for (int i = 1; i < colliders.Length; i++)
            b.Encapsulate(colliders[i].bounds);

        if (!ProjectBounds(b, cam, out Rect full)) return false;

        Vector2 min = Vector2.Max(new Vector2(full.xMin, full.yMin), Vector2.zero);
        Vector2 max = Vector2.Min(new Vector2(full.xMax, full.yMax), new Vector2(Screen.width, Screen.height));
        if (max.x <= min.x || max.y <= min.y) return false;

        rect = Rect.MinMaxRect(min.x, min.y, max.x, max.y);
        return true;
    }

    private void SpawnShine()
    {
        if (shineSprites == null || shineSprites.Length == 0) return;

        Camera cam = Camera.main;
        if (cam == null || !TryGetScreenRect(cam, out Rect rect)) return;

        for (int i = 0; i < shineCount; i++)
        {
            if (!TrySampleSurface(cam, rect, out RaycastHit hit))
                continue;

            GameObject go = new GameObject("Shine");
            Transform t = go.transform;

            t.SetParent(hit.collider.transform, true);
            t.position = hit.point + hit.normal * 0.05f;
            t.rotation = cam.transform.rotation * Quaternion.Euler(0f, 0f, Random.Range(-20f, 20f));

            Sprite sprite = shineSprites[Random.Range(0, shineSprites.Length)];
            float s = WorldSizeToScale(sprite, Random.Range(shineSizeRange.x, shineSizeRange.y));
            Vector3 ps = t.parent.lossyScale;
            Vector3 baseScale = new Vector3(
                ps.x != 0f ? s / ps.x : s,
                ps.y != 0f ? s / ps.y : s,
                ps.z != 0f ? s / ps.z : s);
            t.localScale = Vector3.zero;

            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = sortingOrder + 10;

            StartCoroutine(Twinkle(t, sr, baseScale, Random.Range(0f, shineMaxDelay)));
        }
    }

    private IEnumerator Twinkle(Transform t, SpriteRenderer sr, Vector3 baseScale, float delay)
    {
        sr.enabled = false;
        yield return new WaitForSeconds(delay);
        if (t == null) yield break;

        sr.enabled = true;
        Quaternion startRotation = t.localRotation;
        float spin = Random.Range(-shineSpin, shineSpin);
        float time = 0f;

        while (time < shineDuration)
        {
            if (t == null) yield break;

            time += Time.deltaTime;
            float k = time / shineDuration;

            t.localScale = baseScale * Mathf.Sin(k * Mathf.PI);
            t.localRotation = startRotation * Quaternion.Euler(0f, 0f, spin * k);

            yield return null;
        }

        if (t != null) Destroy(t.gameObject);
    }

    private void SpawnBubbles()
    {
        if (bubbleSprites == null || bubbleSprites.Length == 0) return;

        Camera cam = Camera.main;
        if (cam == null || !TryGetScreenRect(cam, out Rect rect)) return;

        for (int i = 0; i < bubbleCount; i++)
        {
            if (!TrySampleSurface(cam, rect, out RaycastHit hit))
                continue;

            GameObject go = new GameObject("Bubble");
            go.transform.position = hit.point + hit.normal * 0.1f;
            go.transform.rotation = cam.transform.rotation;

            Sprite sprite = bubbleSprites[Random.Range(0, bubbleSprites.Length)];

            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = sortingOrder + 5;

            float s = WorldSizeToScale(sprite, Random.Range(bubbleSizeRange.x, bubbleSizeRange.y));

            bubbles.Add(go);
            StartCoroutine(FloatBubble(
                go.transform,
                sr,
                Vector3.one * s,
                Random.Range(0f, 6.28f)));
        }
    }

    private IEnumerator FloatBubble(Transform t, SpriteRenderer sr, Vector3 baseScale, float phase)
    {
        Vector3 start = t.position;
        float time = 0f;

        while (time < bubbleLifetime)
        {
            if (t == null) yield break;

            time += Time.deltaTime;
            float k = time / bubbleLifetime;

            Camera cam = Camera.main;
            Vector3 right = cam != null ? cam.transform.right : Vector3.right;

            t.position = start
                         + Vector3.up * (bubbleRiseSpeed * time)
                         + right * (Mathf.Sin(phase + time * 3f) * bubbleSway);

            if (cam != null) t.rotation = cam.transform.rotation;

            t.localScale = baseScale * Mathf.Clamp01(time / 0.2f);

            Color c = sr.color;
            c.a = 1f - k * k;
            sr.color = c;

            yield return null;
        }

        if (t != null) Destroy(t.gameObject);
    }
}