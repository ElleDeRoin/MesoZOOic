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

    [Header("Washing")]
    [SerializeField] private float washRadius = 80f;

    private RectTransform rect;
    private RectTransform canvasRect;
    private Vector2 restAnchoredPosition;
    private Vector3 grabOffset;
    private bool dragging;

    private Camera UICamera =>
        canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;

    private void Awake()
    {
        rect = (RectTransform)transform;

        if (canvas == null) canvas = GetComponentInParent<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("SShower hose in wrong spot dawg.", this);
            enabled = false;
            return;
        }

        canvas = canvas.rootCanvas;
        canvasRect = (RectTransform)canvas.transform;

        restAnchoredPosition = rect.anchoredPosition;

        if (nozzle == null) nozzle = transform;
        if (hoseImage == null) hoseImage = GetComponent<Image>();
        if (idleSprite == null && hoseImage != null) idleSprite = hoseImage.sprite;
    }

    private void OnDisable()
    {
        if (dragging)
        {
            dragging = false;
            DinoWashing.IsWashing = false;
            SetHeldSprite(false);
        }
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
        }

        if (dragging)
        {
            if (TryGetCanvasPoint(pos, uiCam, out Vector3 point))
                rect.position = point + grabOffset;

            Camera sceneCam = Camera.main;
            if (DinoWashing.Active != null && sceneCam != null)
            {
                Vector2 nozzleScreen = RectTransformUtility.WorldToScreenPoint(uiCam, nozzle.position);
                DinoWashing.Active.Wash(nozzleScreen, sceneCam, washRadius, Time.deltaTime);
            }
        }
        else if (returnToRestOnRelease)
        {
            float t = 1f - Mathf.Exp(-returnSpeed * Time.deltaTime);
            rect.anchoredPosition = Vector2.Lerp(rect.anchoredPosition, restAnchoredPosition, t);
        }
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