using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DinoRenamePopup : MonoBehaviour
{
    [SerializeField] private GameObject panel;
    [SerializeField] private TMP_InputField input;
    [SerializeField] private Button saveButton;
    [SerializeField] private Button cancelButton;

    private OwnedDino target;

    private void Awake()
    {
        if (input != null)
            input.characterLimit = DinoCollection.MaxNameLength;

        if (saveButton != null) saveButton.onClick.AddListener(Save);
        if (cancelButton != null) cancelButton.onClick.AddListener(Close);
        if (panel != null) panel.SetActive(false);
    }
    public void OpenForSelected()
    {
        if (GameSession.SelectedDino != null)
            Open(GameSession.SelectedDino);
    }

    public void Open(OwnedDino dino)
    {
        if (dino == null || panel == null || input == null) return;

        target = dino;
        input.text = dino.nickname ?? "";
        if (input.placeholder is TMP_Text hint)
            hint.text = DinoCollection.Instance.GetData(dino.dataId).displayName;

        panel.SetActive(true);
        input.ActivateInputField();
    }

    private void Save()
    {
        if (target != null)
            DinoCollection.Instance.Rename(target, input.text);

        Close();
    }

    private void Close()
    {
        target = null;
        if (panel != null) panel.SetActive(false);
    }
}