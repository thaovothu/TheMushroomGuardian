using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Hệ thống quản lý Inventory toàn cầu
/// Singleton pattern - chỉ có 1 instance duy nhất
/// </summary>
public class InventorySystem : MonoBehaviour
{
    public static InventorySystem Instance { get; private set; }

    [SerializeField] private ItemSO itemSO;              // Cấu hình tất cả items
    [SerializeField] private ItemIconSO itemIconSO;      // Icon 2D của items

    private List<InventoryBag> bags = new();
    private const int BAG_COUNT = 3;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Khởi tạo 3 túi
        for (int i = 0; i < BAG_COUNT; i++)
        {
            bags.Add(new InventoryBag($"Bag {i + 1}"));
        }
        Debug.Log("[InventorySystem] Initialized with 3 bags of 9 slots each");
    }

    private void OnValidate()
    {
        if (itemSO == null)
        {
            Debug.LogWarning("[InventorySystem] ItemSO not assigned!");
        }
    }

    /// <summary>
    /// Thêm item vào inventory (túi đầu tiên có chỗ trống)
    /// </summary>
    public bool AddItem(int itemId, int quantity = 1)
    {
        if (itemSO == null)
        {
            Debug.LogError("[InventorySystem] ItemSO not assigned!");
            return false;
        }

        ItemData itemData = itemSO.GetItem(itemId);
        if (itemData == null)
        {
            Debug.LogError($"[InventorySystem] Item with ID {itemId} not found!");
            return false;
        }

        return AddItem(itemData, quantity);
    }

    /// <summary>
    /// Thêm item vào inventory (từ ItemData)
    /// </summary>
    public bool AddItem(ItemData itemData, int quantity = 1)
    {
        // Thử thêm vào túi đầu tiên có chỗ trống
        for (int bagIndex = 0; bagIndex < bags.Count; bagIndex++)
        {
            if (bags[bagIndex].HasEmptySlot())
            {
                for (int i = 0; i < bags[bagIndex].SlotCount; i++)
                {
                    if (bags[bagIndex].GetSlot(i).IsEmpty)
                    {
                        bags[bagIndex].GetSlot(i).AddItem(itemData, quantity);
                        GameEvent.Inventory.OnItemAdded?.Invoke(bagIndex, i);
                        GameEvent.Inventory.OnSlotChanged?.Invoke(bagIndex, i);
                        Debug.Log($"[InventorySystem] Item '{itemData.itemName}' added to bag {bagIndex} slot {i}");
                        return true;
                    }
                }
            }
        }

        Debug.LogWarning("[InventorySystem] All bags are full!");
        return false;
    }

    /// <summary>
    /// Thêm item vào túi cụ thể
    /// </summary>
    public bool AddItemToBag(int bagIndex, ItemData itemData, int quantity = 1)
    {
        if (bagIndex < 0 || bagIndex >= bags.Count)
        {
            Debug.LogError($"[InventorySystem] Invalid bag index: {bagIndex}");
            return false;
        }

        if (!bags[bagIndex].HasEmptySlot())
        {
            Debug.LogWarning($"[InventorySystem] Bag {bagIndex} is full!");
            return false;
        }

        for (int i = 0; i < bags[bagIndex].SlotCount; i++)
        {
            if (bags[bagIndex].GetSlot(i).IsEmpty)
            {
                bags[bagIndex].GetSlot(i).AddItem(itemData, quantity);
                GameEvent.Inventory.OnItemAdded?.Invoke(bagIndex, i);
                GameEvent.Inventory.OnSlotChanged?.Invoke(bagIndex, i);
                Debug.Log($"[InventorySystem] Item '{itemData.itemName}' added to bag {bagIndex} slot {i}");
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Lấy item từ inventory
    /// </summary>
    public bool RemoveItem(int bagIndex, int slotIndex, int quantity = 1)
    {
        if (bagIndex < 0 || bagIndex >= bags.Count)
        {
            Debug.LogError($"[InventorySystem] Invalid bag index: {bagIndex}");
            return false;
        }

        InventorySlot slot = bags[bagIndex].GetSlot(slotIndex);
        if (slot == null || slot.IsEmpty)
        {
            Debug.LogWarning($"[InventorySystem] Cannot remove item from empty slot {slotIndex} in bag {bagIndex}");
            return false;
        }

        slot.RemoveItem(quantity);
        GameEvent.Inventory.OnItemRemoved?.Invoke(bagIndex, slotIndex);
        GameEvent.Inventory.OnSlotChanged?.Invoke(bagIndex, slotIndex);
        Debug.Log($"[InventorySystem] Item removed from bag {bagIndex} slot {slotIndex}");
        return true;
    }

    /// <summary>
    /// Sử dụng item
    /// </summary>
    public void UseItem(int bagIndex, int slotIndex)
    {
        if (bagIndex < 0 || bagIndex >= bags.Count)
        {
            Debug.LogError($"[InventorySystem] Invalid bag index: {bagIndex}");
            return;
        }

        InventorySlot slot = bags[bagIndex].GetSlot(slotIndex);
        if (slot == null || slot.IsEmpty)
        {
            Debug.LogWarning($"[InventorySystem] Cannot use item from empty slot {slotIndex} in bag {bagIndex}");
            return;
        }

        ItemData itemData = slot.ItemData;
        Debug.Log($"[InventorySystem] Using item: {itemData.itemName}");

        // TODO: Xử lý logic sử dụng item dựa vào ItemType

        // Sau khi dùng, lấy item khỏi inventory
        RemoveItem(bagIndex, slotIndex, 1);
    }

    /// <summary>
    /// Xoá item từ slot
    /// </summary>
    public void DeleteItem(int bagIndex, int slotIndex)
    {
        if (bagIndex < 0 || bagIndex >= bags.Count)
        {
            Debug.LogError($"[InventorySystem] Invalid bag index: {bagIndex}");
            return;
        }

        bags[bagIndex].ClearSlot(slotIndex);
        GameEvent.Inventory.OnSlotChanged?.Invoke(bagIndex, slotIndex);
        Debug.Log($"[InventorySystem] Item deleted from bag {bagIndex} slot {slotIndex}");
    }

    /// <summary>
    /// Lấy slot tại vị trí
    /// </summary>
    public InventorySlot GetSlot(int bagIndex, int slotIndex)
    {
        if (bagIndex < 0 || bagIndex >= bags.Count)
        {
            Debug.LogError($"[InventorySystem] Invalid bag index: {bagIndex}");
            return null;
        }

        return bags[bagIndex].GetSlot(slotIndex);
    }

    /// <summary>
    /// Lấy túi tại chỉ số
    /// </summary>
    public InventoryBag GetBag(int bagIndex)
    {
        if (bagIndex < 0 || bagIndex >= bags.Count)
        {
            Debug.LogError($"[InventorySystem] Invalid bag index: {bagIndex}");
            return null;
        }

        return bags[bagIndex];
    }

    /// <summary>
    /// Lấy số lượng túi
    /// </summary>
    public int GetBagCount()
    {
        return bags.Count;
    }

    /// <summary>
    /// Kiểm tra inventory còn trống không
    /// </summary>
    public bool IsFull()
    {
        // Kiểm tra tất cả túi
        foreach (var bag in bags)
        {
            if (bag.HasEmptySlot())
                return false;
        }
        return true;
    }

    /// <summary>
    /// Lấy tổng số slot trống
    /// </summary>
    public int GetEmptySlotCount()
    {
        int total = 0;
        foreach (var bag in bags)
        {
            total += bag.GetEmptySlotCount();
        }
        return total;
    }

    /// <summary>
    /// Xoá toàn bộ inventory
    /// </summary>
    public void ClearInventory()
    {
        foreach (var bag in bags)
        {
            bag.ClearAll();
        }
        Debug.Log("[InventorySystem] Inventory cleared");
    }

    /// <summary>
    /// Lấy icon của item
    /// </summary>
    public Sprite GetItemIcon(int itemId)
    {
        if (itemIconSO == null)
        {
            Debug.LogWarning("[InventorySystem] ItemIconSO not assigned!");
            return null;
        }

        return itemIconSO.GetIcon(itemId);
    }
}
