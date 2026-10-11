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

    public OwnedDino Owned { get; private set; }
    public string DisplayName { get; private set; }

    private DinoState state;

    public void Bind(OwnedDino owned, System.Action<OwnedDino> onClick)
    {
        Owned = owned;
        var data = DinoCollection.Instance.GetData(owned.dataId);
        DisplayName = data.displayName;
        nameText.text = data.displayName;
        icon.sprite = data.icon;
        DinoRegistry.Instance.TryGet(owned.instanceId, out state);

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => onClick(owned));
    }

    private void Update()
    {
        if (state == null) return;
        moneyText.text = $"${state.currentMoneyRate:0.##}/sec";
        happinessSlider.value = state.happiness;
        hungerSlider.value = state.hunger;
    }
}