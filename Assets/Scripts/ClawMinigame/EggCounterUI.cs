
using UnityEngine;
using TMPro;

namespace DinoDig
{
    public class EggCounterUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text eggCounterText;

        private void Start()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnEggCollected += UpdateEggCounter;

                // Initialize the counter
                UpdateEggCounter(0);
            }
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnEggCollected -= UpdateEggCounter;
            }
        }

        private void UpdateEggCounter(int column)
        {
            eggCounterText.text = "Eggs: " + GameManager.Instance.EggsCollected;
        }
    }
}