using UnityEngine;

/// <summary>
/// Đại diện cho 1 slot trong inventory
/// Chứa thông tin của item trong slot đó
/// </summary>
[System.Serializable]
public class InventorySlot
{
    [SerializeField] private int itemId;              // ID của item
    [SerializeField] private int quantity;           // Số lượng item
    [SerializeField] private ItemData itemData;      // Thông tin chi tiết của item

    public int ItemId => itemId;
    public int Quantity => quantity;
    public ItemData ItemData => itemData;
    public bool IsEmpty => itemId == 0 || quantity == 0;

    public InventorySlot()
    {
        itemId = 0;
        quantity = 0;
        itemData = null;
    }

    public InventorySlot(ItemData data, int qty = 1)
    {
        itemId = data.itemId;
        quantity = qty;
        itemData = data;
    }

    /// <summary>
    /// Thêm item vào slot
    /// </summary>
    public void AddItem(ItemData data, int qty = 1)
    {
        itemId = data.itemId;
        itemData = data;
        quantity += qty;
    }

    /// <summary>
    /// Lấy ra số lượng item từ slot
    /// </summary>
    public int RemoveItem(int qty)
    {
        int removed = Mathf.Min(qty, quantity);
        quantity -= removed;

        if (quantity <= 0)
        {
            Clear();
        }

        return removed;
    }

    /// <summary>
    /// Xoá item khỏi slot
    /// </summary>
    public void Clear()
    {
        itemId = 0;
        quantity = 0;
        itemData = null;
    }

    /// <summary>
    /// Thiết lập số lượng item
    /// </summary>
    public void SetQuantity(int qty)
    {
        quantity = Mathf.Max(0, qty);
        if (quantity == 0)
        {
            Clear();
        }
    }
}
