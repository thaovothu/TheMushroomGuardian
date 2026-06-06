using UnityEngine;
using TMPro;

/// <summary>
/// Hiển thị coin và exp. Đặt dưới UIInventory trong hierarchy — không phải singleton.
/// Dữ liệu lưu dưới dạng static để các hệ thống khác gọi UIMoney.AddCoin() mà không cần instance.
/// </summary>
public class UIMoney : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI coinText;
    // [SerializeField] private TextMeshProUGUI expText;

    public static int TotalCoins { get; private set; }
    // public static int TotalExp   { get; private set; }

    private static UIMoney _ui;

    private void Awake()
    {
        _ui = this;
    }

    private void OnDestroy()
    {
        if (_ui == this) _ui = null;
    }

    private void Start() => RefreshUI();

    // ── Public static API ─────────────────────────────────────────────────────

    public static void AddCoin(int amount)
    {
        if (amount == 0) return;
        TotalCoins += amount;
        if (_ui != null) _ui.RefreshCoinText();
        Debug.Log($"[UIMoney] +{amount} coin (Total: {TotalCoins})");
    }

    // public static void AddExp(int amount)
    // {
    //     if (amount == 0) return;
    //     TotalExp += amount;
    //     if (_ui != null) _ui.RefreshExpText();
    //     Debug.Log($"[UIMoney] +{amount} exp (Total: {TotalExp})");
    // }

    public static void RestoreCoins(int amount)
    {
        TotalCoins = amount;
        if (_ui != null) _ui.RefreshCoinText();
        Debug.Log($"[UIMoney] Coins restored to {TotalCoins}");
    }

    public static void ResetMoney()
    {
        TotalCoins = 0;
        // TotalExp   = 0;
        if (_ui != null) _ui.RefreshUI();
        Debug.Log("[UIMoney] Money reset");
    }

    // ── Private UI refresh ────────────────────────────────────────────────────

    private void RefreshCoinText()
    {
        if (coinText != null) coinText.text = $"{TotalCoins}";
    }

    // private void RefreshExpText()
    // {
    //     if (expText != null) expText.text = $"EXP: {TotalExp}";
    // }

    private void RefreshUI()
    {
        RefreshCoinText();
        // RefreshExpText();
    }
}
