using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class DinoWashing : MonoBehaviour
{
    public static DinoWashing Active { get; private set; }

    public static bool IsWashing { get; set; }

    [Header("Setup")]
    [SerializeField] private Transform dinoRoot;

    [Header("Dirt")]
    [SerializeField] private Sprite[] dirtSprites;
    [SerializeField] private int minDirt = 4;
    [SerializeField] private int maxDirt = 12;
    [SerializeField] private float dirtSize = 0.4f;
    [SerializeField] private float surfaceOffset = 0.05f;
    [SerializeField] private float minDirtSpacing = 0.15f;
    [SerializeField] private int sortingOrder = 10;
    [SerializeField] private float secondsToWashDirt = 0.6f;

    [Header("Bubbles 030")]
    [SerializeField] private Sprite[] bubbleSprites;
    [SerializeField] private int bubbleCount = 15;
    [SerializeField] private float bubbleLifetime = 1.5f;
    [SerializeField] private float bubbleRiseSpeed = 0.5f;
    [SerializeField] private float bubbleSway = 0.1f;
    [SerializeField] private Vector2 bubbleSizeRange = new Vector2(0.2f, 0.45f);

    [Header("Shine!!!")]
    [SerializeField] private Sprite[] shineSprites;
    [SerializeField] private int shineCount = 6;
    [SerializeField] private float shineDuration = 0.8f;
    [SerializeField] private float shineMaxDelay = 0.6f;
    [SerializeField] private Vector2 shineSizeRange = new Vector2(0.2f, 0.4f);
    [SerializeField] private float shineSpin = 90f;

    [Header("Events")]
    public UnityEvent onWashed;

    private class Dirt
    {
        public Transform t;
        public SpriteRenderer sr;
        public Vector3 baseScale;
        public Color baseColor;
        public float roll;
        public float health = 1f;
    }

    private readonly List<Dirt> dirts = new List<Dirt>();
    private readonly List<GameObject> bubbles = new List<GameObject>();

    private DinoStats dino;
    private bool synced;
    private bool wasDirty;
    private bool warnedNoSprites;
    private float lastCleanliness = DinoState.MaxCleanliness;
    private float decayAccum;
    private float retryTimer;

    private void Awake()
    {
        if (dinoRoot == null) dinoRoot = transform;

        dino = GetComponent<DinoStats>();
        if (dino == null) dino = GetComponentInChildren<DinoStats>(true);
        if (dino == null) dino = GetComponentInParent<DinoStats>();
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
        if (dino == null) return;

        bool dirty = dino.isDirty;
        if (!synced)
        {
            synced = true;
            wasDirty = dirty;
            lastCleanliness = dino.cleanliness;
            if (dirty) SpawnDirt(DirtCountForCleanliness(dino.cleanliness));
            return;
        }

        if (dirty && !wasDirty)
        {
            decayAccum = 0f;
            SpawnDirt(minDirt);
        }
        else if (!dirty && wasDirty)
        {
            ClearDirt();
            decayAccum = 0f;
        }
        wasDirty = dirty;

        if (dirty)
        {
            GrowDirt();
            if (dirts.Count == 0)
            {
                retryTimer -= Time.deltaTime;
                if (retryTimer <= 0f)
                {
                    retryTimer = 1f;
                    SpawnDirt(minDirt);
                }
            }
        }

        lastCleanliness = dino.cleanliness;
    }

    private void LateUpdate()
    {
        if (dirts.Count == 0) return;

        Camera cam = Camera.main;
        if (cam == null) return;

        foreach (Dirt d in dirts)
        {
            if (d.t != null)
                d.t.rotation = cam.transform.rotation * Quaternion.Euler(0f, 0f, d.roll);
        }
    }

    private void GrowDirt()
    {
        float c = dino.cleanliness;
        if (c < lastCleanliness)
            decayAccum += lastCleanliness - c;

        float step = DinoState.MaxCleanliness / Mathf.Max(1, maxDirt - minDirt);

        while (decayAccum >= step)
        {
            decayAccum -= step;
            if (dirts.Count < maxDirt) SpawnDirt(1);
        }
    }

    private int DirtCountForCleanliness(float cleanliness)
    {
        float dirtiness = 1f - cleanliness / DinoState.MaxCleanliness;
        return Mathf.Clamp(Mathf.CeilToInt(Mathf.Lerp(minDirt, maxDirt, dirtiness)), 1, maxDirt);
    }

    public void Wash(Vector2 sprayScreenPoint, Camera cam, float radiusPixels, float dt)
    {
        for (int i = dirts.Count - 1; i >= 0; i--)
        {
            Dirt d = dirts[i];
            Vector3 dirtScreen = cam.WorldToScreenPoint(d.t.position);

            if (dirtScreen.z <= 0f) continue;
            if (Vector2.Distance(sprayScreenPoint, dirtScreen) > radiusPixels) continue;

            d.health -= dt / secondsToWashDirt;

            float h = Mathf.Clamp01(d.health);
            Color c = d.baseColor;
            c.a = d.baseColor.a * h;
            d.sr.color = c;
            d.t.localScale = d.baseScale * Mathf.Lerp(0.6f, 1f, h);

            if (d.health <= 0f)
            {
                Destroy(d.t.gameObject);
                dirts.RemoveAt(i);

                if (OnDirtWashed())
                    return;
            }
        }
    }

    private bool OnDirtWashed()
    {
        if (dirts.Count == 0)
        {
            dino.Clean(DinoState.MaxCleanliness);
            SpawnBubbles();
            SpawnShine();
            onWashed?.Invoke();
            return true;
        }

        int countBefore = dirts.Count + 1;
        dino.Clean((DinoState.MaxCleanliness - dino.cleanliness) / countBefore);
        return false;
    }

    private static float WorldSizeToScale(Sprite sprite, float worldWidth)
    {
        float natural = sprite != null ? sprite.bounds.size.x : 1f;
        return worldWidth / Mathf.Max(natural, 0.0001f);
    }

    private void SpawnDirt(int count)
    {
        if (dirtSprites == null || dirtSprites.Length == 0)
        {
            if (!warnedNoSprites)
            {
                Debug.LogWarning("forgot dino washing sprite silly!", this);
                warnedNoSprites = true;
            }
            return;
        }

        Camera cam = Camera.main;
        if (cam == null || !TryGetScreenRect(cam, out Rect rect)) return;

        for (int i = 0; i < count && dirts.Count < maxDirt; i++)
        {
            if (TrySampleSurface(cam, rect, true, out RaycastHit hit))
                CreateDirt(hit, cam);
        }
    }

    private void CreateDirt(RaycastHit hit, Camera cam)
    {
        GameObject go = new GameObject("Dirt");
        Transform t = go.transform;

        t.SetParent(hit.collider.transform, true);

        Vector3 toCamera = (cam.transform.position - hit.point).normalized;
        t.position = hit.point + toCamera * surfaceOffset;

        float roll = Random.Range(0f, 360f);
        t.rotation = cam.transform.rotation * Quaternion.Euler(0f, 0f, roll);

        Sprite sprite = dirtSprites[Random.Range(0, dirtSprites.Length)];
        float s = WorldSizeToScale(sprite, dirtSize * Random.Range(0.7f, 1.2f));
        Vector3 ps = t.parent.lossyScale;
        Vector3 localScale = new Vector3(
            ps.x != 0f ? s / ps.x : s,
            ps.y != 0f ? s / ps.y : s,
            ps.z != 0f ? s / ps.z : s);
        t.localScale = localScale;

        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingOrder = sortingOrder;

        dirts.Add(new Dirt
        {
            t = t,
            sr = sr,
            baseScale = localScale,
            baseColor = sr.color,
            roll = roll
        });
    }

    private void ClearDirt()
    {
        foreach (Dirt d in dirts)
            if (d.t != null) Destroy(d.t.gameObject);
        dirts.Clear();
    }

    private bool TrySampleSurface(Camera cam, Rect rect, bool keepSpaced, out RaycastHit result)
    {
        result = default;
        bool found = false;

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
            found = true;

            if (!keepSpaced || FarFromOtherDirt(hit.point))
                break;
        }

        return found;
    }

    private bool FarFromOtherDirt(Vector3 point)
    {
        float sqr = minDirtSpacing * minDirtSpacing;
        foreach (Dirt d in dirts)
            if ((d.t.position - point).sqrMagnitude < sqr)
                return false;
        return true;
    }

    private bool TryGetScreenRect(Camera cam, out Rect rect)
    {
        rect = default;

        Collider[] colliders = dinoRoot.GetComponentsInChildren<Collider>();
        if (colliders.Length == 0) return false;

        Bounds b = colliders[0].bounds;
        for (int i = 1; i < colliders.Length; i++)
            b.Encapsulate(colliders[i].bounds);

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

        min = Vector2.Max(min, Vector2.zero);
        max = Vector2.Min(max, new Vector2(Screen.width, Screen.height));
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
            if (!TrySampleSurface(cam, rect, true, out RaycastHit hit))
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
            if (!TrySampleSurface(cam, rect, false, out RaycastHit hit))
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