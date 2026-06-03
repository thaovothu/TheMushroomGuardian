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

    /// <summary>Sử dụng item.
    /// - Potion/Buff: áp dụng hiệu ứng và tiêu thụ 1 cái.
    /// - Weapon (Sword/Bow): trang bị mà KHÔNG tiêu thụ.
    /// - Element/Crystal: không làm gì (không tiêu thụ).
    /// </summary>
    public void UseItem(ItemType type)
    {
        if (!inventory.TryGetValue(type, out var entry)) return;

        if (type == ItemType.Sword || type == ItemType.Bow)
        {
            var weaponType = type == ItemType.Sword ? WeaponType.Sword : WeaponType.Bow;
            EquipmentSystem.Instance?.EquipFromInventory(weaponType);
            return;
        }

        if (type == ItemType.EarthCrystal || type == ItemType.WindCrystal ||
            type == ItemType.WaterCrystal || type == ItemType.FireCrystal)
            return;

        ApplyItemEffect(entry.data);
        RemoveItem(type, 1);
    }

    private void ApplyItemEffect(ItemData data)
    {
        var player = GameObject.FindWithTag("Player");
        if (player == null)
        {
            Debug.LogWarning("[InventorySystem] Player not found!");
            return;
        }

        switch (data.itemType)
        {
            case ItemType.HealthPotion:
                var healthSys = player.GetComponent<HealthSystem>();
                if (healthSys != null)
                {
                    float healAmt = data.isPercentage
                        ? healthSys.MaxHealth * data.healAmount / 100f
                        : data.healAmount;
                    healthSys.Recover(healAmt);
                    Debug.Log($"[InventorySystem] ✓ Used {data.itemName}: Healed {healAmt} HP");
                }
                break;

            case ItemType.ManaPotion:
                var skillCtrl = player.GetComponent<PlayerSkillController>();
                if (skillCtrl != null)
                {
                    float manaAmt = data.isPercentage
                        ? skillCtrl.GetMaxMana() * data.healAmount / 100f
                        : data.healAmount;
                    skillCtrl.RecoverMana(manaAmt);
                    Debug.Log($"[InventorySystem] ✓ Used {data.itemName}: Recovered {manaAmt} Mana");
                }
                break;

            case ItemType.StrengthBuff:
                var playerCtrl = player.GetComponent<PlayerController>();
                if (playerCtrl != null)
                {
                    playerCtrl.AddAttackBuff(data.buffValue, data.buffDuration);
                    Debug.Log($"[InventorySystem] ✓ Used {data.itemName}: Attack +{data.buffValue}% for {data.buffDuration}s");
                }
                break;

            case ItemType.DefenseBuff:
                var healthForBuff = player.GetComponent<HealthSystem>();
                if (healthForBuff != null)
                {
                    healthForBuff.AddDefenseBuff(data.buffValue, data.buffDuration);
                    Debug.Log($"[InventorySystem] ✓ Used {data.itemName}: Defense +{data.buffValue}% for {data.buffDuration}s");
                }
                break;

            default:
                Debug.Log($"[InventorySystem] Using {data.itemName} — no effect defined for {data.itemType}");
                break;
        }
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

    /// <summary>Trả về bản sao của inventory để lưu checkpoint.</summary>
    public Dictionary<ItemType, (ItemData data, int quantity)> GetInventorySnapshot()
    {
        return new Dictionary<ItemType, (ItemData, int)>(inventory);
    }

    /// <summary>Khôi phục inventory về snapshot đã lưu — dùng khi respawn sau khi chết.</summary>
    public void RestoreInventory(Dictionary<ItemType, (ItemData data, int quantity)> snapshot)
    {
        inventory = new Dictionary<ItemType, (ItemData, int)>(snapshot);
        GameEvent.Inventory.OnSlotChanged?.Invoke(0, 0);
        Debug.Log($"[InventorySystem] Inventory restored: {inventory.Count} item type(s)");
    }

    // ── Legacy support ────────────────────────────────────────────────────────
    public int GetBagCount() => 1;
    public bool IsFull() => false;
    public int GetEmptySlotCount() => 999;
}