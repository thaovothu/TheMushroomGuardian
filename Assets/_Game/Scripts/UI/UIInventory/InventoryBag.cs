using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Đại diện cho 1 túi trong inventory
/// Chứa 20 slots để lưu trữ items
/// </summary>
[System.Serializable]
public class InventoryBag
{
    public const int MAX_SLOTS = 20;
    
    [SerializeField] private List<InventorySlot> slots = new();
    [SerializeField] private string bagName = "Main Bag";

    public string BagName => bagName;
    public int SlotCount => slots.Count;
    public List<InventorySlot> Slots => slots;

    public InventoryBag(string name = "Main Bag")
    {
        bagName = name;
        slots = new List<InventorySlot>();

        // Khởi tạo 20 slots rỗng
        for (int i = 0; i < MAX_SLOTS; i++)
        {
            slots.Add(new InventorySlot());
        }
    }

    /// <summary>
    /// Thêm item vào túi
    /// Trả về true nếu thêm thành công, false nếu túi đầy
    /// </summary>
    public bool AddItem(ItemData itemData, int quantity = 1)
    {
        if (itemData == null)
        {
            Debug.LogWarning("[InventoryBag] Cannot add null item data");
            return false;
        }

        // Tìm slot trống đầu tiên
        for (int i = 0; i < slots.Count; i++)
        {
            if (slots[i].IsEmpty)
            {
                slots[i] = new InventorySlot(itemData, quantity);
                Debug.Log($"[InventoryBag] Item '{itemData.itemName}' added to slot {i}");
                return true;
            }
        }

        Debug.LogWarning("[InventoryBag] Inventory is full!");
        return false;
    }

    /// <summary>
    /// Lấy item từ túi
    /// </summary>
    public bool RemoveItem(int slotIndex, int quantity = 1)
    {
        if (slotIndex < 0 || slotIndex >= slots.Count)
        {
            Debug.LogWarning($"[InventoryBag] Invalid slot index: {slotIndex}");
            return false;
        }

        if (slots[slotIndex].IsEmpty)
        {
            Debug.LogWarning($"[InventoryBag] Slot {slotIndex} is empty");
            return false;
        }

        slots[slotIndex].RemoveItem(quantity);
        Debug.Log($"[InventoryBag] Item removed from slot {slotIndex}");
        return true;
    }

    /// <summary>
    /// Lấy item từ slot
    /// </summary>
    public InventorySlot GetSlot(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= slots.Count)
        {
            Debug.LogWarning($"[InventoryBag] Invalid slot index: {slotIndex}");
            return null;
        }

        return slots[slotIndex];
    }

    /// <summary>
    /// Kiểm tra còn slot trống không
    /// </summary>
    public bool HasEmptySlot()
    {
        foreach (var slot in slots)
        {
            if (slot.IsEmpty)
                return true;
        }
        return false;
    }

    /// <summary>
    /// Lấy số lượng slot trống
    /// </summary>
    public int GetEmptySlotCount()
    {
        int count = 0;
        foreach (var slot in slots)
        {
            if (slot.IsEmpty)
                count++;
        }
        return count;
    }

    /// <summary>
    /// Xoá slot
    /// </summary>
    public void ClearSlot(int slotIndex)
    {
        if (slotIndex >= 0 && slotIndex < slots.Count)
        {
            slots[slotIndex].Clear();
        }
    }

    /// <summary>
    /// Xoá toàn bộ túi
    /// </summary>
    public void ClearAll()
    {
        foreach (var slot in slots)
        {
            slot.Clear();
        }
    }
}
