using UnityEngine;
using DinoDig;

public class EggTypeMarker : MonoBehaviour
{
    [SerializeField] private bool paidMinigame;

    private void Start()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.LastEggsWerePaid = paidMinigame;
    }
}