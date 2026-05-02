using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Enum cho loại item
/// </summary>
public enum ItemType
{
    None = 0,
    HealthPotion = 1,      // Hồi máu
    ManaPotion = 2,        // Hồi mana
    StrengthBuff = 3,      // Tăng lực
    DefenseBuff = 4,       // Bình phòng thủ
    Sword = 5,             // Kiếm
    Bow = 6,               // Cung
    EXPGem = 7,            // Tinh thể EXP
    Coin = 8,              // Xu
    EarthCrystal = 9,      // Tinh thể đất
    WindCrystal = 10,      // Tinh thể khí
    WaterCrystal = 11,     // Tinh thể nước
    FireCrystal = 12       // Tinh thể lửa
}

/// <summary>
/// Dữ liệu của từng item
/// </summary>
[System.Serializable]
public class ItemData
{
    [SerializeField] public int itemId;
    [SerializeField] public string itemName;
    [SerializeField] public ItemType itemType;
    [SerializeField] public string description;

    // Cho Potion (Hồi máu/mana)
    [SerializeField] public int healAmount;              // 30 (cho 30%hp hoặc 30 hp)
    [SerializeField] public bool isPercentage;           // true = %, false = flat value

    // Cho Buff (Tăng lực/phòng thủ)
    [SerializeField] public int buffValue;               // 30 (cho 30%)
    [SerializeField] public float buffDuration = 10f;    // 10 giây

    // Cho Weapon (Kiếm/Cung)
    [SerializeField] public int damageBonus;             // 20 (cho 20%)

    // Cho EXP Gem
    [SerializeField] public int expMin;                  // 10
    [SerializeField] public int expMax;                  // 100

    // Cho Element Crystal
    [SerializeField] public ElementType element = ElementType.None;

    public ItemData() { }

    public ItemData(int itemId, string itemName, ItemType itemType, string description)
    {
        this.itemId = itemId;
        this.itemName = itemName;
        this.itemType = itemType;
        this.description = description;
    }
}

/// <summary>
/// ScriptableObject chứa cấu hình tất cả items
/// </summary>
[CreateAssetMenu(fileName = "ItemConfig", menuName = "EntityData/Item")]
public class ItemSO : ScriptableObject
{
    [SerializeField] private List<ItemData> items = new();

    private Dictionary<int, ItemData> itemLookup;
    private Dictionary<string, ItemData> itemNameLookup;
    private Dictionary<ItemType, List<ItemData>> itemsByType;
    private bool isInitialized = false;

    private void Init()
    {
        if (isInitialized) return;

        itemLookup = new Dictionary<int, ItemData>();
        itemNameLookup = new Dictionary<string, ItemData>();
        itemsByType = new Dictionary<ItemType, List<ItemData>>();

        // Initialize type lists
        foreach (ItemType type in System.Enum.GetValues(typeof(ItemType)))
        {
            itemsByType[type] = new List<ItemData>();
        }

        if (items == null || items.Count == 0)
        {
            //Debug.LogWarning($"ItemSO '{name}': No item data configured!");
            return;
        }

        foreach (var item in items)
        {
            if (item == null)
            {
                //Debug.LogWarning($"ItemSO '{name}': Null item data found, skipping.");
                continue;
            }

            // Kiểm tra duplicate ID
            if (itemLookup.ContainsKey(item.itemId))
            {
                //Debug.LogWarning($"ItemSO '{name}': Duplicate item ID '{item.itemId}', skipping.");
                continue;
            }

            // Kiểm tra duplicate name
            if (!string.IsNullOrEmpty(item.itemName) && itemNameLookup.ContainsKey(item.itemName))
            {
                //Debug.LogWarning($"ItemSO '{name}': Duplicate item name '{item.itemName}', skipping.");
                continue;
            }

            itemLookup.Add(item.itemId, item);

            if (!string.IsNullOrEmpty(item.itemName))
            {
                itemNameLookup.Add(item.itemName, item);
            }

            // Add to type list
            if (item.itemType != ItemType.None && itemsByType.ContainsKey(item.itemType))
            {
                itemsByType[item.itemType].Add(item);
            }
        }

        isInitialized = true;
    }

    /// <summary>
    /// Lấy thông tin item theo ID
    /// </summary>
    public ItemData GetItem(int itemId)
    {
        Init();

        if (itemLookup.TryGetValue(itemId, out ItemData item))
        {
            return item;
        }

        //Debug.LogWarning($"ItemSO '{name}': Item ID {itemId} not found!");
        return null;
    }

    /// <summary>
    /// Lấy thông tin item theo tên
    /// </summary>
    public ItemData GetItemByName(string itemName)
    {
        Init();

        if (itemNameLookup.TryGetValue(itemName, out ItemData item))
        {
            return item;
        }

        //Debug.LogWarning($"ItemSO '{name}': Item '{itemName}' not found!");
        return null;
    }

    /// <summary>
    /// Lấy tất cả items của một loại
    /// </summary>
    public List<ItemData> GetItemsByType(ItemType type)
    {
        Init();

        if (itemsByType.TryGetValue(type, out List<ItemData> typeItems))
        {
            return new List<ItemData>(typeItems);
        }

        return new List<ItemData>();
    }

    /// <summary>
    /// Sử dụng Health Potion
    /// </summary>
    public int UseHealthPotion(int itemId, int currentHealth, int maxHealth)
    {
        var item = GetItem(itemId);
        if (item == null || item.itemType != ItemType.HealthPotion) return currentHealth;

        int healAmount = item.isPercentage
            ? Mathf.RoundToInt(maxHealth * item.healAmount / 100f)
            : item.healAmount;

        return Mathf.Min(currentHealth + healAmount, maxHealth);
    }

    /// <summary>
    /// Sử dụng Mana Potion
    /// </summary>
    public int UseManaPotion(int itemId, int currentMana, int maxMana)
    {
        var item = GetItem(itemId);
        if (item == null || item.itemType != ItemType.ManaPotion) return currentMana;

        int manaAmount = item.isPercentage
            ? Mathf.RoundToInt(maxMana * item.healAmount / 100f)
            : item.healAmount;

        return Mathf.Min(currentMana + manaAmount, maxMana);
    }

    /// <summary>
    /// Lấy buff value (damage/defense)
    /// </summary>
    public int GetBuffValue(int itemId)
    {
        var item = GetItem(itemId);
        return item?.buffValue ?? 0;
    }

    /// <summary>
    /// Lấy buff duration
    /// </summary>
    public float GetBuffDuration(int itemId)
    {
        var item = GetItem(itemId);
        return item?.buffDuration ?? 0f;
    }

    /// <summary>
    /// Lấy damage bonus từ weapon
    /// </summary>
    public int GetWeaponDamageBonus(int itemId)
    {
        var item = GetItem(itemId);
        if (item == null || (item.itemType != ItemType.Sword && item.itemType != ItemType.Bow))
            return 0;

        return item.damageBonus;
    }

    /// <summary>
    /// Lấy random EXP từ EXP Gem
    /// </summary>
    public int GetRandomEXP(int itemId)
    {
        var item = GetItem(itemId);
        if (item == null || item.itemType != ItemType.EXPGem) return 0;

        return Random.Range(item.expMin, item.expMax + 1);
    }

    /// <summary>
    /// Lấy element từ Crystal
    /// </summary>
    public ElementType GetCrystalElement(int itemId)
    {
        var item = GetItem(itemId);
        if (item == null)
            return ElementType.None;

        return item.itemType switch
        {
            ItemType.EarthCrystal => ElementType.Earth,
            ItemType.WindCrystal => ElementType.Wind,
            ItemType.WaterCrystal => ElementType.Water,
            ItemType.FireCrystal => ElementType.Fire,
            _ => ElementType.None
        };
    }

    /// <summary>
    /// Lấy tất cả items
    /// </summary>
    public List<ItemData> GetAllItems()
    {
        Init();
        return new List<ItemData>(items);
    }

    /// <summary>
    /// Kiểm tra item tồn tại
    /// </summary>
    public bool ItemExists(int itemId)
    {
        Init();
        return itemLookup.ContainsKey(itemId);
    }

    /// <summary>
    /// Lấy số lượng items
    /// </summary>
    public int GetItemCount()
    {
        Init();
        return itemLookup.Count;
    }

    /// <summary>
    /// Lấy số lượng items theo loại
    /// </summary>
    public int GetItemCountByType(ItemType type)
    {
        return GetItemsByType(type).Count;
    }
}