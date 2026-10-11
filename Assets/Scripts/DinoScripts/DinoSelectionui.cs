using TMPro;
using UnityEngine;

public class DinoSelectionUI : MonoBehaviour
{
    [Header("Panels")]
    public GameObject zooPanel;
    public GameObject listPanel;

    [Header("List panel")]
    public Transform content;
    public DinoCard cardPrefab;
    public TMP_Text headerText;
    public TMP_InputField searchField;

    [Header("Pen")]
    public PenManager penManager;

    private Diet currentDiet = Diet.Herbivore;

    private void Start()
    {
        DinoCollection.Instance.OnChanged += Refresh;
        if (searchField != null) searchField.onValueChanged.AddListener(_ => Refresh());
    }

    private void OnDestroy()
    {
        if (DinoCollection.Instance != null) DinoCollection.Instance.OnChanged -= Refresh;
    }

    public void OpenHerbivores() => OpenSection(Diet.Herbivore);
    public void OpenCarnivores() => OpenSection(Diet.Carnivore);
    public void OpenOmnivores()  => OpenSection(Diet.Omnivore);

    public void BackToZoo()
    {
        listPanel.SetActive(false);
        zooPanel.SetActive(true);
    }

    private void OpenSection(Diet diet)
    {
        currentDiet = diet;
        if (searchField != null) searchField.SetTextWithoutNotify("");

        zooPanel.SetActive(false);
        listPanel.SetActive(true);
        Refresh();
    }

    private void Refresh()
    {
        if (!listPanel.activeSelf) return;

        headerText.text = currentDiet switch
        {
            Diet.Herbivore => "Herbivores",
            Diet.Carnivore => "Carnivores",
            _ => "Omnivores"
        };

        foreach (Transform child in content) Destroy(child.gameObject);

        string query = searchField != null ? searchField.text.Trim().ToLowerInvariant() : "";

        foreach (var owned in DinoCollection.Instance.GetByDiet(currentDiet))
        {
            var card = Instantiate(cardPrefab, content);
            card.Bind(owned, OnDinoChosen);
            if (query.Length > 0 && !card.DisplayName.ToLowerInvariant().Contains(query))
                card.gameObject.SetActive(false);
        }
    }

    private void OnDinoChosen(OwnedDino owned)
    {
        listPanel.SetActive(false);
        penManager.ShowPen(owned);
    }
}