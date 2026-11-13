using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DecorationRemoveButtons : MonoBehaviour
{
    [Header("Setup")]
    [SerializeField] private GameObject decorTabPanel;
    [SerializeField] private Button removeButtonPrefab;
    [SerializeField] private RectTransform container;

    [Header("Placement")]
    [SerializeField] private Vector2 screenOffset = new Vector2(0f, 40f);

    private readonly Dictionary<DecorationSlot, Button> buttons = new Dictionary<DecorationSlot, Button>();
    private Canvas canvas;
    private RectTransform canvasRect;

    private void Awake()
    {
        Transform anchor = container != null ? container
            : decorTabPanel != null ? decorTabPanel.transform
            : transform;

        Canvas c = anchor.GetComponentInParent<Canvas>();
        if (c == null)
        {
            Debug.LogError("missing canvaw.", this);
            enabled = false;
            return;
        }

        canvas = c.rootCanvas;
        canvasRect = (RectTransform)canvas.transform;

        if (removeButtonPrefab == null)
        {
            Debug.LogError("missing button", this);
            enabled = false;
        }
    }
    private void OnDisable()
    {
        foreach (var pair in buttons)
            if (pair.Value != null) pair.Value.gameObject.SetActive(false);
    }

    private void LateUpdate()
    {
        bool tabOpen = decorTabPanel == null || decorTabPanel.activeInHierarchy;
        Camera cam = Camera.main;
        Camera uiCam = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;

        foreach (DecorationSlot slot in DecorationSlot.AllSlots)
        {
            if (slot == null) continue;

            bool want = tabOpen && cam != null && slot.Current != null;
            Vector3 sp = want ? cam.WorldToScreenPoint(slot.ItemTopWorldPosition) : Vector3.zero;
            if (want && sp.z <= 0f) want = false;

            if (!want)
            {
                if (buttons.TryGetValue(slot, out Button hidden) && hidden != null)
                    hidden.gameObject.SetActive(false);
                continue;
            }

            Button button = GetOrCreate(slot);

            if (!button.gameObject.activeSelf)
                button.gameObject.SetActive(true);

            if (RectTransformUtility.ScreenPointToWorldPointInRectangle(
                    canvasRect, (Vector2)sp + screenOffset, uiCam, out Vector3 world))
            {
                button.transform.position = world;
            }
        }
    }
    private Button GetOrCreate(DecorationSlot slot)
    {
        if (buttons.TryGetValue(slot, out Button existing) && existing != null)
            return existing;

        Transform parent = container != null ? container : canvasRect;
        Button button = Instantiate(removeButtonPrefab, parent, false);
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() =>
        {
            slot.Clear();
            button.gameObject.SetActive(false);
        });

        buttons[slot] = button;
        return button;
    }
}