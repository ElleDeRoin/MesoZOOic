using TMPro;
using UnityEngine;

public class MoneyDisplay : MonoBehaviour
{
    [SerializeField] private TMP_Text moneyText;

    private void Start()
    {
        Wallet.Instance.Changed += UpdateMoney;
        UpdateMoney(Wallet.Instance.Money);
    }

    private void OnDestroy()
    {
        if (Wallet.Instance != null)
            Wallet.Instance.Changed -= UpdateMoney;
    }

    private void UpdateMoney(float money)
    {
        moneyText.text = "$" + Mathf.FloorToInt(money).ToString("D4");
    }
}