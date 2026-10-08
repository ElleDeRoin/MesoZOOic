using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class AmberRewardButton : MonoBehaviour
{
    [SerializeField] private int amount = 5;

    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(OnClicked);
    }

    private void OnDestroy()
    {
        if (button != null) button.onClick.RemoveListener(OnClicked);
    }

    private void OnClicked()
    {
        if (Wallet.Instance != null)
            Wallet.Instance.AddAmber(amount);
    }
}