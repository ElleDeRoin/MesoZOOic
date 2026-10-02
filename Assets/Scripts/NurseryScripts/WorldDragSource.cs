using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


public abstract class WorldDragSource : MonoBehaviour,
    IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Drag")]
    [SerializeField] protected Image iconImage;
    [SerializeField] protected LayerMask dropMask = ~0;
    [SerializeField] private float maxRayDistance = 200f;
    [SerializeField] private float ghostScale = 1.1f;

    private Canvas rootCanvas;
    private RectTransform ghost;
    private bool dragging;

    protected virtual void Awake()
    {
        rootCanvas = GetComponentInParent<Canvas>().rootCanvas;
    }

    protected abstract bool CanBeginDrag();
    protected abstract void OnDroppedOnWorld(RaycastHit hit);
    protected virtual void OnDragStarted() { }
    protected virtual void OnDragEnded() { }
    protected virtual void OnDroppedNowhere() { }

    public void OnBeginDrag(PointerEventData e)
    {
        if (!CanBeginDrag()) return;
        dragging = true;
        CreateGhost();
        MoveGhost(e);
        OnDragStarted();
    }

    public void OnDrag(PointerEventData e)
    {
        if (dragging) MoveGhost(e);
    }

    public void OnEndDrag(PointerEventData e)
    {
        if (!dragging) return;
        dragging = false;

        if (ghost != null) Destroy(ghost.gameObject);
        OnDragEnded();

        
        bool overUI = e.pointerCurrentRaycast.gameObject != null;

        Camera cam = Camera.main;
        if (!overUI && cam != null)
        {
            Ray ray = cam.ScreenPointToRay(e.position);
            if (Physics.Raycast(ray, out RaycastHit hit, maxRayDistance, dropMask,
                                QueryTriggerInteraction.Collide))
            {
                OnDroppedOnWorld(hit);
                return;
            }
        }
        OnDroppedNowhere();
    }

    private void CreateGhost()
    {
        var go = new GameObject("DragGhost", typeof(RectTransform), typeof(Image));
        ghost = go.GetComponent<RectTransform>();
        ghost.SetParent(rootCanvas.transform, false);
        ghost.SetAsLastSibling();

        var img = go.GetComponent<Image>();
        img.sprite = iconImage != null ? iconImage.sprite : null;
        img.preserveAspect = true;
        img.raycastTarget = false;

        var src = (RectTransform)(iconImage != null ? iconImage.transform : transform);
        ghost.sizeDelta = src.rect.size;
        ghost.localScale = Vector3.one * ghostScale;
    }

    private void MoveGhost(PointerEventData e)
    {
        if (ghost == null) return;
        Camera uiCam = rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay
            ? null : rootCanvas.worldCamera;

        RectTransformUtility.ScreenPointToWorldPointInRectangle(
            (RectTransform)rootCanvas.transform, e.position, uiCam, out Vector3 world);
        ghost.position = world;
    }
}