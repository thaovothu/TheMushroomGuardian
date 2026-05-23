using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 1 slot item trong UIShop.
/// Lấy icon từ ItemIconSO theo itemId.
/// </summary>
public class UIShopItemSlot : MonoBehaviour
{
    [SerializeField] private Image itemIcon;
    [SerializeField] private TextMeshProUGUI itemNameText;
    [SerializeField] private TextMeshProUGUI priceText;
    [SerializeField] private TextMeshProUGUI stockText;
    [SerializeField] private Button buyButton;

    private int itemId;
    private int price;
    private int stock;
    private Action<int, int> onBuy;

    public void Setup(int id, int itemPrice, int itemStock, Action<int, int> buyCallback, ItemIconSO iconSO, ItemSO itemSO)
    {
        itemId = id;
        price = itemPrice;
        stock = itemStock;
        onBuy = buyCallback;

        // Lấy icon từ ItemIconSO
        if (itemIcon != null && iconSO != null)
            itemIcon.sprite = iconSO.GetIcon(id);

        // Lấy tên item từ ItemSO
        if (itemNameText != null && itemSO != null)
        {
            var data = itemSO.GetItem(id);
            itemNameText.text = data != null ? data.itemName : $"Item {id}";
        }

        if (priceText != null) priceText.text = $"{price} xu";
        if (stockText != null) stockText.text = stock == -1 ? "∞" : $"x{stock}";

        if (buyButton != null)
        {
            buyButton.onClick.RemoveAllListeners();
            buyButton.onClick.AddListener(() => onBuy?.Invoke(itemId, price));
        }
    }
}