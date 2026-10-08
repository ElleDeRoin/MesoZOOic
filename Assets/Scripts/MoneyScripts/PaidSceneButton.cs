using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class PaidSceneButton : MonoBehaviour
{
    [SerializeField] private string sceneName;
    [SerializeField] private CurrencyType currency = CurrencyType.Money;
    [SerializeField] private int cost = 50;

    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(OnClicked);
    }

    private void Start()
    {
        if (Wallet.Instance == null) return;

        if (currency == CurrencyType.Money)
        {
            Wallet.Instance.Changed += OnMoneyChanged;
            OnMoneyChanged(Wallet.Instance.Money);
        }
        else
        {
            Wallet.Instance.AmberChanged += OnAmberChanged;
            OnAmberChanged(Wallet.Instance.Amber);
        }
    }

    private void OnDestroy()
    {
        if (button != null) button.onClick.RemoveListener(OnClicked);

        if (Wallet.Instance == null) return;
        Wallet.Instance.Changed -= OnMoneyChanged;
        Wallet.Instance.AmberChanged -= OnAmberChanged;
    }

    private void OnMoneyChanged(float money) => button.interactable = money >= cost;
    private void OnAmberChanged(int Amber) => button.interactable = Amber >= cost;

    private void OnClicked()
    {
        if (Wallet.Instance == null) return;

        if (!Application.CanStreamedLevelBeLoaded(sceneName))
        {
            return;
        }

        bool paid = currency == CurrencyType.Money
            ? Wallet.Instance.TrySpend(cost)
            : Wallet.Instance.TrySpendAmber(cost);

        if (paid)
            SceneManager.LoadScene(sceneName);
    }
}