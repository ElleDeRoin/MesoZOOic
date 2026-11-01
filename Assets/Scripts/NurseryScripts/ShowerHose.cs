using UnityEngine;
using UnityEngine.UI;

public class ShowerHose : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform nozzle;
    [SerializeField] private Canvas canvas;

    [Header("Sprites")]
    [SerializeField] private Image hoseImage;
    [SerializeField] private Sprite idleSprite;
    [SerializeField] private Sprite heldSprite;

    [Header("Dragging")]
    [SerializeField] private float grabRadius = 150f;
    [SerializeField] private bool returnToRestOnRelease = true;
    [SerializeField] private float returnSpeed = 12f;

    [Header("Effects")]
    [SerializeField] private ParticleSystem sprayEffect;
    [SerializeField] private bool effectIsChildOfNozzle = false;
    [SerializeField] private float effectScale = 0.15f;
    [SerializeField] private bool matchDinoDepth = true;
    [SerializeField] private float effectDepthOffset = -0.5f;
    [SerializeField] private float effectDistance = 5f;

    [Header("Washing")]
    [SerializeField] private float washRadius = 80f;

    private RectTransform rect;
    private RectTransform canvasRect;
    private Vector2 restAnchoredPosition;
    private Vector3 grabOffset;
    private Vector3 effectBaseScale = Vector3.one;
    private bool dragging;
    private bool effectStarted;

    private Camera UICamera =>
        canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;

    private void Awake()
    {
        rect = (RectTransform)transform;

        if (canvas == null) canvas = GetComponentInParent<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("add showwer hose to canvas dawg", this);
            enabled = false;
            return;
        }

        canvas = canvas.rootCanvas;
        canvasRect = (RectTransform)canvas.transform;

        restAnchoredPosition = rect.anchoredPosition;

        if (nozzle == null) nozzle = transform;
        if (hoseImage == null) hoseImage = GetComponent<Image>();
        if (idleSprite == null && hoseImage != null) idleSprite = hoseImage.sprite;
        if (sprayEffect != null && !sprayEffect.gameObject.scene.IsValid())
        {
            Camera mainCam = Camera.main;
            sprayEffect = Instantiate(sprayEffect, mainCam != null ? mainCam.transform : null);
        }

        if (sprayEffect != null)
        {
            if (!effectIsChildOfNozzle)
            {
                Camera cam = Camera.main;
                if (cam != null && sprayEffect.GetComponentInParent<Canvas>() != null)
                {
                    sprayEffect.transform.SetParent(cam.transform, false);
                    sprayEffect.transform.localPosition = Vector3.zero;
                }

                effectBaseScale = sprayEffect.transform.localScale;
                sprayEffect.transform.localScale = effectBaseScale * effectScale;
            }

            foreach (ParticleSystem ps in sprayEffect.GetComponentsInChildren<ParticleSystem>(true))
            {
                ParticleSystem.MainModule main = ps.main;
                main.loop = true;
                main.playOnAwake = false;
                main.simulationSpace = ParticleSystemSimulationSpace.World;
                main.scalingMode = ParticleSystemScalingMode.Hierarchy;
            }
            sprayEffect.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
    }

    private void OnDisable()
    {
        if (dragging)
        {
            dragging = false;
            DinoWashing.IsWashing = false;
            SetHeldSprite(false);
        }

        StopSprayEffect();
    }

    private void Update()
    {
        Camera uiCam = UICamera;
        ReadPointer(out Vector2 pos, out bool pressed, out bool began);

        if (pressed && began && !dragging)
        {
            Vector2 hoseScreen = RectTransformUtility.WorldToScreenPoint(uiCam, rect.position);

            if (Vector2.Distance(pos, hoseScreen) <= grabRadius &&
                TryGetCanvasPoint(pos, uiCam, out Vector3 point))
            {
                dragging = true;
                grabOffset = rect.position - point;
                DinoWashing.IsWashing = true;
                SetHeldSprite(true);
            }
        }
        if (!pressed && dragging)
        {
            dragging = false;
            DinoWashing.IsWashing = false;
            SetHeldSprite(false);
            StopSprayEffect();
        }

        if (dragging)
        {
            if (TryGetCanvasPoint(pos, uiCam, out Vector3 point))
                rect.position = point + grabOffset;

            Camera sceneCam = Camera.main;
            Vector2 nozzleScreen = RectTransformUtility.WorldToScreenPoint(uiCam, nozzle.position);

            UpdateSprayEffect(sceneCam, nozzleScreen);

            if (DinoWashing.Active != null && sceneCam != null)
                DinoWashing.Active.Wash(nozzleScreen, sceneCam, washRadius, Time.deltaTime);
        }
        else if (returnToRestOnRelease)
        {
            float t = 1f - Mathf.Exp(-returnSpeed * Time.deltaTime);
            rect.anchoredPosition = Vector2.Lerp(rect.anchoredPosition, restAnchoredPosition, t);
        }
    }

    private void UpdateSprayEffect(Camera sceneCam, Vector2 nozzleScreen)
    {
        if (sprayEffect == null) return;

        if (effectIsChildOfNozzle)
        {
            StartEffectOnce();
            return;
        }
        if(sceneCam == null) return;

        float depth = effectDistance;
        if (matchDinoDepth && DinoWashing.Active != null)
        {
            Vector3 toDino = DinoWashing.Active.transform.position - sceneCam.transform.position;
            depth = Vector3.Dot(toDino, sceneCam.transform.forward) + effectDepthOffset;
        }
        depth = Mathf.Max(depth, sceneCam.nearClipPlane + 0.1f);

        sprayEffect.transform.localScale = effectBaseScale * effectScale;
        sprayEffect.transform.position = sceneCam.ScreenToWorldPoint(
            new Vector3(nozzleScreen.x, nozzleScreen.y, depth));

        if (!sprayEffect.gameObject.activeSelf)
            sprayEffect.gameObject.SetActive(true);

        StartEffectOnce();
    }

    private void StartEffectOnce()
    {
        if (effectStarted) return;

        effectStarted = true;
        sprayEffect.Play(true);
    }

    private void StopSprayEffect()
    {
        effectStarted = false;

        if (sprayEffect != null)
            sprayEffect.Stop(true, ParticleSystemStopBehavior.StopEmitting);
    }

    private void SetHeldSprite(bool held)
    {
        if (hoseImage == null) return;

        Sprite wanted = held ? heldSprite : idleSprite;
        if (wanted != null)
            hoseImage.sprite = wanted;
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