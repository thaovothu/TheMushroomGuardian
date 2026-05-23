using UnityEngine;
using TMPro;

/// <summary>
/// Hiển thị và quản lý UI tiền coin và exp
/// </summary>
public class UIMoney : BaseSingleton<UIMoney>
{
    [SerializeField] private TextMeshProUGUI coinText;      // Hiển thị số coin
    [SerializeField] private TextMeshProUGUI expText;       // Hiển thị số exp

    private int totalCoins = 0;
    private int totalExp = 0;


    private void Start()
    {
        RefreshUI();
    }

    /// <summary>
    /// Thêm coin
    /// </summary>
    public void AddCoin(int amount)
    {
        if (amount <= 0) return;

        totalCoins += amount;
        RefreshCoinText();
        Debug.Log($"[UIMoney] +{amount} coin (Total: {totalCoins})");
    }

    /// <summary>
    /// Thêm exp
    /// </summary>
    public void AddExp(int amount)
    {
        if (amount <= 0) return;

        totalExp += amount;
        RefreshExpText();
        Debug.Log($"[UIMoney] +{amount} exp (Total: {totalExp})");
    }

    /// <summary>
    /// Cập nhật text coin
    /// </summary>
    private void RefreshCoinText()
    {
        if (coinText != null)
        {
            coinText.text = $"Coin: {totalCoins}";
        }
    }

    /// <summary>
    /// Cập nhật text exp
    /// </summary>
    private void RefreshExpText()
    {
        if (expText != null)
        {
            expText.text = $"EXP: {totalExp}";
        }
    }

    /// <summary>
    /// Cập nhật cả coin và exp
    /// </summary>
    private void RefreshUI()
    {
        RefreshCoinText();
        RefreshExpText();
    }

    /// <summary>
    /// Lấy số coin hiện tại
    /// </summary>
    public int GetTotalCoins() => totalCoins;

    /// <summary>
    /// Lấy số exp hiện tại
    /// </summary>
    public int GetTotalExp() => totalExp;

    /// <summary>
    /// Reset coin và exp
    /// </summary>
    public void ResetMoney()
    {
        totalCoins = 0;
        totalExp = 0;
        RefreshUI();
        Debug.Log("[UIMoney] Money reset");
    }
}
