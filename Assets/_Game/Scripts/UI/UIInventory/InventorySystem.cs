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
    [SerializeField] private InventoryBag mainBag;      // Túi chính của player

    // Event khi có thay đổi trong inventory
    public delegate void InventoryChangedDelegate(int slotIndex);
    public event InventoryChangedDelegate OnItemAdded;
    public event InventoryChangedDelegate OnItemRemoved;
    public event InventoryChangedDelegate OnSlotChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Khởi tạo túi chính
        mainBag = new InventoryBag("Main Bag");
        Debug.Log("[InventorySystem] Initialized with main bag of 20 slots");
    }

    private void OnValidate()
    {
        if (itemSO == null)
        {
            Debug.LogWarning("[InventorySystem] ItemSO not assigned!");
        }
    }

    /// <summary>
    /// Thêm item vào inventory
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
        if (!mainBag.HasEmptySlot())
        {
            Debug.LogWarning("[InventorySystem] Inventory is full!");
            return false;
        }

        // Tìm slot trống và thêm item
        for (int i = 0; i < mainBag.SlotCount; i++)
        {
            if (mainBag.GetSlot(i).IsEmpty)
            {
                mainBag.GetSlot(i).AddItem(itemData, quantity);
                OnItemAdded?.Invoke(i);
                OnSlotChanged?.Invoke(i);
                Debug.Log($"[InventorySystem] Item '{itemData.itemName}' added to slot {i}");
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Lấy item từ inventory
    /// </summary>
    public bool RemoveItem(int slotIndex, int quantity = 1)
    {
        InventorySlot slot = mainBag.GetSlot(slotIndex);
        if (slot == null || slot.IsEmpty)
        {
            Debug.LogWarning($"[InventorySystem] Cannot remove item from empty slot {slotIndex}");
            return false;
        }

        slot.RemoveItem(quantity);
        OnItemRemoved?.Invoke(slotIndex);
        OnSlotChanged?.Invoke(slotIndex);
        Debug.Log($"[InventorySystem] Item removed from slot {slotIndex}");
        return true;
    }

    /// <summary>
    /// Sử dụng item
    /// </summary>
    public void UseItem(int slotIndex)
    {
        InventorySlot slot = mainBag.GetSlot(slotIndex);
        if (slot == null || slot.IsEmpty)
        {
            Debug.LogWarning($"[InventorySystem] Cannot use item from empty slot {slotIndex}");
            return;
        }

        ItemData itemData = slot.ItemData;
        Debug.Log($"[InventorySystem] Using item: {itemData.itemName}");

        // TODO: Xử lý logic sử dụng item dựa vào ItemType
        // Ví dụ: HealthPotion -> heal player, StrengthBuff -> apply buff, v.v.

        // Sau khi dùng, lấy item khỏi inventory
        RemoveItem(slotIndex, 1);
    }

    /// <summary>
    /// Xoá item từ slot
    /// </summary>
    public void DeleteItem(int slotIndex)
    {
        mainBag.ClearSlot(slotIndex);
        OnSlotChanged?.Invoke(slotIndex);
        Debug.Log($"[InventorySystem] Item deleted from slot {slotIndex}");
    }

    /// <summary>
    /// Lấy slot tại vị trí
    /// </summary>
    public InventorySlot GetSlot(int slotIndex)
    {
        return mainBag.GetSlot(slotIndex);
    }

    /// <summary>
    /// Lấy túi chính
    /// </summary>
    public InventoryBag GetMainBag()
    {
        return mainBag;
    }

    /// <summary>
    /// Kiểm tra inventory còn trống không
    /// </summary>
    public bool IsFull()
    {
        return !mainBag.HasEmptySlot();
    }

    /// <summary>
    /// Lấy số slot trống
    /// </summary>
    public int GetEmptySlotCount()
    {
        return mainBag.GetEmptySlotCount();
    }

    /// <summary>
    /// Xoá toàn bộ inventory
    /// </summary>
    public void ClearInventory()
    {
        mainBag.ClearAll();
        Debug.Log("[InventorySystem] Inventory cleared");
    }
}
