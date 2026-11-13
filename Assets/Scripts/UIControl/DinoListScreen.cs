using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class DinoListScreen : MonoBehaviour
{
    public Diet diet;
    public string penSceneName = "Pen";

    public Transform content;
    public DinoCard cardPrefab;
    public TMP_InputField searchField;

    [Header("Delete Confirrmation")]
    [SerializeField] private GameObject confirmPanel;
    [SerializeField] private TMP_Text confirmText;
    [SerializeField] private Button confirmYesButton;
    [SerializeField] private Button confirmNoButton;

    private OwnedDino pendingDelete;

    [Header("Rename")]
    [SerializeField] private DinoRenamePopup renamePopup;

    private void Start()
    {
        if (searchField != null) searchField.onValueChanged.AddListener(_ => Refresh());

        if (confirmPanel != null) confirmPanel.SetActive(false);
        if (confirmYesButton != null) confirmYesButton.onClick.AddListener(ConfirmDelete);
        if (confirmNoButton != null) confirmNoButton.onClick.AddListener(CancelDelete);
        Refresh();
    }

    private void Refresh()
    {
        foreach (Transform child in content) Destroy(child.gameObject);

        string query = searchField != null ? searchField.text.Trim().ToLowerInvariant() : "";

        foreach (var owned in DinoCollection.Instance.GetByDiet(diet))
        {
            var card = Instantiate(cardPrefab, content);
            card.Bind(owned, OnDinoChosen, OnDeleteRequested, renamePopup != null ? OnRenameRequested : (System.Action<OwnedDino>)null);
            if (query.Length > 0 && !card.SearchText.Contains(query))
                card.gameObject.SetActive(false);
        }
    }

    private void OnRenameRequested(OwnedDino owned)
    {
        if (renamePopup != null)
            renamePopup.Open(owned);
    }
    private void OnEnable()
    {
        DinoCollection.Instance.OnChanged += OnCollectionChanged;
    }

    private void OnDisable()
    {
        if (DinoCollection.Instance != null)
            DinoCollection.Instance.OnChanged -= OnCollectionChanged;
    }

    private void OnCollectionChanged()
    {
        if (content != null) Refresh();
    }

    private void OnDeleteRequested(OwnedDino owned)
    {
        if (confirmPanel == null)
        {
            DeleteDino(owned);
            return;
        }

        pendingDelete = owned;

        if (confirmText != null)
        {
            var data = DinoCollection.Instance.GetData(owned.dataId);
            string name = data != null ? data.displayName : "this dino";
            confirmText.text = $"Release {name}? This can't be undone.";
        }

        confirmPanel.SetActive(true);
    }

    private void ConfirmDelete()
    {
        if (confirmPanel != null) confirmPanel.SetActive(false);

        if (pendingDelete != null)
            DeleteDino(pendingDelete);

        pendingDelete = null;
    }

    private void CancelDelete()
    {
        pendingDelete = null;
        if (confirmPanel != null) confirmPanel.SetActive(false);
    }

    private void DeleteDino(OwnedDino owned)
    {
        if (GameSession.SelectedDino != null && GameSession.SelectedDino.instanceId == owned.instanceId)
            GameSession.SelectedDino = null;

        DinoCollection.Instance.Remove(owned);
    }

    private void OnDinoChosen(OwnedDino owned)
    {
        GameSession.SelectedDino = owned;
        GameSession.ReturnScene = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(penSceneName);
    }
}