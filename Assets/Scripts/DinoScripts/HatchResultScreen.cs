using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class HatchResultScreen : MonoBehaviour
{
    public Image dinoImage;
    public TMP_Text nameText;
    public TMP_Text nextButtonLabel;
    public Button nextButton;
    public UnityEvent onFinished;

    private int index;
    private bool revealed;

    private void OnEnable()
    {
        index = 0;
        nextButton.onClick.RemoveAllListeners();
        nextButton.onClick.AddListener(OnNext);
        ShowSilhouette();
    }

    private void ShowSilhouette()
    {
        var dino = EggHatcher.LastHatch[index];
        dinoImage.sprite = dino.icon;
        dinoImage.color = Color.black;
        nameText.text = "[???]!";
        nextButtonLabel.text = "Reveal";
        revealed = false;
    }

    private void Reveal()
    {
        var dino = EggHatcher.LastHatch[index];
        dinoImage.color = Color.white;
        nameText.text = $"[{dino.displayName}]!";
        bool last = index >= EggHatcher.LastHatch.Count - 1;
        nextButtonLabel.text = last ? "Done" : "Next";
        revealed = true;
    }

    private void OnNext()
    {
        if (!revealed) { Reveal(); return; }

        index++;
        if (index >= EggHatcher.LastHatch.Count) { onFinished?.Invoke(); return; }
        ShowSilhouette();
    }
}