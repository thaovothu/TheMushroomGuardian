using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Inventory lưu trữ theo ItemType — không dùng bag/slot vật lý.
/// Mỗi ItemType có 1 entry với số lượng tương ứng.
/// </summary>
public class InventorySystem : BaseSingleton<InventorySystem>
{
    [SerializeField] private ItemSO itemSO;
    [SerializeField] private ItemIconSO itemIconSO;

    // key = ItemType, value = (ItemData, quantity)
    private Dictionary<ItemType, (ItemData data, int quantity)> inventory
        = new Dictionary<ItemType, (ItemData, int)>();

    protected override void Awake()
    {
        base.Awake();
    }

    // ── Public API ────────────────────────────────────────────────────────────

    /// <summary>Thêm item theo itemId.</summary>
    public bool AddItem(int itemId, int quantity = 1)
    {
        if (itemSO == null) { Debug.LogError("[InventorySystem] ItemSO not assigned!"); return false; }

        // Coin và EXPGem không vào inventory
        if (itemId == 8 || itemId == 7) return false;

        var data = itemSO.GetItem(itemId);
        if (data == null) { Debug.LogWarning($"[InventorySystem] Item ID {itemId} not found!"); return false; }

        return AddItem(data, quantity);
    }

    /// <summary>Thêm item theo ItemData.</summary>
    public bool AddItem(ItemData itemData, int quantity = 1)
    {
        if (itemData == null) return false;

        var type = itemData.itemType;

        if (inventory.ContainsKey(type))
        {
            var entry = inventory[type];
            inventory[type] = (entry.data, entry.quantity + quantity);
        }
        else
        {
            inventory[type] = (itemData, quantity);
        }

        Debug.Log($"[InventorySystem] Added {itemData.itemName} x{quantity} — total: {inventory[type].quantity}");

        GameEvent.Inventory.OnItemAdded?.Invoke(0, 0);
        GameEvent.Inventory.OnSlotChanged?.Invoke(0, 0);
        return true;
    }

    /// <summary>Lấy số lượng theo ItemType.</summary>
    public int GetItemQuantity(ItemType type)
    {
        return inventory.TryGetValue(type, out var entry) ? entry.quantity : 0;
    }

    /// <summary>Lấy icon theo ItemType.</summary>
    public Sprite GetItemIconByType(ItemType type)
    {
        if (itemIconSO == null) return null;
        return itemIconSO.GetIcon((int)type);
    }

    /// <summary>Lấy icon theo itemId.</summary>
    public Sprite GetItemIcon(int itemId)
    {
        if (itemIconSO == null) return null;
        return itemIconSO.GetIcon(itemId);
    }

    /// <summary>Kiểm tra có item theo itemId không.</summary>
    public bool HasItem(int itemId)
    {
        var type = (ItemType)itemId;
        return inventory.TryGetValue(type, out var entry) && entry.quantity > 0;
    }

    /// <summary>Xóa item theo ItemType.</summary>
    public bool RemoveItem(ItemType type, int quantity = 1)
    {
        if (!inventory.TryGetValue(type, out var entry)) return false;

        int newQty = entry.quantity - quantity;
        if (newQty <= 0)
            inventory.Remove(type);
        else
            inventory[type] = (entry.data, newQty);

        GameEvent.Inventory.OnItemRemoved?.Invoke(0, 0);
        GameEvent.Inventory.OnSlotChanged?.Invoke(0, 0);
        return true;
    }

    /// <summary>Sử dụng item.</summary>
    public void UseItem(ItemType type)
    {
        if (!inventory.TryGetValue(type, out var entry)) return;
        Debug.Log($"[InventorySystem] Using {entry.data.itemName}");
        RemoveItem(type, 1);
    }

    /// <summary>Xóa toàn bộ inventory.</summary>
    public void ClearInventory()
    {
        inventory.Clear();
        Debug.Log("[InventorySystem] Inventory cleared");
    }

    public ItemData GetItemDataByType(ItemType type)
    {
        if (inventory.TryGetValue(type, out var entry))
            return entry.data;
        return null;
    }
    // ── Legacy support ────────────────────────────────────────────────────────
    public int GetBagCount() => 1;
    public bool IsFull() => false;
    public int GetEmptySlotCount() => 999;
}