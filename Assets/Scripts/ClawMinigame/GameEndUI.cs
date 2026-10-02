
using UnityEngine;

namespace DinoDig
{
    public class GameEndUI : MonoBehaviour
    {
        [SerializeField] private GameObject winPopup;
        [SerializeField] private GameObject losePopup;

        private void Start()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnGameEnd += ShowGameEndPopup;
            }
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnGameEnd -= ShowGameEndPopup;
            }
        }

        private void ShowGameEndPopup(GameManager.GameEndState endState)
        {
            winPopup.SetActive(endState == GameManager.GameEndState.Win);
            losePopup.SetActive(endState == GameManager.GameEndState.Lose);
        }
    }
}