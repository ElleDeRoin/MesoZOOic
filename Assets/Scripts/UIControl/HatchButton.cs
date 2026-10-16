using TMPro;
using UnityEngine;
using DinoDig;

public class HatchButton : MonoBehaviour
{
    [SerializeField] private TMP_Text eggsGotText;
    private bool hatched;

    private void OnEnable()
    {
        hatched = false;

        if (eggsGotText != null && GameManager.Instance != null)
        {
            int n = GameManager.Instance.EggsCollected;
            eggsGotText.text = n == 1 ? "You got 1 egg!" : $"You got {n} eggs!";
        }
    }

    public void HatchEggs()
    {
        if (hatched || GameManager.Instance == null) return;
        int eggs = GameManager.Instance.EggsCollected;
        if (eggs <= 0) return;

        hatched = true;
        EggHatcher.Hatch(eggs, GameManager.Instance.LastEggsWerePaid);
    }
}