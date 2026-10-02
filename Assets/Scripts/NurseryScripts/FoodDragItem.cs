using TMPro;
using UnityEngine;

public class FoodDragItem : WorldDragSource
{
    [SerializeField] private FoodData food;
    [SerializeField] private TMP_Text priceLabel;
    [SerializeField] private Color unaffordableTint = new Color(1f, 1f, 1f, 0.4f);

    protected override void Awake()
    {
        base.Awake();
        if (iconImage != null && food != null && food.icon != null)
            iconImage.sprite = food.icon;

        if (priceLabel != null)
            priceLabel.text = food.price > 0 ? "$" + food.price : "Free";
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
        if (iconImage == null) return;
        bool affordable = food.price <= 0f || money >= food.price;
        iconImage.color = affordable ? Color.white : unaffordableTint;
    }

    protected override bool CanBeginDrag()
    {
        return food.price <= 0f || (Wallet.Instance != null && Wallet.Instance.CanAfford(food.price));
    }

    protected override void OnDroppedOnWorld(RaycastHit hit)
    {
        var dino = hit.collider.GetComponentInParent<DinoStats>();
        if (dino == null) return;

        FeedResult result = dino.TryFeed(food);

        if (result == FeedResult.Ate && food.price > 0f && Wallet.Instance != null)
            Wallet.Instance.TrySpend(food.price);
    }
}