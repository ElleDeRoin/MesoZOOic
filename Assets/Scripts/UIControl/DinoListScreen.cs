using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DinoListScreen : MonoBehaviour
{
    public Diet diet;
    public string penSceneName = "Pen";

    public Transform content;
    public DinoCard cardPrefab;
    public TMP_InputField searchField;

    private void Start()
    {
        if (searchField != null) searchField.onValueChanged.AddListener(_ => Refresh());
        Refresh();
    }

    private void Refresh()
    {
        foreach (Transform child in content) Destroy(child.gameObject);

        string query = searchField != null ? searchField.text.Trim().ToLowerInvariant() : "";

        foreach (var owned in DinoCollection.Instance.GetByDiet(diet))
        {
            var card = Instantiate(cardPrefab, content);
            card.Bind(owned, OnDinoChosen);
            if (query.Length > 0 && !card.DisplayName.ToLowerInvariant().Contains(query))
                card.gameObject.SetActive(false);
        }
    }

    private void OnDinoChosen(OwnedDino owned)
    {
        GameSession.SelectedDino = owned;
        GameSession.ReturnScene = SceneManager.GetActiveScene().name;   // so Back returns here
        SceneManager.LoadScene(penSceneName);
    }
}