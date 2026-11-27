using TMPro;
using UnityEngine;
public class DinoNameLabel : MonoBehaviour
{
    [SerializeField] private TMP_Text label;

    private DinoCollection collection;

    private void OnEnable()
    {
        collection = DinoCollection.Instance;
        collection.OnChanged += Refresh;
        Refresh();
    }

    private void OnDisable()
    {
        if (collection != null) collection.OnChanged -= Refresh;
        collection = null;
    }
    private void Refresh()
    {
        if (label == null) return;

        OwnedDino selected = GameSession.SelectedDino;
        if (selected == null)
        {
            label.text = "";
            return;
        }

        label.text = DinoCollection.Instance.GetDisplayName(selected);
        label.color = RarityColors.Get(DinoCollection.Instance.GetData(selected.dataId).rarity);
    }
}