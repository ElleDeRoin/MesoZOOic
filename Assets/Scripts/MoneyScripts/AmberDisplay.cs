using TMPro;
using UnityEngine;

public class AmberDisplay : MonoBehaviour
{
    [SerializeField] private TMP_Text amberText;

    private void Start()
    {
        Wallet.Instance.AmberChanged += UpdateAmber;
        UpdateAmber(Wallet.Instance.Amber);
    }

    private void OnDestroy()
    {
        if (Wallet.Instance != null)
            Wallet.Instance.AmberChanged -= UpdateAmber;
    }

    private void UpdateAmber(int amber)
    {
        amberText.text = amber.ToString();
    }
}