using TMPro;
using UnityEngine;

public class DecorationDragItem : WorldDragSource
{
    [SerializeField] private DecorationData decoration;
    [SerializeField] private TMP_Text nameLabel;
    [SerializeField] private TMP_Text priceLabel;
    [SerializeField] private Color unaffordableTint = new Color(1f, 1f, 1f, 0.4f);

    protected override void Awake()
    {
        base.Awake();
        if (iconImage != null && decoration.icon != null) iconImage.sprite = decoration.icon;
        if (nameLabel != null) nameLabel.text = decoration.displayName;
        if (priceLabel != null) priceLabel.text = "$" + decoration.price;
    }

    private void Start()
    {
        if (Wallet.Instance != null) Wallet.Instance.Changed += Refresh;
        Refresh(Wallet.Instance != null ? Wallet.Instance.Money : 0f);
    }

    private void OnDestroy()
    {
        if (Wallet.Instance != null) Wallet.Instance.Changed -= Refresh;
    }

    private void Refresh(float money)
    {
        if (iconImage != null)
            iconImage.color = money >= decoration.price ? Color.white : unaffordableTint;
    }

    protected override bool CanBeginDrag()
    {
        return Wallet.Instance != null && Wallet.Instance.CanAfford(decoration.price);
    }

    protected override void OnDragStarted() => DecorationSlot.ShowMarkersFor(decoration);
    protected override void OnDragEnded() => DecorationSlot.HideAllMarkers();

    protected override void OnDroppedOnWorld(RaycastHit hit)
    {
        var slot = hit.collider.GetComponentInParent<DecorationSlot>();
        if (slot == null || !slot.CanAccept(decoration)) return;

        if (Wallet.Instance.TrySpend(decoration.price))
            slot.Place(decoration);
    }
}