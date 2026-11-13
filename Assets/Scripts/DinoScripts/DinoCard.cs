using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DinoCard : MonoBehaviour
{
    public TMP_Text nameText;
    public Image icon;
    public TMP_Text moneyText;
    public Slider happinessSlider;
    public Slider hungerSlider;
    public Button button;
    public Button deleteButton;
    public Button renameButton;

    public OwnedDino Owned { get; private set; }
    public string DisplayName { get; private set; }
    public string SearchText { get; private set; }

    private DinoState state;

    public void Bind(OwnedDino owned, System.Action<OwnedDino> onClick, System.Action<OwnedDino> onDelete = null, System.Action<OwnedDino> onRename = null)
    {
        Owned = owned;
        var data = DinoCollection.Instance.GetData(owned.dataId);
        DisplayName = DinoCollection.Instance.GetDisplayName(owned);
        SearchText = (DisplayName + " " + data.displayName).ToLowerInvariant();
        nameText.text = DisplayName;
        icon.sprite = data.icon;
        DinoRegistry.Instance.TryGet(owned.instanceId, out state);

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => onClick(owned));

        if (renameButton != null)
        {
            renameButton.onClick.RemoveAllListeners();
            renameButton.gameObject.SetActive(onRename != null);

            if (onRename != null)
                renameButton.onClick.AddListener(() => onRename(owned));
        }

        if (deleteButton != null)
        {
            deleteButton.onClick.RemoveAllListeners();
            deleteButton.gameObject.SetActive(onDelete != null);

            if (onDelete != null)
                deleteButton.onClick.AddListener(() => onDelete(owned));
        }
    }

    private void Update()
    {
        if (state == null) return;
        moneyText.text = $"${state.currentMoneyRate:0.##}/sec";
        happinessSlider.value = state.happiness;
        hungerSlider.value = state.hunger;
    }
}