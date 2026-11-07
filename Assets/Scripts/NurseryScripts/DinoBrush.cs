using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class DinoBrush : MonoBehaviour
{
    public static bool IsBrushing { get; private set; }
    private static float nextReadyTime;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        IsBrushing = false;
        nextReadyTime = 0f;
    }

    [Header("References")]
    [SerializeField] private Transform brushPoint;
    [SerializeField] private Canvas canvas;

    [Header("Sprites")]
    [SerializeField] private Image brushImage;
    [SerializeField] private Sprite readySprite;
    [SerializeField] private Sprite heldSprite;
    [SerializeField] private Sprite cooldownSprite;

    [Header("Dragging")]
    [SerializeField] private float grabRadius = 150f;
    [SerializeField] private bool returnToRestOnRelease = true;
    [SerializeField] private float returnSpeed = 12f;

    [Header("Brushing")]
    [SerializeField] private float happinessPerStroke = 15f;
    [SerializeField] private float movementRequired = 30f;
    [SerializeField] private float strokeCooldown = 0.1f;
    [SerializeField] private int maxStrokesPerUse = 15;
    [SerializeField] private float cooldownSeconds = 30f;

    [Header("Audio")]
    [SerializeField] private AudioSource brushAudioSource;
    [SerializeField] private AudioClip[] brushStrokeClips;
    [Range(0f, 1f)]
    [SerializeField] private float brushVolume = 1f;
    [SerializeField] private Vector2 pitchRange = new Vector2(0.95f, 1.05f);

    [Header("Events")]
    public UnityEvent onStroke;
    public UnityEvent onSessionEnded;
    public UnityEvent onReady;

    private RectTransform rect;
    private RectTransform canvasRect;
    private Vector2 restAnchoredPosition;
    private Vector3 grabOffset;
    private Vector2 lastStrokePosition;
    private DinoPetting cachedPetting;
    private bool dragging;
    private bool wasOverDino;
    private bool wasOnCooldown;
    private float strokeTimer;
    private int strokes;


    private bool OnCooldown => Time.time < nextReadyTime;

    private Camera UICamera =>
        canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;

    private void Awake()
    {

        rect = (RectTransform)transform;

        if (canvas == null) canvas = GetComponentInParent<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("add dinobrush to the canvas silly", this);
            enabled = false;
            return;
        }

        canvas = canvas.rootCanvas;
        canvasRect = (RectTransform)canvas.transform;

        restAnchoredPosition = rect.anchoredPosition;

        if (brushAudioSource == null) brushAudioSource = GetComponent<AudioSource>();
        if (brushAudioSource != null) brushAudioSource.playOnAwake = false;

        if (brushPoint == null) brushPoint = transform;
        if (brushImage == null) brushImage = GetComponent<Image>();
        if (readySprite == null && brushImage != null) readySprite = brushImage.sprite;
    }
    private void OnEnable()
    {
        wasOnCooldown = OnCooldown;
        RefreshSprite();
    }

    private void OnDisable()
    {
        if (dragging) EndSession();
    }
    private void Update()
    {
        Camera uiCam = UICamera;
        ReadPointer(out Vector2 pos, out bool pressed, out bool began);

        bool onCooldown = OnCooldown;
        if (onCooldown != wasOnCooldown)
        {
            wasOnCooldown = onCooldown;
            RefreshSprite();
            if (!onCooldown) onReady?.Invoke();
        }

        strokeTimer -= Time.deltaTime;



        if (pressed && began && !dragging && !onCooldown)
        {
            Vector2 brushScreen = RectTransformUtility.WorldToScreenPoint(uiCam, rect.position);

            if (Vector2.Distance(pos, brushScreen) <= grabRadius &&
                TryGetCanvasPoint(pos, uiCam, out Vector3 point))
            {
                dragging = true;
                grabOffset = rect.position - point;
                strokes = 0;
                wasOverDino = false;
                IsBrushing = true;
                RefreshSprite();
            }
        }


        if (!pressed && dragging)
            EndSession();

        if (dragging)
        {
            if (TryGetCanvasPoint(pos, uiCam, out Vector3 point))
                rect.position = point + grabOffset;

            HandleBrushing(uiCam);
        }
        else if (returnToRestOnRelease)
        {
            float t = 1f - Mathf.Exp(-returnSpeed * Time.deltaTime);
            rect.anchoredPosition = Vector2.Lerp(rect.anchoredPosition, restAnchoredPosition, t);
        }
    }
    private void HandleBrushing(Camera uiCam)
    {
        DinoPetting petting = FindPetting();
        if (petting == null) return;

        Vector2 tip = RectTransformUtility.WorldToScreenPoint(uiCam, brushPoint.position);
        bool overDino = petting.IsTouchingDino(tip);

        if (!overDino || !wasOverDino)
            lastStrokePosition = tip;

        wasOverDino = overDino;
        if (!overDino) return;


        if (Vector2.Distance(tip, lastStrokePosition) >= movementRequired && strokeTimer <= 0f)
        {
            petting.PetDino(happinessPerStroke);
            PlayStrokeSound();
            onStroke?.Invoke();

            strokes++;
            lastStrokePosition = tip;
            strokeTimer = strokeCooldown;

            if (strokes >= maxStrokesPerUse)
                EndSession();
        }
    }
    private void EndSession()
    {
        dragging = false;
        IsBrushing = false;

        if (strokes > 0)
        {
            nextReadyTime = Time.time + cooldownSeconds;
            wasOnCooldown = true;
            onSessionEnded?.Invoke();
            TutorialEvents.Raise(TutorialAction.DinoBrushed);
        }

        strokes = 0;
        RefreshSprite();
    }


    private void PlayStrokeSound()
    {
        if (brushAudioSource == null || brushStrokeClips == null || brushStrokeClips.Length == 0)
            return;

        AudioClip clip = brushStrokeClips[Random.Range(0, brushStrokeClips.Length)];
        if (clip == null) return;

        brushAudioSource.pitch = Random.Range(pitchRange.x, pitchRange.y);
        brushAudioSource.PlayOneShot(clip, brushVolume);
    }

    private void RefreshSprite()
    {
        if (brushImage == null) return;

        Sprite wanted;
        if (OnCooldown) wanted = cooldownSprite;
        else if (dragging && heldSprite != null) wanted = heldSprite;
        else wanted = readySprite;

        if (wanted != null)
            brushImage.sprite = wanted;
    }


    private DinoPetting FindPetting()
    {
        if (cachedPetting != null) return cachedPetting;

        foreach (DinoStats stats in DinoStats.All)
        {
            if (stats == null) continue;

            DinoPetting p = stats.GetComponent<DinoPetting>();
            if (p == null) p = stats.GetComponentInParent<DinoPetting>();
            if (p == null) p = stats.GetComponentInChildren<DinoPetting>();

            if (p != null)
            {
                cachedPetting = p;
                return p;
            }
        }

        return null;
    }

    private bool TryGetCanvasPoint(Vector2 screenPos, Camera uiCam, out Vector3 worldPoint)
    {
        return RectTransformUtility.ScreenPointToWorldPointInRectangle(
            canvasRect, screenPos, uiCam, out worldPoint);
    }
    private static void ReadPointer(out Vector2 pos, out bool pressed, out bool began)
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            pos = touch.position;
            pressed = touch.phase != TouchPhase.Ended && touch.phase != TouchPhase.Canceled;
            began = touch.phase == TouchPhase.Began;
        }
        else
        {
            pos = Input.mousePosition;
            pressed = Input.GetMouseButton(0);
            began = Input.GetMouseButtonDown(0);
        }
    }
}